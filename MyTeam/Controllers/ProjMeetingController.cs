using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;

namespace MyTeam.Controllers
{
    public class ProjMeetingController : BaseController
    {
        //
        // GET: /ReqMeeting/

        public ActionResult Index()
        {
            List<ProjMeeting> ls = dbContext.ProjMeetings.ToList();
            List<Proj> projLs = dbContext.Projs.ToList();
            foreach (ProjMeeting rm in ls)
            {
                Proj p = projLs.Find(a => a.ProjID == rm.ProjID);
                rm.ProjName = p == null ? "未知" : p.ProjName;
            }
            return View(ls);
        }

        //
        // GET: /ReqMeeting/Details/5

        public ActionResult Details(int id)
        {
            List<ProjMeeting> ls = dbContext.ProjMeetings.ToList();
            ProjMeeting reqMeeting = ls.Find(a => a.MeetingID == id);

            if (reqMeeting == null)
            {
                ModelState.AddModelError("", "不存在该需求会议记录！");
                reqMeeting = new ProjMeeting();
            }
            return View(reqMeeting);
        }

        //
        // GET: /ReqMeeting/Create

        public ActionResult Create()
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;

            // 会议类型列表
            ViewBag.MeetingTypeList = MyTools.GetSelectList(Constants.MeetingTypeList);

            // 需求会议评审结果列表
            ViewBag.ReviewConclusionList = MyTools.GetSelectList(Constants.ReviewConclusionList);

            // 需求会议当前状态列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.StatList);

            return View();
        }

        //
        // POST: /ReqMeeting/Create

        [HttpPost]
        public ActionResult Create(ProjMeeting reqMeeting)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.ProjMeetings.Add(reqMeeting);
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
        // GET: /ReqMeeting/Edit/5

        public ActionResult Edit(int id)
        {
            // 会议类型列表
            ViewBag.MeetingTypeList = MyTools.GetSelectList(Constants.MeetingTypeList);

            // 需求会议评审结果列表
            ViewBag.ReviewConclusionList = MyTools.GetSelectList(Constants.ReviewConclusionList);

            // 需求会议当前状态列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.StatList);

            List<ProjMeeting> rm = dbContext.ProjMeetings.ToList();
            ProjMeeting reqMeeting = rm.Find(a => a.MeetingID == id);

            if (reqMeeting == null)
            {
                ModelState.AddModelError("", "不存在该需求会议记录！");
            }

            return View(reqMeeting);
        }

        //
        // POST: /ReqMeeting/Edit/5

        [HttpPost]
        public ActionResult Edit(ProjMeeting reqMeeting)
        {
            try
            {
                dbContext.Entry(reqMeeting).State = System.Data.Entity.EntityState.Modified;
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
        // GET: /ReqMeeting/Delete/5

        public string Delete(int id)
        {
            try
            {
                List<ProjMeeting> ls = dbContext.ProjMeetings.ToList();
                ProjMeeting reqMeeting = ls.Find(a => a.MeetingID == id);

                dbContext.Entry(reqMeeting).State = System.Data.Entity.EntityState.Deleted;
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
