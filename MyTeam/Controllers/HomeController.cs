using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyTeam.Models;

namespace MyTeam.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            // 首页显示 未出池的需求，未完成的项目
            User user = this.GetSessionCurrentUser();
            if(user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/Home/Index" });
            }

            // 根据用户UID，
            // （1）找到负责的系统，统计未出池的需求
            // （2）TODO：根据项目计划判断有无超期
            // 若为管理员，则显示全部

            HomeResult hr = new HomeResult();

            if(user.IsAdmin)
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum from Reqs t where t.ReqStat = N'入池' group by t.SysId").ToList();
                //hr.Works = dbContext.WeekReportDetails.Where(a => a.WorkStat != "完成").ToList();
            }
            else
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum from Reqs t where t.ReqStat = N'入池' and t.SysId in (select ss.SysId from RetailSystems ss where ss.ReqPersonID = @p0) group by t.SysId", user.UID).ToList();
                //hr.Works = dbContext.WeekReportDetails.Where(a => a.WorkStat != "完成" && a.Person.Contains(user.Realname)).ToList();
            }           

            // 判断入池已超过三个月，但是没有出池的需求记录
            if (user.IsAdmin)
            {
                hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReqDelay>("select t.SysId, count(1) as ReqDelayNum from Reqs t where t.ReqStat = N'入池' and t.AcptDate <= DATEADD(month,-3,GETDATE()) group by t.SysId").ToList();
            }
            else
            {
                hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReqDelay>("select t.SysId, count(1) as ReqDelayNum from Reqs t where t.ReqStat = N'入池' and t.AcptDate <= DATEADD(month,-3,GETDATE()) and t.SysId in (select ss.SysId from RetailSystems ss where ss.ReqPersonID = @p0) group by t.SysId", user.UID).ToList();
            }   
            foreach(HomeReqDelay d in hr.ReqDelayLS){
                int a = d.SysId;
                int b = d.ReqDelayNum;
            }


            // 筛选出各个阶段延期的项目
            // 首先获得所有有时间计划的项目列表，对没有时间计划的项目将不统计其延期的情况
            List<ProjPlan> plans = dbContext.ProjPlans.ToList();
            List<Proj> projs = dbContext.Projs.ToList();
            List<HomeProjDelay> delays = new List<HomeProjDelay>();

            foreach(ProjPlan plan in plans){
                Proj p = new Proj();
                if (user.IsAdmin)
                {
                    p = projs.Find(a => a.ProjID == plan.ProjID);
                }
                // 如果是非管理员登陆，显示自己的延期项目
                else
                {
                    p = projs.Find(a => a.ProjID == plan.ProjID && a.ReqAnalysisID == user.UID);
                }
                
                // 如果筛选出项目在项目计划列表中，那么判断时间是否延期
                if(p != null){
                    // 判断各个阶段的时间是否延期
                   if (p.OutlineEndDate == null && plan.OutlineFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "需求大纲结束编写";
                        delays.Add(projDelay);
                        continue;
                    }
                   else if (p.ReviewAcptDate == null && plan.ReviewStartDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "业需开始评审";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.ReqPublishDate == null && plan.ReviewFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "业需结束评审";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.TechFeasiReviewStartDate == null && plan.TechFeasiReviewStartDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "技术可行性分析报告评审开始";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.TechFeasiReviewFinishDate == null && plan.TechFeasiReviewFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "技术可行性分析报告评审结束";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.SoftBudgetStartDate == null && plan.SoftBudgetStartDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "软件实施投入预算开始";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.SoftBudgetFinishDate == null && plan.SoftBudgetFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "软件实施投入预算结束";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.ImplementPlansStartDate == null && plan.ImplementPlansStartDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "实施方案开始";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.ImplementPlansFinishDate == null && plan.ImplementPlansFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "实施方案结束";
                        delays.Add(projDelay);
                        continue;
                    }
                }
            }
            hr.ProjDetails = delays;
            
            return View(hr);
        }

        /// <summary>
        /// 更新内存
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateMemory()
        {
            this.Update();
            return Content("已更新内存");
        }

    }
}
