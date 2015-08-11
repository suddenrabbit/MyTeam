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
    /*
     * 1、分为3个Index分别显示：（1）重点工作 （2）每周工作 （3）风险
     * 2、重点工作包含 重点工作本身和对应的每周工作
     * 3、每周工作页面显示全部
     */ 

    public class WeekReportController : BaseController
    {
        // 重点工作页面
        public ActionResult MainIndex()
        {
            List<WeekReportMain> ls = dbContext.WeekReportMains.ToList();
            // 若非管理员只显示自己填报的
            if(!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if(user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/MainIndex"});
                }
                ls.Where(a => a.RptPersonID == user.UID);
            }
            return View(ls);
        }

        // 填报:重点工作
        public ActionResult AddMain()
        {
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStageList);
            ViewBag.WorkStageList = sl;

            // 默认加上当前的用户UID
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddMain" });
            }

            WeekReportMain main = new WeekReportMain() { WorkTime = 0, RptPersonID = user.UID, PlanDeadLine=null };
            return View(main);
        }

        [HttpPost]
        public ActionResult AddMain(WeekReportMain main)
        {
            try
            {
                if(ModelState.IsValid)
                {                 
                    dbContext.WeekReportMains.Add(main);
                    dbContext.SaveChanges();
                }
            }
            catch(Exception e1)
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
            if(main == null)
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
                dbContext.Entry(main).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                
                return RedirectToAction("MainIndex");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 用户列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
                ViewBag.WorkStageList = sl; 
                return View(main);
            }
        }

        // AJAX调用
        // POST: /WeekReport/Delete/5
        [HttpPost]
        public string Delete(int id)
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

        // 每周工作页面
        public ActionResult DetailIndex()
        {
            return View();
        }

        // 风险与待协调问题页面
        public ActionResult RiskIndex()
        {
            return View();
        }
        
        // 获取最新的5个周报日期列表        
        public string GetRptDate()
        {
            string result = "2015年0709-0712,2015年0715-0719,2015年0722-0726";

            return result;
        }
    }
}
