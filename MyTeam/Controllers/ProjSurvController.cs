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
    public class ProjSurvController : BaseController
    {
        //
        // GET: /ProjSurv/

        public ActionResult Index(int pageNum = 1)
        {
            List<ProjSurv> ls = dbContext.ProjSurvs.ToList();
            List<Proj> projLs = dbContext.Projs.ToList();
            foreach(ProjSurv rs in ls)
            {
                Proj p = projLs.Find(a => a.ProjID == rs.ProjID);
                rs.ProjName = p == null ? "未知" : p.ProjName;
            }

            // 分页
            var ls1 = ls.ToPagedList(pageNum, Constants.PAGE_SIZE);
            return View(ls1);
        }

        //
        // GET: /ProjSurv/Create

        public ActionResult Create()
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl = null;
            sl = new SelectList(ls, "ProjID","ProjName");

            ViewBag.ProjList = sl;

            // 调研方式
            ViewBag.SurveyWayList = MyTools.GetSelectList(Constants.SurveyWayList);

            return View();
        }

        //
        // POST: /ProjSurv/Create

        [HttpPost]
        public string Create(ProjSurv reqSurv)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.ProjSurvs.Add(reqSurv);
                    dbContext.SaveChanges();
                }
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ProjSurv/Edit/5

        public ActionResult Edit(int id)
        {
            List<ProjSurv> ls2 = dbContext.ProjSurvs.ToList();
            ProjSurv reqSur = ls2.Find(a => a.SurvID == id);

            if (reqSur == null)
            {
                return View();
            }

            //项目列表
            List<Proj> ls1 = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls1, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;

            // 调研方式列表
            ViewBag.SurveyWayList = MyTools.GetSelectList(Constants.SurveyWayList);
            
            return View(reqSur);
        }

        //
        // POST: /ProjSurv/Edit/5

        [HttpPost]
        public string Edit(ProjSurv reqSurv)
        {
            try
            {
                dbContext.Entry(reqSurv).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ProjSurv/Delete/5

        public string Delete(int id)
        {
            try
            {
                List<ProjSurv> ls = dbContext.ProjSurvs.ToList();
                ProjSurv reqSurv = ls.Find(a => a.SurvID == id);

                dbContext.Entry(reqSurv).State = System.Data.Entity.EntityState.Deleted;
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
