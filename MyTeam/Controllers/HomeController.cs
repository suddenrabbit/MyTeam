using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Models;
using MyTeam.Enums;

namespace MyTeam.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            // 首页显示 未出池的需求，未入池的需求，未完成的项目，未完成的下发
            User user = this.GetSessionCurrentUser();
            
            return View(GetHomeResult(user));
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

        /// <summary>
        /// 更新历史
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 供定时任务调用的查询接口
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Notify()
        {
            return View(GetHomeResult(isNotify:true));
        }

        protected HomeResult GetHomeResult(User user = null, bool isNotify = false)
        {
            bool isAdmin = true;
            if (user != null)
            {
                isAdmin = user.IsAdmin;
            }

            // 管理员可以查看所有，否则只能看到自己负责的系统或项目

            HomeResult hr = new HomeResult();

            if (isAdmin)
            {
                if(!isNotify)
                {
                    hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.入池 + " group by t.SysID").ToList();
                    hr.ReqInpoolLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.待评估 + " group by t.SysID").ToList();
                }
                hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.入池 + " and t.AcptDate <= DATEADD(month,-3,GETDATE()) group by t.SysID").ToList();
                hr.ReqInpoolDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, 0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.待评估 + " and t.AcptDate <= DATEADD(day,-8,GETDATE()) group by t.SysID").ToList();
            }
            else
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.入池 + " and t.SysID in (select rs.SysID from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysID", user.UID).ToList();
                hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.入池 + " and t.AcptDate <= DATEADD(month,-3,GETDATE()) and t.SysID in (select rs.SysID from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysID", user.UID).ToList();
                hr.ReqInpoolLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.待评估 + " and t.SysID in (select rs.SysID from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysID", user.UID).ToList();
                hr.ReqInpoolDelayLS = dbContext.Database.SqlQuery<HomeReq>("select t.SysID, count(1) as ReqNum, @p0 as ReqAcptPerson from Reqs t where t.ReqStat = " + (int)ReqStatEnums.待评估 + " and t.AcptDate <= DATEADD(day,-8,GETDATE()) and t.SysID in (select rs.SysID from RetailSystems rs where rs.ReqPersonID = @p0) group by t.SysID", user.UID).ToList();

            }
     
            if (!isNotify) //notify时不需要计算
            {
                // 统计计算3个月未出池的需求总数
                int reqLsSum = 0;                
                foreach (HomeReq q in hr.ReqLs)
                {
                    reqLsSum += q.ReqNum;
                }
                hr.ReqLsSum = reqLsSum;

                // 统计计算8天未入池的需求总数
                int reqInpoolLsSum = 0;
                foreach (HomeReq q in hr.ReqInpoolLS)
                {
                    reqInpoolLsSum += q.ReqNum;
                }
                hr.ReqInpoolLsSum = reqInpoolLsSum;

                // 统计计算三个月未出池的需求总数
                int reqDelayLsSum = 0;
                foreach (HomeReq q in hr.ReqDelayLS)
                {
                    reqDelayLsSum += q.ReqNum;
                }

                hr.ReqDelayLsSum = reqDelayLsSum;

                // 统计计算超过8天未入池的需求总数
                int reqInpoolDelayLsSum = 0;
                foreach (HomeReq q in hr.ReqInpoolDelayLS)
                {
                    reqInpoolDelayLsSum += q.ReqNum;
                }

                hr.ReqInpoolDelayLsSum = reqInpoolDelayLsSum;

            }
 
            //////////////////////////////////////////////////////////////////////

            // 筛选出各个阶段延期的项目（只统计项目状态为：进行中）
            // 首先获得所有有时间计划的项目列表，对没有时间计划的项目将不统计其延期的情况
            List<ProjPlan> plans = dbContext.ProjPlans.ToList();
            List<Proj> projs = dbContext.Projs.Where(p => p.ProjStat == "进行中").ToList();
            List<HomeProjDelay> delays = new List<HomeProjDelay>();

            foreach (ProjPlan plan in plans)
            {
                Proj p = new Proj();
                if (isAdmin)
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
            string sql = "select distinct t.RlsNo, t.SecondRlsNo, t.PlanRlsDate from Reqs t where t.PlanRlsDate < getdate()-1 and t.ReqStat=" + (int)ReqStatEnums.出池;

            if (!isAdmin)
            {
                sql += " and t.ReqAcptPerson = " + user.UID;
            }
            hr.RlsDelayLS = dbContext.Database.SqlQuery<HomeRlsDelay>(sql).ToList();

            /////////////////////////////////////////////////////////////////////

            // notify中增加忘记填写下发通知编号的提醒
            if(isNotify)
            {
                sql = "select distinct t.SysId, t.Version from Reqs t where t.RlsNo is null and t.ReqStat=" + (int)ReqStatEnums.出池;
                hr.NoRlsNoLS = dbContext.Database.SqlQuery<HomeNoRlsNo>(sql).ToList(); 
            }

            return hr;
        }
    }
}
