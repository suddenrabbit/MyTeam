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
            // 首页显示 未出池的需求，未完成的项目
            User user = this.GetSessionCurrentUser();
            if(user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/Home/Index" });
            }

            // 根据用户UID，
            // （1）找到负责的系统，统计未出池的需求
            // （2）找到负责的项目，显示项目进度（不为100%的）
            // 若为管理员，则显示全部

            HomeResult hr = new HomeResult();

            List<Proj> projList = null;

            if(user.IsAdmin)
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum from Reqs t where t.ReqStat = N'入池' group by t.SysId").ToList();
                projList = this.GetProjList();
            }
            else
            {
                hr.ReqLs = dbContext.Database.SqlQuery<HomeReq>("select t.SysId, count(1) as ReqNum from Reqs t where t.ReqStat = N'入池' and t.SysId in (select ss.SysId from RetailSystems ss where ss.ReqPersonID = @p0) group by t.SysId", user.UID).ToList();
                projList = this.GetProjList().Where(p => p.ReqAnalysisID == user.UID).ToList();
            }

            // 根据ProjList计算每个项目的进度
            // 计算依据：所有的日期填写情况
            List<HomeProj> hpLs = new List<HomeProj>();
            foreach(var p in projList)
            {
                var props = p.GetType().GetProperties();
                int i = 0;
                int all = 0;
                foreach(var pr in props)
                {
                    if(pr.PropertyType == typeof(DateTime?)) //Proj里Datetime全部可null
                    {
                        all++;
                        object o = pr.GetValue(p, null);
                        if(o!=null && !string.IsNullOrEmpty(o.ToString()))
                        {
                            i++;
                        }
                    }
                }
                decimal result = Math.Round((decimal)i / all * 100, 1);

                if(result < 100)
                {
                    hpLs.Add(new HomeProj() { ProjID = p.ProjID, ProjName = p.ProjName, Progress = result.ToString() });
                }
            }

            hr.ProjLs = hpLs;

            return View(hr);
        }

        /// <summary>
        /// 更新内存
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult UpdateMemory()
        {
            this.Update();
            return Content("已更新内存");
        }

    }
}
