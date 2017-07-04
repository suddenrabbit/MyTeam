using MyTeam.Models;
using MyTeam.Utils;
using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 下发管理
    /// </summary>
    public class ReleaseManageController : BaseController
    {
        //
        // GET: /ReleaseManage/

        public ActionResult Index(ReleaseQuery query, int pageNum = 1, bool isQuery = false)
        {
            if (isQuery && query != null)
            {
                var ls = from a in dbContext.ReqReleases select a;
                if (!string.IsNullOrEmpty(query.ReleaseNo))
                {
                    ls = ls.Where(p => p.ReleaseNo.Contains(query.ReleaseNo));
                }
                if (!string.IsNullOrEmpty(query.PlanReleaseDateStart))
                {
                    var planReleaseDateStart = DateTime.Parse(query.PlanReleaseDateStart);
                    ls = ls.Where(p => p.PlanReleaseDate >= planReleaseDateStart);
                }
                if (!string.IsNullOrEmpty(query.PlanReleaseDateEnd))
                {
                    var planReleaseDateEnd = DateTime.Parse(query.PlanReleaseDateEnd);
                    ls = ls.Where(p => p.PlanReleaseDate <= planReleaseDateEnd);
                }
                if (!string.IsNullOrEmpty(query.ReleaseDateStart))
                {
                    var ReleaseDateStart = DateTime.Parse(query.ReleaseDateStart);
                    ls = ls.Where(p => p.ReleaseDate >= ReleaseDateStart);
                }
                if (!string.IsNullOrEmpty(query.ReleaseDateEnd))
                {
                    var ReleaseDateEnd = DateTime.Parse(query.ReleaseDateEnd);
                    ls = ls.Where(p => p.ReleaseDate <= ReleaseDateEnd);
                }

                ls = ls.OrderByDescending(p => p.ReqReleaseID);

                query.ResultList = ls.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE); ;
            }
            else
            {
                query = new ReleaseQuery();
            }
            return View(query);
        }

        //
        // GET: /ReleaseManage/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ReleaseManage/Create

        [HttpPost]
        public string Create(ReqRelease reqRelease)
        {
            try
            {
                var checkResult = checkReleaseNo(reqRelease.ReleaseNo);
                if (checkResult != "")
                {
                    throw new Exception(checkResult);
                }

                dbContext.ReqReleases.Add(reqRelease);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        //
        // GET: /ReleaseManage/Edit/5

        public ActionResult Edit(int id)
        {
            var rls = dbContext.ReqReleases.Find(id);
            rls.OldReleaseNo = rls.ReleaseNo;
            return View(rls);
        }

        //
        // POST: /ReleaseManage/Edit

        [HttpPost]
        public string Edit(ReqRelease reqRelease)
        {
            try
            {
                var checkResult = checkReleaseNo(reqRelease.ReleaseNo, reqRelease.OldReleaseNo);
                if (checkResult != "")
                {
                    throw new Exception(checkResult);
                }

                dbContext.Entry(reqRelease).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }


        //
        // POST: /ReleaseManage/Delete/5

        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                var rls = dbContext.ReqReleases.Find(id);
                dbContext.Entry(rls).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                // 删除相关维护需求中的下发信息
                string sql = string.Format("update ReqDetails set ReqReleaseID=0, UpdateTime='{0}' where ReqReleaseID = {1}", DateTime.Now.ToString("yyyy/M/d hh:mm:ss"), id);
                int num = dbContext.Database.ExecuteSqlCommand(sql);

                return "删除下发通知成功！同时已将" + num + "条维护需求中的相关下发信息清空！";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        /// <summary>
        /// 检查下发通知编号
        /// </summary>
        /// <param name="releaseNo"></param>
        /// <returns></returns>
        private string checkReleaseNo(string releaseNo, string oldReleaseNo = "")
        {
            // 检查下发通知编号格式
            //string pattern = "(YFZX){1}[0-9]{8}";

            return "";
        }
    }
}
