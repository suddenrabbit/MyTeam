using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 个人量化指标完成情况Controller
    /// </summary>
    public class TargetMissionController : BaseController
    {
        //
        // GET: /TargetMission/

        public ActionResult Index(int year = 0, int TID = 0)
        {
            if (this.GetSessionCurrentUser() == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/TargetMission" });
            }            

            // 按照年份显示
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            //var ls = dbContext.Database.SqlQuery<TargetMissionQuery>("select tm.TMID, tm.TID, tm.PersonName, tm.Mission, tm.TargetPoint, t.TargetName from TargetMissions tm left join Targets t on tm.TID = t.TID where t.TargetYear = @p0", year);
            var ls = from tm in dbContext.TargetMissions
                     join t in dbContext.Targets
                     on tm.TID equals t.TID
                     select new TargetMissionQuery
                     {
                         TMID = tm.TMID,
                         TID = tm.TID,
                         PersonID = tm.PersonID,
                         SidePerson = tm.SidePerson,
                         Mission = tm.Mission,
                         Stat = tm.Stat,
                         TargetName = t.TargetName,
                         TargetYear = t.TargetYear
                     };

            // TID和year只能有一个有效
            if (TID != 0)
            {
                ls = ls.Where(a => a.TID == TID);
            }
            else
            {                
                ls = ls.Where(a => a.TargetYear == year);
            }

            // Admin：显示所有任务
            // 本人：自己主办和协办的任务

            List<TargetMissionQuery> hostLs = null;
            List<TargetMissionQuery> sideLs = null;

            List<TargetMissionPoint> pointLs = new List<TargetMissionPoint>();
            
            if (!this.IsAdminNow())
            {
                User u = this.GetSessionCurrentUser();
                hostLs = ls.Where(a => a.PersonID == u.UID).ToList();
                sideLs = ls.Where(a => a.SidePerson.Contains(u.Realname)).ToList();                
            }
            else
            {
                hostLs = ls.ToList();
            }

            TargetMissionResult result = new TargetMissionResult();
            result.HostLs = hostLs;
            result.SideLs = sideLs;

            ViewBag.year = year;

            ViewBag.IsAdmin = this.IsAdminNow();

            return View(result);
        }

        //
        // GET: /TargetMission/Create

        public ActionResult Create()
        {
            // 获取已有的年度量化记录
            //List<int> years = dbContext.Database.SqlQuery<int>("select distinct t.TargetYear from Targets t").ToList<int>();
            List<int> years = new List<int>() { 2014, 2015, 2016, 2017, 2018, 2019 };

            ViewBag.years = years;

            // 人员列表
            SelectList sl = null;
            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl = new SelectList(this.GetUserList(), "UID", "Realname");
            }
            ViewBag.PersonList = sl;
            return View();
        }

        //
        // POST: /TargetMission/Create

        [HttpPost]
        public string Create(TargetMission tar)
        {
            try
            {
                // 任务内容中的换行符
                tar.Mission = tar.Mission.Replace(System.Environment.NewLine, "<br />");

                dbContext.TargetMissions.Add(tar);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;

            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        //
        // GET: /TargetMission/Edit/5

        public ActionResult Edit(int id)
        {
            TargetMission tar = this.dbContext.TargetMissions.FirstOrDefault(a => a.TMID == id);

            if (tar == null)
            {
                return View();
            }

            // 获取已有的年度量化记录
            List<int> years = dbContext.Database.SqlQuery<int>("select distinct t.TargetYear from Targets t").ToList<int>();
            //List<int> years = new List<int>() { 2014, 2015, 2016, 2017, 2018, 2019 };

            ViewBag.years = years;

            // 人员列表
            SelectList sl = null;
            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl = new SelectList(this.GetUserList(), "UID", "Realname");
            }
            ViewBag.PersonList = sl;

            // 根据TID获取年度
            Target t = dbContext.Targets.Where(a => a.TID == tar.TID).FirstOrDefault();
            if (t != null)
            {
                ViewBag.TargetYear = t.TargetYear;
                tar.TargetName = t.TargetName;
            }

            return View(tar);
        }

        //
        // POST: /TargetMission/Edit/5

        [HttpPost]
        public string Edit(TargetMission tar)
        {
            try
            {
                // 任务内容中的换行符
                tar.Mission = tar.Mission.Replace(System.Environment.NewLine, "<br />");

                dbContext.Entry(tar).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /TargetMission/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                TargetMission tar = this.dbContext.TargetMissions.FirstOrDefault(a => a.TMID == id);
                dbContext.Entry(tar).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        [HttpGet]
        public string GetTargets(int year)
        {
            var ls = from a in dbContext.Targets
                     where a.TargetYear == year
                     select a;
            StringBuilder sb = new StringBuilder();
            foreach (Target t in ls)
            {
                sb.Append("<option value='").Append(t.TID).Append("'>").Append(t.TargetName).Append("</option>");
            }

            return sb.ToString();
        }
      
    }
}
