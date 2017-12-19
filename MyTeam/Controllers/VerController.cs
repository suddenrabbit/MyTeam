using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;

namespace MyTeam.Controllers
{
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

                if (query.SysID != 0)
                {
                    ls = ls.Where(p => p.SysID == query.SysID);
                }

                if (!string.IsNullOrEmpty(query.VerYear))
                {
                    ls = ls.Where(p => p.VerYear == query.VerYear);
                }

                // 按照发布时间排序
                ls = ls.OrderBy(p => p.SysID).ThenByDescending(p => p.PublishTime);

                var result = ls.ToList();
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "年度版本下发记录查询_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 需要对list修改以适应Excel模板
                    List<VerResult> excelList = this.GetExcelList(result);
                    return this.MakeExcel<VerResult>("VerReportT", targetFileName, excelList, 1);
                }
                else
                {
                    // 分页
                    query.ResultList = result.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE); ;
                }
            }
            else
            {
                query = new VerQuery() { VerYear = DateTime.Now.Year.ToString() }; // 默认只今年
            }

            // 项目列表
            List<RetailSystem> sysLs = GetNormalSysList();
            sysLs.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            ViewBag.sysList = new SelectList(sysLs, "SysID", "SysName");

            return View(query);
        }


        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {

            List<RetailSystem> ls1 = this.GetNormalSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname");
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
                // 同一个系统的版本号不能重复
                var checkVer = dbContext.Vers.Where(p => p.SysID == ver.SysID && p.VerNo == ver.VerNo).FirstOrDefault();
                if(checkVer != null)
                {
                    throw new Exception("同一个系统的版本号不能重复创建！");
                }

                dbContext.Vers.Add(ver);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
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
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname");
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

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
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
        private List<VerResult> GetExcelList(List<Ver> list)
        {
            List<VerResult> rl = new List<VerResult>();

            List<RetailSystem> rs = GetNormalSysList();
            

            foreach (Ver s in list)
            {
                var sys = rs.Where(p => p.SysID == s.SysID).FirstOrDefault();
                var sysNO = sys == null ? "未知" : sys.SysNO;
                var sysName = sys == null ? "未知" : sys.SysName;

                VerResult VerExcel = new VerResult()
                {
                    SysNO = sysNO,
                    SysName = sysName,
                    ReleaseFreq = s.ReleaseFreq,
                    PublishTime = s.PublishTime,
                    VerNo = s.VerNo
                };
                rl.Add(VerExcel);
            }
            return rl;
        }


        /*
         * 快速生成版本计划：
         * 根据起始版本号、起始日期，按照频率计算当年的版本计划
         */
        public ActionResult QuickVer()
        {
            List<RetailSystem> ls1 = GetNormalSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname");
            }

            ViewBag.ReqPersonList = sl2;

            // 发布频率列表
            ViewBag.ReleaseFreqList = MyTools.GetSelectList(Constants.ReleaseFreqList);

            return View();
        }

        [HttpPost]
        public string QuickVer(Ver ver)
        {
            try
            {
                // 根据起始日期和频率，确定要制定多少条
                string[] firstDate = ver.VerYear.Split('/');
                string verYear = firstDate[0];
                int verMonth = int.Parse(firstDate[1]);
                int freq = ver.ReleaseFreq;

                int num = (12 - verMonth) / freq + 1; // 版本数量

                string[] verNos = ver.VerNo.Split('.');
                int changeVerNo = int.Parse(verNos[1]); //要变化的版本号为小数点后的数字

                for (int i = 0; i < num; i++)
                {
                    Ver v = new Ver()
                    {
                        SysID = ver.SysID,
                        VerYear = verYear,
                        ReleaseFreq = freq,
                        PublishTime = GetFourthThursday(verYear, verMonth.ToString()), // 自动设计为每月第四个周四
                        VerNo = verNos[0] + "." + changeVerNo,
                        DraftPersonID = ver.DraftPersonID,
                        VerType = "计划版本"
                    };
                    dbContext.Vers.Add(v);

                    // 月份变化
                    verMonth += freq;
                    // 版本号变化
                    changeVerNo++;
                }

                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }

            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        /// <summary>
        /// 获取当月的第四个周四
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public DateTime GetFourthThursday(string year, string month)
        {
            DateTime day = DateTime.Parse(year + "/" + month + "/1");

            while (true)
            {
                if (day.DayOfWeek == DayOfWeek.Thursday)
                {
                    day = day.AddDays(21);
                    break;
                }
                else
                {
                    day = day.AddDays(1);
                }
            }

            return day;

        }
    }
}
