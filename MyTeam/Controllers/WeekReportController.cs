using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Models;
using MyTeam.Utils;

namespace MyTeam.Controllers
{
#if Release
    [Authorize]
#endif
    /// <summary>
    /// 周报管理
    /// </summary>

    public class WeekReportController : BaseController
    {
        /* 重点工作 */

        // 重点工作页面
        public ActionResult MainIndex()
        {
            var ls = from a in dbContext.WeekReportMains select a;
            // 若非管理员只显示负责人中含有自己姓名的记录
            if (!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if (user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/MainIndex" });
                }
                ls = ls.Where(a => a.Person.Contains(user.Realname));
            }
            // 按照『进度』升序
            ls = ls.OrderBy(a => a.Progress);
            return View(ls.ToList());
        }

        // 填报:重点工作
        public ActionResult AddMain()
        {
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStageList);
            ViewBag.WorkStageList = sl;

            // 默认加上当前的用户UID和姓名
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddMain" });
            }

            WeekReportMain main = new WeekReportMain() { WorkTime = 0, RptPersonID = user.UID, PlanDeadLine = null, Person = user.Realname };
            return View(main);
        }

        [HttpPost]
        public ActionResult AddMain(WeekReportMain main)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.WeekReportMains.Add(main);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e1)
            {
                // 任务阶段下拉列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
                ViewBag.WorkStageList = sl;
                ModelState.AddModelError("", "出错了：" + e1.Message);
                return View(main);
            }
            return RedirectToAction("MainIndex");
        }

        // 修改：每周重点工作
        public ActionResult EditMain(int id)
        {
            WeekReportMain main = dbContext.WeekReportMains.ToList().Find(a => a.WRMainID == id);
            if (main == null)
            {
                ModelState.AddModelError("", "无此记录");
                main = new WeekReportMain();
            }
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
            ViewBag.WorkStageList = sl;
            return View(main);
        }

        [HttpPost]
        public ActionResult EditMain(WeekReportMain main)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(main).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    return RedirectToAction("MainIndex");
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 用户列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
                ViewBag.WorkStageList = sl;

            }
            return View(main);
        }

        // AJAX调用
        // POST: /WeekReport/DeleteMain/5
        [HttpPost]
        public string DeleteMain(int id)
        {
            try
            {
                WeekReportMain main = dbContext.WeekReportMains.ToList().Find(a => a.WRMainID == id);
                dbContext.Entry(main).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /* 重点工作对应的每周工作 */


        /* 每周工作 */

        // 每周工作页面
        public ActionResult DetailIndex()
        {
            var ls = from a in dbContext.WeekReportDetails select a;
            // 若非管理员只显示负责人中含有自己姓名的记录
            if (!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if (user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/DetailIndex" });
                }
                ls = ls.Where(a => a.Person.Contains(user.Realname));
            }
            // 此处只显示与“重点任务”无关的
            ls = ls.Where(a => a.IsWithMain != true);
            // 按照RptDate倒序显示
            ls = ls.OrderByDescending(a => a.RptDate);
            return View(ls.ToList());
        }

        // 添加每周工作
        public ActionResult AddDetail()
        {
            // 当前用户
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddDetail" });
            }

            // RptDate备选（取最近的5个）
            var r = dbContext.WeekReportDetails.OrderByDescending(a => a.RptDate).Take(5);
            string rptDates = "";
            foreach (var s in r)
            {
                rptDates += s.RptDate + ",";
            }
            if (rptDates.Length > 1)
                rptDates = rptDates.Substring(0, rptDates.Length - 1);
            ViewBag.RptDates = rptDates;

            // 工作任务：默认“领导交办”，可自由填写            

            // 完成情况的下拉列表
            ViewBag.WorkStatList = MyTools.GetSelectList(Constants.WorkStatList);

            WeekReportDetail detail = new WeekReportDetail()
            {
                Person = user.Realname,
                RptPersonID = user.UID,
                WorkMission = "领导交办",
                IsWithMain = false
            };

            return View(detail);
        }

        [HttpPost]
        public ActionResult AddDetail(WeekReportDetail detail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.WeekReportDetails.Add(detail);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了：" + e1.Message);
                // 完成情况的下拉列表
                ViewBag.WorkStatList = MyTools.GetSelectList(Constants.WorkStatList);
                return View(detail);
            }
            return RedirectToAction("DetailIndex");
        }

        // 修改：每周重点工作
        public ActionResult EditDetail(int id)
        {
            WeekReportDetail detail = dbContext.WeekReportDetails.ToList().Find(a => a.WRDetailID == id);
            if (detail == null)
            {
                ModelState.AddModelError("", "无此记录");
                detail = new WeekReportDetail();
            }
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStatList, false, true, detail.WorkStat);
            ViewBag.WorkStatList = sl;
            return View(detail);
        }

        [HttpPost]
        public ActionResult EditDetail(WeekReportDetail detail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(detail).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    return RedirectToAction("DetailIndex");
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 用户列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStatList, false, true, detail.WorkStat);
                ViewBag.WorkStatList = sl;

            }
            return View(detail);
        }

        // AJAX调用
        // POST: /WeekReport/DeleteDetail/5
        [HttpPost]
        public string DeleteDetail(int id)
        {
            try
            {
                WeekReportDetail detail = dbContext.WeekReportDetails.ToList().Find(a => a.WRDetailID == id);
                dbContext.Entry(detail).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /* 风险与待协调问题 */

        // 风险与待协调问题页面
        public ActionResult RiskIndex()
        {
            var ls = from a in dbContext.WeekReportRisks select a;
            // 若非管理员只显示自己填报的记录
            if (!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if (user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/RiskIndex" });
                }
                ls = ls.Where(a => a.RptPersonID == user.UID);
            }
            // 按照RptDate倒序显示
            ls = ls.OrderByDescending(a => a.RptDate);
            return View(ls.ToList());
        }

        // 填报:风险与待协调问题
        public ActionResult AddRisk()
        {
            // 默认加上当前的用户UID和姓名
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddRisk" });
            }

            WeekReportRisk risk = new WeekReportRisk() { RptPersonID = user.UID };
            return View(risk);
        }

        [HttpPost]
        public ActionResult AddRisk(WeekReportRisk risk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.WeekReportRisks.Add(risk);
                    dbContext.SaveChanges();
                    return RedirectToAction("RiskIndex");

                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了：" + e1.Message);

            }
            return View(risk);
        }

        // 修改：风险与待协调问题
        public ActionResult EditRisk(int id)
        {
            WeekReportRisk risk = dbContext.WeekReportRisks.ToList().Find(a => a.WRRiskID == id);
            if (risk == null)
            {
                ModelState.AddModelError("", "未找到该记录");
                risk = new WeekReportRisk();
            }

            return View(risk);
        }

        [HttpPost]
        public ActionResult EditRisk(WeekReportRisk risk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(risk).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    return RedirectToAction("RiskIndex");
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);

            }
            return View(risk);
        }

        // AJAX调用
        // POST: /WeekReport/DeleteRisk/5
        [HttpPost]
        public string DeleteRisk(int id)
        {
            try
            {
                WeekReportRisk Risk = dbContext.WeekReportRisks.ToList().Find(a => a.WRRiskID == id);
                dbContext.Entry(Risk).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        // 获取最新的5个周报日期列表        
        public string GetRptDate()
        {
            string result = "2015年0709-0712,2015年0715-0719,2015年0722-0726";

            return result;
        }
    }
}
