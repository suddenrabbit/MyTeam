using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyTeam.Models;

namespace MyTeam.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            // 首页显示 用户姓名，未出池的需求
            User user = this.GetSessionCurrentUser();
            if(user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/Home/Index" });
            }
            ViewBag.Name = user.Realname;

            // 根据用户UID，找到负责的系统，统计未出池的需求
            // 若为管理员，则显示全部
            if(user.IsAdmin)
            {
                ViewBag.ResultList = dbContext.Database.SqlQuery<HomeResult>("select t.SysId, count(1) as ReqNum from Reqs t where t.ReqStat != N'出池' group by t.SysId");
            }
            else
            {
                ViewBag.ResultList = dbContext.Database.SqlQuery<HomeResult>("select t.SysId, count(1) as ReqNum from Reqs t where t.ReqStat != N'出池' and t.SysId in (select ss.SysId from RetailSystems ss where ss.ReqPersonID = @p0) group by t.SysId", user.UID);
            }

            return View();
        }

    }
}
