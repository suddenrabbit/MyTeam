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
        // GET: /ReqSurvey/

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
        // GET: /ReqSurvey/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /ReqSurvey/Create

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
        // POST: /ReqSurvey/Create

        [HttpPost]
        public ActionResult Create(ProjSurv reqSurv)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.ProjSurvs.Add(reqSurv);
                    dbContext.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        //
        // GET: /ReqSurvey/Edit/5

        public ActionResult Edit(int id)
        {

            //项目列表
            List<Proj> ls1 = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls1, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;

            // 调研方式列表
            ViewBag.SurveyWayList = MyTools.GetSelectList(Constants.SurveyWayList);

            List<ProjSurv> ls2 = dbContext.ProjSurvs.ToList();
            ProjSurv reqSur = ls2.Find(a => a.SurvID == id);

            if (reqSur == null)
            {
                ModelState.AddModelError("", "不存在该需求调研记录！");
            }

            return View(reqSur);
        }

        //
        // POST: /ReqSurvey/Edit/5

        [HttpPost]
        public ActionResult Edit(ProjSurv reqSurv)
        {
            try
            {
                dbContext.Entry(reqSurv).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        //
        // GET: /ReqSurvey/Delete/5

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
