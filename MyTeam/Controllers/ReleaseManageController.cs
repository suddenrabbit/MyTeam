using MyTeam.Enums;
using MyTeam.Models;
using MyTeam.Utils;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

                ls = ls.OrderByDescending(p => p.PlanReleaseDate);

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
            ViewBag.UserList = new SelectList(this.GetFormalUserList(), "UID", "Realname", GetSessionCurrentUser().UID);
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

            ViewBag.UserList = new SelectList(this.GetFormalUserList(), "UID", "Realname", rls.DraftPersonID);

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

                var msg = Constants.AJAX_EDIT_SUCCESS_RETURN;

                // 如果实际下发日期变成空，则要将相关需求的状态修改为【出池】；如果实际下发日期变成非空，则要将相关需求的状态修改为【办结】
                // 2017年11月7日：同时更新需求中的计划下发日期和实际下发日期
                if (!reqRelease.IsSideRelease)
                {
                    int reqStat = (int)ReqStatEnums.办结;
                    if (reqRelease.ReleaseDate == null)
                    {
                        reqStat = (int)ReqStatEnums.出池;
                    }

                    var sql = "Update ReqDetails set ReqStat = @p0, PlanReleaseDate=@p1, ReleaseDate=@p2 where ReqReleaseID=@p3";
                    int num = dbContext.Database.ExecuteSqlCommand(sql, reqStat, reqRelease.PlanReleaseDate, reqRelease.ReleaseDate, reqRelease.ReqReleaseID);
                    msg += "<p class='alert alert-info'>同时已更新" + num + "条相关维护需求的状态为【" + Enum.GetName(typeof(ReqStatEnums), reqStat) + "】，计划下发日期和实际下发日期也已同步更新入维护需求信息</p>";
                }

                return msg;
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
                string sql = string.Format("update ReqDetails set ReqReleaseID=0, PlanReleaseDate='', ReleaseDate='', UpdateTime='{0}' where ReqReleaseID = {1}", DateTime.Now.ToString("yyyy/M/d hh:mm:ss"), id);
                int num = dbContext.Database.ExecuteSqlCommand(sql);

                return "删除下发通知成功！同时已将" + num + "条维护需求中的相关下发信息清空";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        // 更新实际下发日期
        [HttpGet]
        public ActionResult UpdateReleaseDate()
        {
            return View();
        }

        [HttpPost]
        public string UpdateReleaseDate(string ReleaseNo, string ReleaseDate)
        {

            try
            {
                var r = dbContext.ReqReleases.Where(p => p.ReleaseNo == ReleaseNo).FirstOrDefault();
                if (r == null)
                {
                    throw new Exception("没有" + ReleaseNo + "相关下发记录");
                }

                r.ReleaseDate = DateTime.Parse(ReleaseDate);

                dbContext.Entry(r).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                string msg = "已更新" + ReleaseNo + "的下发日期";

                // 如果更新了主下发的实际下发日期，则将需求置为已办结
                // 2017年11月7日：同时更新ReqDetail中的实际下发信息
                if (!r.IsSideRelease)
                {
                    var sql = "Update ReqDetails set ReqStat = @p0, ReleaseDate=@p1 where ReqReleaseID = @p2";
                    int num = dbContext.Database.ExecuteSqlCommand(sql, (int)ReqStatEnums.办结, r.ReleaseDate, r.ReqReleaseID);
                    msg += "；同时已更新" + num + "条相关维护需求的状态为【办结】，并更新了维护需求中的实际下发日期";
                }

                return "<p class='alert alert-success'>" + msg + "</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        // 需求管理页面查看下发详情
        [HttpGet]
        public ActionResult GetDetailsForReqManage(int id)
        {
            var r = dbContext.ReqReleases.Find(id);

            return View(r);
        }

        // 从下发通知中移除需求
        [HttpGet]
        public string RemoveReq(int id, bool isSideRelease = false)
        {
            try
            {
                var sql = string.Format("update ReqDetails set {0}=0, Version='', ReqStat=@p2, PlanReleaseDate=NULL, ReleaseDate=NULL, UpdateTime=@p0 where ReqDetailID=@p1", isSideRelease ? "SecondReqReleaseID" : "ReqReleaseID");
                int num = dbContext.Database.ExecuteSqlCommand(sql, DateTime.Now, id, (int)ReqStatEnums.入池);
                if (num != 1)
                {
                    throw new Exception("操作失败！");
                }
            }
            catch (Exception e1)
            {
                return "出错了：" + e1.ToString();
            }

            return "success";
        }

        // 在通知中绑定需求
        [HttpGet]
        public string BindReq(int id, string reqDetailNo, bool isSideRelease = false)
        {
            try
            {
                var detail = dbContext.ReqDetails.Where(p => p.ReqDetailNo == reqDetailNo).FirstOrDefault();

                if (detail == null)
                {
                    return "不存在这个需求！";
                }

                if (detail.ReqReleaseID == id || detail.SecondReqReleaseID == id)
                {
                    return "不能重复添加！";
                }

                if (!isSideRelease)
                {
                    detail.ReqReleaseID = id;
                }
                else
                {
                    detail.SecondReqReleaseID = id;
                }

                detail.UpdateTime = DateTime.Now;

                dbContext.Entry(detail).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return string.Format("<tr id=\"tr{0}\"><td><a href=\"###\" onclick=\"doDelete(\'{0}\')\" class=\"text-danger\"><span class=\"glyphicon glyphicon-remove\"></span></a></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", detail.ReqDetailID, detail.ReqMain.SysName, detail.ReqDetailNo, detail.ShortReqDesc);
            }
            catch (Exception e1)
            {
                return "出错了：" + e1.ToString();
            }
        }

        /// <summary>
        /// 更新下发编号
        /// </summary>
        /// <param name="Reqs"></param>
        /// <param name="ReleaseNo"></param>
        /// <param name="SideReleaseNo"></param>
        /// <param name="PlanReleaseDate"></param>
        /// <returns></returns>
        [HttpPost]
        public string UpdateReleaseNo(string Reqs, string ReleaseNo, string PlanReleaseDate, string ReleaseDesc)
        {
            return updateReleaseNo(Reqs, ReleaseNo, PlanReleaseDate, ReleaseDesc);
        }


        /// <summary>
        /// 更新需求下发信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateReleaseInfo()
        {
            // 提供系统列表
            List<RetailSystem> ls1 = this.GetNormalSysList();
            // 加上“请选择系统”
            ls1.Insert(0, new RetailSystem() { SysID = 0, SysName = "--请选择系统--" });
            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");
            ViewBag.SysList = sl1;

            return View();
        }

        [HttpPost]
        public string UpdateReleaseInfo(int SysID, int Ver, string PlanReleaseDate, string ReleaseNo, string ReleaseDesc)
        {
            // Get Reqs from SysID and Ver
            var version = dbContext.Vers.Find(Ver);
            if(version == null)
            {
                return "<p class='alert alert-danger'>数据有误，请联系管理员！</p>";
            }
            var ls = dbContext.ReqDetails.Where(p => p.ReqMain.SysID == SysID && p.Version == version.VerNo);
            if(ls == null || ls.Count() < 1)
            {
                return "<p class='alert alert-danger'>找不到相关的需求，请确认选择的系统和版本号！</p>";
            }

            List<string> reqList = new List<string>();
            foreach(var r in ls)
            {
                reqList.Add(r.ReqDetailID.ToString());
            }
            string reqs = string.Join(",", reqList.ToArray());

            return updateReleaseNo(reqs, ReleaseNo, PlanReleaseDate, ReleaseDesc);
        }


        /// <summary>
        /// 检查下发通知编号
        /// </summary>
        /// <param name="releaseNo"></param>
        /// <returns></returns>
        private string checkReleaseNo(string releaseNo, string oldReleaseNo = "")
        {
            // 检查下发通知编号格式
            string pattern = "(YFZX){1}[0-9]{8}";
            Regex regex = new Regex(pattern);
            if (!regex.IsMatch(releaseNo))
            {
                return "下发通知编号" + releaseNo + "格式不正确，必须是YFZX+8位数字";
            }

            if (releaseNo != oldReleaseNo)
            {
                var rls = dbContext.ReqReleases.Where(p => p.ReleaseNo == releaseNo).FirstOrDefault();
                if (rls != null)
                {
                    return "下发通知编号" + releaseNo + "已经存在！";
                }
            }

            return "";
        }

        /// <summary>
        /// 更新维护需求编号的通用代码
        /// </summary>
        /// <param name="Reqs"></param>
        /// <param name="ReleaseNo"></param>
        /// <param name="PlanReleaseDate"></param>
        /// <param name="ReleaseDesc"></param>
        /// <returns></returns>
        private string updateReleaseNo(string Reqs, string ReleaseNo, string PlanReleaseDate, string ReleaseDesc)
        {
            string msg = "";

            try
            {
                // 首先生成主下发的release记录，得到release ID
                int reqReleaseID;

                var checkRelease = dbContext.ReqReleases.Where(p => p.ReleaseNo == ReleaseNo).FirstOrDefault();
                if (checkRelease != null)
                {
                    // 已存在则更新
                    checkRelease.PlanReleaseDate = DateTime.Parse(PlanReleaseDate);
                    checkRelease.ReleaseNo = ReleaseNo;
                    checkRelease.DraftPersonID = GetSessionCurrentUser().UID;//默认记录当前人员
                    checkRelease.ReleaseDesc = ReleaseDesc;
                    dbContext.Entry(checkRelease).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    reqReleaseID = checkRelease.ReqReleaseID;

                    msg = "<p class='alert alert-warning'>下发编号" + ReleaseNo + "已存在，对现有记录进行了更新！</p>";
                }
                else
                {
                    // 不存在则新增
                    var release = new ReqRelease
                    {
                        PlanReleaseDate = DateTime.Parse(PlanReleaseDate),
                        ReleaseNo = ReleaseNo,
                        DraftPersonID = GetSessionCurrentUser().UID,//默认记录当前人员
                        ReleaseDesc = ReleaseDesc
                    };
                    dbContext.ReqReleases.Add(release);
                    dbContext.SaveChanges();

                    reqReleaseID = release.ReqReleaseID;

                    msg = "<p class='alert alert-success'>添加下发记录" + ReleaseNo + "成功</p>";
                }

                // 将下发ID更新到req中
                string sql = string.Format("update ReqDetails set ReqReleaseID = '{0}', UpdateTime='{1}'  where ReqDetailID in ({2})", reqReleaseID, DateTime.Now.ToString("yyyy/M/d hh:mm:ss"), Reqs);

                int updatedNum = dbContext.Database.ExecuteSqlCommand(sql);

                msg += "<p class='alert alert-success'>" + updatedNum + "条需求更新成功！</p>";

            }
            catch (Exception e1)
            {
                msg += "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }

            return msg;
        }
    }
}
