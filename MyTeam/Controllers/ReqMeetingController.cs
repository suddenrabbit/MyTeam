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
    public class ReqMeetingController : BaseController
    {
        //
        // GET: /ReqMeeting/

        public ActionResult Index()
        {
            List<ReqMeeting> ls = dbContext.ReqMeetings.ToList();

            List<Proj> projLs = dbContext.Projs.ToList();
            foreach (ReqMeeting rm in ls)
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
            return View();
        }

        //
        // GET: /ReqMeeting/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ReqMeeting/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /ReqMeeting/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /ReqMeeting/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /ReqMeeting/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /ReqMeeting/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
