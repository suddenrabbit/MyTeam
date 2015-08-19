using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;
using PagedList;

namespace MyTeam.Controllers
{
#if Release
    [Authorize]
#endif
    // 年度版本下发计划Controller
    public class VerController : BaseController
    {
        //
        // GET: /Ver/     
        public ActionResult Index(VerQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {
            if (isQuery)
            {
                var ls = from a in dbContext.Vers
                         select a;

                if (query.SysId != 0)
                {
                    ls = ls.Where(p => p.SysId == query.SysId);
                }
                if (!string.IsNullOrEmpty(query.VerYear))
                {
                    ls = ls.Where(p => p.VerYear == query.VerYear);
                }
                var result = ls.ToList();
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "年度版本下发记录查询_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 需要对list修改以适应Excel模板
                    List<VerResult> excelList = this.GetExcelList(ls);
                    return this.makeExcel<VerResult>("VerReportT", targetFileName, excelList, 1);
                }
                else
                {
                    // 分页
                    query.ResultList = result.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE); ;
                }
            }
            else
            {
                query = new VerQuery();
            }

            // 项目列表
            List<RetailSystem> sysLs = dbContext.RetailSystems.ToList();
            sysLs.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            ViewBag.sysList = new SelectList(sysLs, "SysID", "SysName");

            List<Ver> vLs = dbContext.Vers.ToList();
            foreach (Ver v in vLs)
            {
                RetailSystem r = sysLs.Find(a => a.SysID == v.SysId);
                v.SysName = r == null ? "未知" : r.SysName;
            }

            return View(query);
        }
        
        
        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {

            List<RetailSystem> ls1 = this.GetSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone");
            }

            ViewBag.ReqPersonList = sl2;

            // 发布频率列表
            ViewBag.ReleaseFreqList = MyTools.GetSelectList(Constants.ReleaseFreqList);

            return View();
        }

        //
        // POST: /SysManage/Create

        [HttpPost]
        public string Create(Ver ver)
        {
            

            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Vers.Add(ver);
                    dbContext.SaveChanges();

                }
                return "<p class='alert alert-success'>新增成功</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }


        public ActionResult Edit(int id)
        {
            List<Ver> ls = dbContext.Vers.ToList();
            Ver ver = ls.Find(a => a.VerID == id);

            if (ver == null)
            {
                return View();
            }

            List<RetailSystem> ls1 = this.GetSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone");
            }

            ViewBag.ReqPersonList = sl2;

            // 发布频率列表
            ViewBag.ReleaseFreqList = MyTools.GetSelectList(Constants.ReleaseFreqList);

            return View(ver);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public string Edit(Ver ver)
        {
            try
            {
                dbContext.Entry(ver).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return "<p class='alert alert-success'>更新成功</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /SysManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {

            try
            {
                List<Ver> ls = dbContext.Vers.ToList();
                Ver ver = ls.Find(a => a.VerID == id);

                dbContext.Entry(ver).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                // 更新内存
                this.Update();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /// <summary>
        /// 生成用于Excel输出的list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<VerResult> GetExcelList(IQueryable<Ver> list)
        {
            List<VerResult> rl = new List<VerResult>();

            List<RetailSystem> rs = dbContext.RetailSystems.ToList();
            string sysNO = "";

            foreach (Ver s in list)
            {
                foreach(RetailSystem r in rs){
                    if(s.SysId == r.SysID){
                        s.SysName = r.SysName;
                        sysNO = r.SysNO;
                    }
                }

                VerResult VerExcel = new VerResult()
                {
                    SysNO = sysNO,
                    SysName = s.SysName,
                    ReleaseFreq = s.ReleaseFreq,
                    PublishTime = s.PublishTime,
                    VerNo = s.VerNo
                };
                rl.Add(VerExcel);
            }
            return rl;
        }
    }
}
