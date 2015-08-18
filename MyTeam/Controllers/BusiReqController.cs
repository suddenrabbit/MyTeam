using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using MyTeam.Utils;
using MyTeam.Models;

namespace MyTeam.Controllers
{
#if Release
    [Authorize]
#endif
    /// <summary>
    /// 业需明细管理
    /// </summary>
    public class BusiReqController : BaseController
    {
        //
        // GET: /BusiReq/

        public ActionResult Index(BusiReqQuery query,  int pageNum = 1, bool isQuery = false)
        {
            var ls = from a in dbContext.BusiReqs select a;
            if(isQuery)
            {
                if(query.ProjID!=0)
                {
                    ls = ls.Where(p => p.ProjID == query.ProjID);
                }
                // 分页
                query.ResultList = ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE);
            }
            else
            {
                query = new BusiReqQuery();
            }

            // 项目列表
            var r = dbContext.Projs.ToList();
            // 添加全部
            r.Insert(0, new Proj(){ProjID = 0, ProjName = "全部"});
            ViewBag.ProjList = new SelectList(r, "ProjID", "ProjName", query.ProjID);

            return View(query);
        }

        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {
            // 项目列表
            var ls = dbContext.Projs.ToList();
            ViewBag.ProjList = new SelectList(ls, "ProjID", "ProjName");

            // 需求来源及状态的下拉列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.BusiReqStat);

            return View();
        }

        //
        // POST: /SysManage/Create

        [HttpPost]
        public ActionResult Create(BusiReq br)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    dbContext.BusiReqs.Add(br);
                    dbContext.SaveChanges();                    
                }
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        //
        // GET: /SysManage/Edit/5

        public ActionResult Edit(int id)
        {
            BusiReq br = dbContext.BusiReqs.ToList().Find(a => a.BRID == id);

            if (br == null)
            {
                ModelState.AddModelError("", "无此记录！");
                return View();
            }

            // 项目列表
            var ls = dbContext.Projs.ToList();
            ViewBag.ProjList = new SelectList(ls, "ProjID", "ProjName", br.ProjID);

            // 需求来源及状态的下拉列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.BusiReqStat, false, true, br.Stat);

            return View(br);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public ActionResult Edit(BusiReq br)
        {
            try
            {
                dbContext.Entry(br).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
             
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 项目列表
                var ls = dbContext.Projs.ToList();
                ViewBag.ProjList = new SelectList(ls, "ProjID", "ProjName", br.ProjID);

                // 需求来源及状态的下拉列表
                ViewBag.StatList = MyTools.GetSelectList(Constants.BusiReqStat, false, true, br.Stat);
                return View(br);
            }
        }

        // AJAX调用
        // POST: /SysManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                BusiReq br = dbContext.BusiReqs.ToList().Find(a => a.BRID == id); 
                dbContext.Entry(br).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
            
                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /// <summary>
        /// 导出跟踪表
        /// </summary>
        /// <param name="ProjID"></param>
        /// <returns></returns>
        public ActionResult Export(string ProjID)
        {
            // 联立查询 BusiReqs 和 Reqs
            var ls = from br in dbContext.BusiReqs
                     join req in dbContext.Reqs
                     on br.BusiReqNo equals req.BusiReqNo into results                    
                     from rr in results.DefaultIfEmpty()
                     where br.ProjID.ToString() == ProjID
                     select new BusiReqExcel
                     {
                         ProjName = "项目",
                         BusiReqNo = br.BusiReqNo,
                         BusiReqName = br.BusiReqName,
                         Desc = br.Desc,
                         CreateDate = br.CreateDate,
                         Stat = br.Stat,
                         AcptDate = rr.AcptDate,
                         ReqNo = rr.ReqNo,
                         ReqDetailNo = rr.ReqDetailNo,
                         Version = rr.Version,
                         ReqReason = rr.ReqReason,
                         ReqDesc = rr.ReqDesc,
                         ReqFromPerson = rr.ReqFromPerson,
                         ReqType = rr.ReqType,
                         RlsDate = rr.RlsDate
                     };
            return this.makeExcel<BusiReqExcel>("BusiReqReportT", "业务需求变更跟踪", ls.ToList<BusiReqExcel>(), 2);
                        
        }
    }
}
