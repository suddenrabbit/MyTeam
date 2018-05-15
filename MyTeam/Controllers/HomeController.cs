using MyTeam.Enums;
using MyTeam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class HomeController : BaseController
    {
        const int OUTPOOL_DELAY_MONTHS = -3; // 超过3个月未出池
        const int INPOOL_DELAY_DAYS = -4; // 超过4天未入池

        //
        // GET: /Home/
        public ActionResult Index()
        {
            User user = this.GetSessionCurrentUser();

            HomeResult hr = GetHomeResult(user);

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

        /// <summary>
        /// 更新历史
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            var logs = dbContext.UpgradeLogs.OrderByDescending(p => p.ReleaseDate).ToList();
            return View(logs);
        }

        /// <summary>
        /// 接口：获取通知信息
        /// </summary>
        /// <param name="id">NotesID</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Notify(string id)
        {
            User u = GetUserList().Find(p => p.Username == id);
            if(u == null)
            {
                return null;
            }
            var uid = u.UID;

            // 未入池的详细信息
            var delayDate = DateTime.Now.AddDays(INPOOL_DELAY_DAYS);
            var inPoolDelay = dbContext.ReqMains.Where(p => p.ProcessStat == (int)ReqProcessStatEnums.研发评估 && p.AcptDate < delayDate && p.ReqAcptPerson == uid).ToList();
            
            // 延期的项目
            var projs = dbContext.Projs.Where(p => p.ProjStat == (int)ProjStatEnums.进行中 && p.ReqAnalysisID == uid).ToList();
            var projDelay = getProjDelays(projs);

            NotifyInfo notifyInfo = new NotifyInfo { InPoolDelay = inPoolDelay, ProjDelay = projDelay };

            return View(notifyInfo);
        }

        /// <summary>
        /// 接口：获取通知用户列表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public string GetNotifyUsers(bool debug = false)
        {
            var userList = new List<string>();

            if (!debug)
            {
                foreach (var u in GetFormalUserList())
                {
                    userList.Add(u.Username);
                }

                return string.Join(",", userList.ToArray());
            }
            else
            {
                return "014690";
            }
        }

        /// <summary>
        /// 获取提醒结果
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isNotify"></param>
        /// <returns></returns>
        protected HomeResult GetHomeResult(User user = null)
        {
            var isAdmin = true;

            var uid = 0;

            if (user != null && !user.IsAdmin)
            {
                isAdmin = false;
                // 2017年1月5日新增：外协根据BelongTo可以看到相应的内容
                uid = user.UID;
                if (user.UserType == (int)UserTypeEnums.外协)
                {
                    uid = user.BelongTo;
                }

            }

            // 管理员可以查看所有，否则只能看到自己负责的系统或项目
            HomeResult hr = new HomeResult();

            hr.UID = uid;

            string sql = "SELECT main.SysID, count(1) as ReqNum, {0} as ReqAcptPerson FROM ReqMains main LEFT JOIN ReqDetails detail ON main.ReqMainID = detail.ReqMainID WHERE detail.ReqStat = {1} {3}"; //组织SQL语句

            if (!isAdmin)
            {
                // 非Admin则SQL增加用户信息
                //sql += " and main.SysID in (select rs.SysID from RetailSystems rs where rs.ReqPersonID = {0}) ";
                sql += " and main.ReqAcptPerson = {0}"; // 谁受理，提醒谁
            }

            sql += " {2} GROUP BY main.SysID";


            hr.ReqLS = dbContext.Database.SqlQuery<HomeReq>(string.Format(sql, uid, (int)ReqStatEnums.入池, "", "")).ToList();
            hr.ReqInpoolLS = dbContext.Database.SqlQuery<HomeReq>(string.Format(sql, uid, (int)ReqStatEnums.待评估, "", "")).ToList();


            hr.ReqDelayLS = dbContext.Database.SqlQuery<HomeReq>(string.Format(sql, uid, (int)ReqStatEnums.入池, "and main.AcptDate <= DATEADD(month,"+OUTPOOL_DELAY_MONTHS+",GETDATE())", "")).ToList();
            hr.ReqInpoolDelayLS = dbContext.Database.SqlQuery<HomeReq>(string.Format(sql, uid, (int)ReqStatEnums.待评估, "and main.AcptDate <= DATEADD(day,"+INPOOL_DELAY_DAYS+",GETDATE())", "and main.ProcessStat = " + (int)ReqProcessStatEnums.研发评估)).ToList(); // 对于拟稿人办理的不跟踪


            // 统计计算未出池的需求总数
            int reqLsSum = 0;
            foreach (HomeReq q in hr.ReqLS)
            {
                reqLsSum += q.ReqNum;
            }
            hr.ReqLsSum = reqLsSum;

            // 统计计算未入池的需求总数
            int reqInpoolLsSum = 0;
            foreach (HomeReq q in hr.ReqInpoolLS)
            {
                reqInpoolLsSum += q.ReqNum;
            }
            hr.ReqInpoolLsSum = reqInpoolLsSum;

            // 统计计算超期未出池的需求总数
            int reqDelayLsSum = 0;
            foreach (HomeReq q in hr.ReqDelayLS)
            {
                reqDelayLsSum += q.ReqNum;
            }

            hr.ReqDelayLsSum = reqDelayLsSum;

            // 统计计算超期未入池的需求总数
            int reqInpoolDelayLsSum = 0;
            foreach (HomeReq q in hr.ReqInpoolDelayLS)
            {
                reqInpoolDelayLsSum += q.ReqNum;
            }

            hr.ReqInpoolDelayLsSum = reqInpoolDelayLsSum;



            //////////////////////////////////////////////////////////////////////
            // 筛选出各个阶段延期的项目（只统计项目状态为：进行中）
            //List<ProjPlan> plans = dbContext.ProjPlans.ToList();

            // 找到进行中的项目
            List<Proj> projs = null;
            if (isAdmin)
            {
                projs = dbContext.Projs.Where(p => p.ProjStat == (int)ProjStatEnums.进行中).ToList();
            }
            else
            {
                projs = dbContext.Projs.Where(p => p.ProjStat == (int)ProjStatEnums.进行中 && p.ReqAnalysisID == uid).ToList();
            }

            // 顺便统计进行中的项目有多少
            ViewBag.ProjsInProcessNum = projs.Count;

            // 接下来判断延期的具体，信息，提取到公共方法

            List<HomeProjDelay> delays = getProjDelays(projs);

            hr.ProjDetails = delays;

            //////////////////////////////////////////////////////////////////////

            // 列出超过计划下发日期仍未下发的
            // 2018年2月9日修改：同时统计所有正在进行中的下发
            var rlsLs = dbContext.ReqReleases.Where(p => p.ReleaseDate == null);
            if (!isAdmin)
            {
                rlsLs = rlsLs.Where(p => p.DraftPersonID == uid);
            }

            ViewBag.ReleasesInProcessNum = rlsLs.Count();

            var d = DateTime.Now.AddDays(-1);

            hr.RlsDelayLS = rlsLs.Where(p => p.PlanReleaseDate < d).ToList();

            return hr;
        }        

        /// <summary>
        /// 右上角用户信息和登录退出分部视图
        /// </summary>
        /// <returns></returns>
        public PartialViewResult LoginPartial()
        {
            if (Session == null || Session["Realname"] == null)
            {
                var user = this.GetSessionCurrentUser();
                Session["Realname"] = user.Realname;
                Session["IsAdmin"] = user.IsAdmin;
            }

            return PartialView();
        }

        /* 
         * 2017年2月27日：去除主题相关功能
         * 
        /// <summary>
        /// 获取主题
        /// </summary>
        /// <returns></returns>
        public string GetTheme()
        {
            // 通过cookie判断主题
            var theme = "navbar-inverse";
            if (Request.Cookies["theme"] != null)
            {
                theme = "navbar-" + Request.Cookies["theme"].Value;
            }

            return theme;
        }

        /// <summary>
        /// 记录主题
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public string SetTheme(string theme)
        {
            Response.Cookies["theme"].Value = theme;
            Response.Cookies["theme"].Expires = DateTime.MaxValue;

            return Constants.AJAX_RESULT_SUCCESS;
        }
        */


        /// <summary>
        /// 获取项目延期的具体情况
        /// </summary>
        /// <param name="projs"></param>
        /// <returns></returns>
        private List<HomeProjDelay> getProjDelays(List<Proj> projs)
        {
            List<HomeProjDelay> delays = new List<HomeProjDelay>();

            foreach (Proj p in projs) //从筛选出的项目中，寻找项目计划
            {
                ProjPlan plan = dbContext.ProjPlans.Where(a => a.ProjID == p.ProjID).FirstOrDefault();

                // 如果筛选出项目在项目计划列表中，那么判断时间是否延期
                if (plan != null)
                {
                    // 判断各个阶段的时间是否延期
                    if (p.OutlineEndDate == null && plan.OutlineFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "需求大纲编写结束";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.ReviewAcptDate == null && plan.ReviewStartDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "业需评审开始";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.ReqPublishDate == null && plan.ReviewFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "业需评审结束";
                        delays.Add(projDelay);
                        continue;
                    }
                    else if (p.TechFeasiReviewFinishDate == null && plan.TechFeasiReviewFinishDate <= DateTime.Now)
                    {
                        HomeProjDelay projDelay = new HomeProjDelay();
                        projDelay.ProjId = p.ProjID;
                        projDelay.DelayDetail = "技术可行性评审结束";
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

            return delays;
        }

    }

}
