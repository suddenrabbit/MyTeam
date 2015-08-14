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
    public class ReqTrackController : BaseController
    {
        //
        // GET: /ReqTrack/

        public ActionResult Index()
        {
            List<ReqTrack> ls = dbContext.ReqTracks.ToList();

            return View(ls);
        }

        //
        // GET: /ReqTrack/Details/5

        public ActionResult Details(int id)
        {
            List<ReqTrack> ls = dbContext.ReqTracks.ToList();
            ReqTrack reqTrack = ls.Find(a => a.TrackID == id);

            if (reqTrack == null)
            {
                ModelState.AddModelError("", "不存在该业需软需状态跟踪记录！");
                reqTrack = new ReqTrack();
            }
            return View(reqTrack);
        }

        //
        // GET: /ReqTrack/Create

        public ActionResult Create()
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl = null;
            sl = new SelectList(ls, "ProjID", "ProjName");

            ViewBag.ProjList = sl;
            // 优先级列表
            ViewBag.PriorityList = MyTools.GetSelectList(Constants.PriorityList);

            // 变更标识列表
            ViewBag.ChangeCharList = MyTools.GetSelectList(Constants.ChangeCharList);

            // 需求状态列表
            ViewBag.ReqSoftStatList = MyTools.GetSelectList(Constants.ReqSoftStatList);

            return View();
        }

        //
        // POST: /ReqTrack/Create

        [HttpPost]
        public ActionResult Create(ReqTrack reqTrack)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.ReqTracks.Add(reqTrack);
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
        // GET: /ReqTrack/Edit/5

        public ActionResult Edit(int id)
        {
            // 优先级列表
            ViewBag.PriorityList = MyTools.GetSelectList(Constants.PriorityList);

            // 变更标识列表
            ViewBag.ChangeCharList = MyTools.GetSelectList(Constants.ChangeCharList);

            // 需求状态列表
            ViewBag.ReqSoftStatList = MyTools.GetSelectList(Constants.ReqSoftStatList);

            List<ReqTrack> rm = dbContext.ReqTracks.ToList();
            ReqTrack reqTrack = rm.Find(a => a.TrackID == id);

            if (reqTrack == null)
            {
                ModelState.AddModelError("", "不存在该业需软需状态跟踪记录！");
            }
            return View(reqTrack);
        }

        //
        // POST: /ReqTrack/Edit/5

        [HttpPost]
        public ActionResult Edit(ReqTrack reqTrack)
        {
            try
            {
                dbContext.Entry(reqTrack).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        // POST: /ReqTrack/Delete/5

        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                List<ReqTrack> ls = dbContext.ReqTracks.ToList();
                ReqTrack reqTrack = ls.Find(a => a.TrackID == id);

                dbContext.Entry(reqTrack).State = System.Data.Entity.EntityState.Deleted;
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
