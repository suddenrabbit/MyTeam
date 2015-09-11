using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;
using PagedList;

namespace MyTeam.Controllers
{
    public class ProjPlanController : BaseController
    {
        //
        // GET: /ProjPlan/
        public ActionResult Index(ProjPlanQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {
            if (this.GetSessionCurrentUser() == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/ProjPlan" });
            }

            if (isQuery)
            {
                var ls = from a in dbContext.ProjPlans select a;
                if (query.ProjID != 0)
                {
                    ls = ls.Where(p => p.ProjID == query.ProjID);
                }
                var result = ls.ToList();
                // 分页
                query.ResultList = result.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE);
            }
            else
            {
                query = new ProjPlanQuery();
            }

            // 为了保证查询部分正常显示，对下拉列表处理           
            // 获取项目下拉列表
            List<Proj> projLs = dbContext.Projs.ToList();
            // 加上“全部”
            projLs.Insert(0, new Proj() { ProjID = 0, ProjName = "全部" });
            ViewBag.ProjList = new SelectList(projLs, "ProjID", "ProjName", query.ProjID);

            List<ProjPlan> list = dbContext.ProjPlans.ToList();
            foreach (ProjPlan rs in list)
            {
                Proj p = projLs.Find(a => a.ProjID == rs.ProjID);
                rs.ProjName = p == null ? "未知" : p.ProjName;
            }

            return View(query);
        }

        //
        // GET: /ProjPlan/Create

        public ActionResult Create()
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl = null;
            sl = new SelectList(ls, "ProjID", "ProjName");

            ViewBag.ProjList = sl;

            return View();
        }

        //
        // POST: /ProjPlan/Create

        [HttpPost]
        public string Create(ProjPlan projPlan)
        {
            // 判断是否有重复的项目名称，如有重复不允许新增
            ProjPlan plan = dbContext.ProjPlans.ToList().Find(a => a.ProjID == projPlan.ProjID);
            if (plan != null)
            {
                return "<p class='alert alert-danger'>出错了: " + projPlan.ProjName + "的项目计划表已存在，不允许重复添加！" + "</p>";
            }

            try
            {
                dbContext.ProjPlans.Add(projPlan);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ProjPlan/Edit/5

        public ActionResult Edit(int id)
        {
            List<ProjPlan> ls2 = dbContext.ProjPlans.ToList();
            ProjPlan projPlan = ls2.Find(a => a.PlanID == id);

            if (projPlan == null)
            {
                return View();
            }

            //项目列表
            List<Proj> ls1 = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls1, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;
            projPlan.OldProjID = projPlan.ProjID;

            return View(projPlan);
        }

        //
        // POST: /ProjPlan/Edit/5

        [HttpPost]
        public string Edit(ProjPlan projPlan)
        {
            if (projPlan.ProjID != projPlan.OldProjID)
            {
                // 若项目名称改变，则判断新改的系统名称是否有重复，如有重复不允许新增
                ProjPlan plan = dbContext.ProjPlans.ToList().Find(a => a.ProjName == projPlan.ProjName);
                if (plan != null)
                {
                    return "<p class='alert alert-danger'>出错了: " + projPlan.ProjName + "的项目计划表已存在，不允许更新！" + "</p>";
                }
            } 

            try
            {
                dbContext.Entry(projPlan).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ProjPlan/Delete/5

        public string Delete(int id)
        {
            try
            {
                List<ProjPlan> ls = dbContext.ProjPlans.ToList();
                ProjPlan projPlan = ls.Find(a => a.PlanID == id);

                dbContext.Entry(projPlan).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }
    }
}
