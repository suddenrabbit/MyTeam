using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/Home/Index" });
            }

            // 根据用户UID，
            // （1）找到负责的系统，统计未出池的需求（未出池需求，超过三个月未出池需求，超过两个星期未入池需求）
            // （2）根据项目计划判断有无超期
            // 若为管理员，则显示全部

            HomeResult hr = new HomeResult();

            if (user.IsAdmin)
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = N'入池' group by t.SysId").ToList();
            }
            else
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = N'入池' and t.SysId in (select rs.SysId from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysId", user.UID).ToList();
            }

            // 统计计算未出池的需求总数
            int reqLsSum = 0;
            foreach(HomeReq q in hr.ReqLs)
            {
                reqLsSum += q.ReqNum;
            }

            ViewBag.ReqLsSum = reqLsSum;

            //////////////////////////////////////////////////////////////////////

            // 判断入池已超过三个月，但是没有出池的需求记录
            if (user.IsAdmin)
            {
                hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = N'入池' and t.AcptDate <= DATEADD(month,-3,GETDATE()) group by t.SysId").ToList();
            }
            else
            {
                hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = N'入池' and t.AcptDate <= DATEADD(month,-3,GETDATE()) and t.SysId in (select rs.SysId from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysId", user.UID).ToList();
            }

            // 统计计算三个月未出池的需求总数
            int reqDelayLsSum = 0;
            foreach (HomeReq q in hr.ReqDelayLS)
            {
                reqDelayLsSum += q.ReqNum;
            }

            ViewBag.ReqDelayLsSum = reqDelayLsSum;

            //////////////////////////////////////////////////////////////////////

            // 判断超过10天还没入池的记录（状态为「待评估」）
            if (user.IsAdmin)
            {
                hr.ReqInpoolDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = N'待评估' and t.AcptDate <= DATEADD(day,-10,GETDATE()) group by t.SysId").ToList();
            }
            else
            {
                hr.ReqInpoolDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = N'待评估' and t.AcptDate <= DATEADD(day,-10,GETDATE()) and t.SysId in (select rs.SysId from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysId", user.UID).ToList();
            }

            // 统计计算超过10天未入池的需求总数
            int reqInpoolDelayLsSum = 0;
            foreach (HomeReq q in hr.ReqInpoolDelayLS)
            {
                reqInpoolDelayLsSum += q.ReqNum;
            }

            ViewBag.ReqInpoolDelayLsSum = reqInpoolDelayLsSum;

            //////////////////////////////////////////////////////////////////////

            // 筛选出各个阶段延期的项目（只统计项目状态为：进行中）
            // 首先获得所有有时间计划的项目列表，对没有时间计划的项目将不统计其延期的情况
            List<ProjPlan> plans = dbContext.ProjPlans.ToList();
            List<Proj> projs = dbContext.Projs.Where(p=>p.ProjStat=="进行中").ToList();
            List<HomeProjDelay> delays = new List<HomeProjDelay>();

            foreach (ProjPlan plan in plans)
            {
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
                if (p != null)
                {
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
                    else if (p.TechFeasiReviewFinishDate == null && plan.TechFeasiReviewFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "技术可行性分析报告评审结束";
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

            //////////////////////////////////////////////////////////////////////

            // 列出超过计划下发日期仍未下发的
            string sql = "select distinct t.RlsNo, t.SecondRlsNo, t.PlanRlsDate from Reqs t where ((t.RlsNo is not null and t.RlsDate is null ) or (t.SecondRlsNo is not null and t.SecondRlsDate is null )) and t.PlanRlsDate < getdate()-1 and t.ReqStat=N'出池'";

            if (!IsAdminNow())
            {
                sql += " and t.ReqAcptPerson = " + user.UID;
            }
            hr.RlsDelayLS = dbContext.Database.SqlQuery<HomeRlsDelay>(sql).ToList();

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

        public ActionResult About()
        {
            return View();
        }

    }
}
