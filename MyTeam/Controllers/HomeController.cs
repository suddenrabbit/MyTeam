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
