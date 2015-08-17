using MyTeam.Models;
using MyTeam.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 维护需求管理Controller
    /// </summary>
    /* 包括如下功能：
     * 1、批量入池
     * 2、查询
     * 3、批处理：批量出池、更新下发通知编号、下发日期、批量删除
     * 4、单笔新增
     * 5、单笔修改
     * 6、单笔删除
     * 7、出池计划导出
     * 8、按照查询条件导出
     */
#if Release
    [Authorize]
#endif
    public class ReqManageController : BaseController
    {
        /*
         * 【1】批量入池
         */

        // 入池：第一步，输入维护需求主信息
        public ActionResult MainInPool()
        {
            MainInPoolReq mainInPoolReq = new MainInPoolReq();
            // 1、生成系统列表
            SelectList sl1 = new SelectList(this.GetSysList(), "SysID", "SysName");

            ViewBag.SysList = sl1;

            // 2、生成需求受理人列表，默认当前用户为需求受理人 
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

            ViewBag.UserList = sl2;

            // 3、需求发起单位 
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            // 4、需求数量默认为1
            mainInPoolReq.ReqAmt = 1;

            return View(mainInPoolReq);
        }

        [HttpPost]
        public ActionResult MainInPool(MainInPoolReq mainInPoolReq)
        {
            return RedirectToAction("DetailInPool", mainInPoolReq);
        }

        // 入池：第二步，输入明细信息

        // 提供一个检查FirstReqDetailNo的接口（AJAX调用）
        public string CheckFirstReqDetailNo(string id)
        {
            string[] firstReqNum;
            int changeNum = 0;
            try
            {
                firstReqNum = id.Split('-');
                // 第二个是需要变化的数字
                changeNum = int.Parse(firstReqNum[1]);
                return "S";
            }
            catch
            {
                return "error";
            }
        }

        [HttpPost]
        public ActionResult DetailInPool(MainInPoolReq mainInPoolReq)
        {
            string firstReqDetailNo = mainInPoolReq.FirstReqDetailNo;
            int reqAmt = mainInPoolReq.ReqAmt;

            // 根据FirstReqDetailNo计算其他的需求编号，如为空则均为空
            List<string> reqDetailNoList = new List<string>();
            reqDetailNoList.Add(firstReqDetailNo);
            if (!string.IsNullOrEmpty(firstReqDetailNo))
            {
                // 判断需求编号是否正确
                string[] firstReqNum = null;
                int changeNum = 0;
                try
                {
                    firstReqNum = firstReqDetailNo.Split('-');
                    // 第二个是需要变化的数字
                    changeNum = int.Parse(firstReqNum[1]);
                }
                catch
                {
                    //return ContentResult("起始需求编号输入有误");
                }
                for (int i = 0; i < reqAmt - 1; i++)
                {
                    changeNum++;
                    reqDetailNoList.Add(this.GetReqNum(firstReqNum, changeNum));
                }
            }
            else
            {
                for (int i = 0; i < reqAmt - 1; i++)
                {
                    reqDetailNoList.Add("");
                }
            }

            // 生成List，添加维护需求编号
            List<Req> reqList = new List<Req>();
            foreach (string s in reqDetailNoList)
            {
                Req newReq = new Req()
                {
                    SysId = mainInPoolReq.SysId,
                    AcptDate = mainInPoolReq.AcptDate,
                    ReqNo = mainInPoolReq.ReqNo,
                    ReqReason = mainInPoolReq.ReqReason,
                    ReqFromDept = mainInPoolReq.ReqFromDept,
                    ReqFromPerson = mainInPoolReq.ReqFromPerson,
                    ReqAcptPerson = mainInPoolReq.ReqAcptPerson,
                    ReqDevPerson = mainInPoolReq.ReqDevPerson,
                    ReqBusiTestPerson = mainInPoolReq.ReqBusiTestPerson,
                    DevAcptDate = mainInPoolReq.DevAcptDate,
                    DevEvalDate = mainInPoolReq.DevEvalDate,
                    ReqDetailNo = s
                };
                reqList.Add(newReq);
            }
            // 需求类型下拉列表
            ViewBag.ReqTypeList = MyTools.GetSelectList(Constants.ReqTypeList);
            // 需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectList(Constants.ReqStatList, false, true, "入池");
            return View(reqList);
        }

        // 正式入池
        [HttpPost]
        public ActionResult InPoolResult(List<Req> reqList)
        {
            string r = "";
            try
            {
                if (ModelState.IsValid)
                {
                    // 入库
                    foreach (Req req in reqList)
                    {
                        dbContext.Reqs.Add(req);
                    }
                    dbContext.SaveChanges();

                    r = "<p class='text-success'>入池成功！</p><p>您可以：</p><p><ul><li><a href='/ReqManage'>返回</a></li><li><a href='/ReqManage/MainInPool'>继续入池</a></li></ul></p>";
                }

            }
            catch (Exception e1)
            {
                r = "<p class='text-danger'>入池失败！" + e1.Message + "</p>";
            }

            ViewBag.Msg = r;
            return View();

        }


        /*
         * 【2】查询
         */

        // 默认页为查询页
        // 按照查询条件查询结果：为使用分页功能，GET模式查询
        public ActionResult Index(ReqQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {
            if (isQuery)
            {
                var ls = from a in dbContext.Reqs
                         select a;
                if (query.SysId != 0)
                {
                    ls = ls.Where(p => p.SysId == query.SysId);
                }
                if (!string.IsNullOrEmpty(query.AcptMonth))
                {
                    ls = ls.Where(p => (p.AcptDate.Value.Year.ToString() + p.AcptDate.Value.Month.ToString()) == query.AcptMonth.Replace("/", ""));
                }
                if (!string.IsNullOrEmpty(query.ReqNo))
                {
                    ls = ls.Where(p => p.ReqNo == query.ReqNo);
                }
                if (!string.IsNullOrEmpty(query.ReqDetailNo))
                {
                    ls = ls.Where(p => p.ReqDetailNo == query.ReqDetailNo);
                }
                if (!string.IsNullOrEmpty(query.Version))
                {
                    ls = ls.Where(p => p.Version == query.Version);
                }
                if (!string.IsNullOrEmpty(query.Version))
                {
                    ls = ls.Where(p => p.Version == query.Version);
                }
                if (query.ReqStat != "全部")
                {
                    ls = ls.Where(p => p.ReqStat == query.ReqStat);
                }
                if (query.ReqAcptPerson != 0)
                {
                    ls = ls.Where(p => p.ReqAcptPerson == query.ReqAcptPerson);
                }

               

                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "维护需求查询_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 需要对list修改以适应Excel模板
                    List<ReqExcel> excelList = this.GetExcelList(ls);
                    return this.makeExcel<ReqExcel>("ReqReportT", targetFileName, excelList);
                }
                else
                {
                    var list = ls.ToList();
                    // 分页
                    query.ResultList = list.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE); ;
                }
            }
            else
            {
                query = new ReqQuery();
            }

            // 为了保证查询部分正常显示，对下拉列表处理

            // 系统列表下拉
            List<RetailSystem> sysList = this.GetSysList();
            // 加上“全部”
            sysList.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            ViewBag.SysList = new SelectList(sysList, "SysID", "SysName", query.SysId); ;

            // 需求受理人下拉
            List<User> userList = this.GetUserList();
            // 加上“全部”
            userList.Insert(0, new User() { UID = 0, Realname = "全部" });
            ViewBag.ReqAcptPerson = new SelectList(userList, "UID", "Realname", query.ReqAcptPerson);

            // 需求状态下拉
            ViewBag.ReqStatList = MyTools.GetSelectList(Constants.ReqStatList, true, true, query.ReqStat);

            return View(query);
        }

        /*
         * 【3】批处理
         */

        // 批处理统一入口
        public ActionResult Bat()
        {
            return View();
        }

        // Ajax调用，批量出池       
        [HttpPost]
        public string BatOutPool(string reqs, string version, string outDate, string planRlsDate, string outPoolProtect)
        {
            try
            {
                // 拼出sql中的in条件
                string whereIn = this.GetWhereIn(reqs);

                string sql = string.Format("update Reqs set Version='{0}', OutDate='{1}', PlanRlsDate='{2}', ReqStat=N'出池' where ReqDetailNo in ({3})", version, outDate, planRlsDate, whereIn);
                if (outPoolProtect == "true")
                {
                    sql += " and ReqStat <> N'出池'";
                }

                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                return "已更新" + r + "条记录!";
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
        }

        // Ajax调用，批量更新下发编号       
        [HttpPost]
        public string BatRlsNo(string reqs, string rlsNo, string RlsNoProtect)
        {
            try
            {
                // 拼出sql中的in条件
                string whereIn = this.GetWhereIn(reqs);

                string sql = string.Format("update Reqs set RlsNo='{0}' where ReqDetailNo in ({1})", rlsNo, whereIn);
                if (RlsNoProtect == "true")
                {
                    sql += " and ReqStat = N'出池'";
                }

                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                return "已更新" + r + "条记录!";
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
        }

        // Ajax调用，批量更新实际下发日期       
        [HttpPost]
        public string BatRlsDate(string reqs, string rlsDate, string RlsDateProtect)
        {
            try
            {
                // 拼出sql中的in条件
                string whereIn = this.GetWhereIn(reqs);

                string sql = string.Format("update Reqs set RlsDate='{0}' where ReqDetailNo in ({1})", rlsDate, whereIn);
                if (RlsDateProtect == "true")
                {
                    sql += " and ReqStat = N'出池'";
                }

                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                return "已更新" + r + "条记录!";
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
        }

        // Ajax调用，批量删除       
        [HttpPost]
        public string BatDel(string reqs)
        {
            try
            {
                // 拼出sql中的in条件
                string whereIn = this.GetWhereIn(reqs);

                string sql = string.Format("delete from Reqs where ReqDetailNo in ({0})", whereIn);

                // 批量删除，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                return "已更新" + r + "条记录!";
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
        }

        /*
         * 【4】单笔新增
         */
        public ActionResult Create()
        {
            // 1、生成系统列表
            List<RetailSystem> ls1 = this.GetSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            // 2、生成需求受理人列表，默认当前用户为需求受理人 
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

            ViewBag.UserList = sl2;

            // 3、需求发起单位 
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            // 4、需求类型下拉列表
            ViewBag.ReqTypeList = MyTools.GetSelectList(Constants.ReqTypeList);

            // 5、需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectList(Constants.ReqStatList, false, true, "入池");

            return View();
        }

        // ajax调用
        [HttpPost]
        public string Create(Req req)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Reqs.Add(req);
                    dbContext.SaveChanges();
                }

                return "新增成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /*
         * 【5】单笔修改
         */
        public ActionResult Edit(int id)
        {
            Req req = dbContext.Reqs.ToList().Find(a => a.RID == id);
            if (req == null)
            {
                ModelState.AddModelError("", "无此记录");
                return View();
            }

            // 下拉框预处理
            // 1、生成系统列表
            List<RetailSystem> ls1 = this.GetSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName", req.SysId);

            ViewBag.SysList = sl1;

            // 2、生成需求受理人列表
            SelectList sl2 = null;

            sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", req.ReqAcptPerson);

            ViewBag.UserList = sl2;

            // 4、需求发起单位 
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList, false, true, req.ReqFromDept);

            // 5、需求类型下拉列表
            ViewBag.ReqTypeList = MyTools.GetSelectList(Constants.ReqTypeList, false, true, req.ReqType);

            // 6、需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectList(Constants.ReqStatList, false, true, req.ReqStat);

            return View(req);
        }

        [HttpPost]
        public ActionResult Edit(Req req)
        {
            try
            {
                dbContext.Entry(req).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        /*
         * 6、单笔删除
         */
        // ajax调用
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                Req r = dbContext.Reqs.Where(a => a.RID == id).FirstOrDefault<Req>();
                dbContext.Entry(r).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /*
         * 7、出池计划查询与导出
         */
        public ActionResult OutPool(OutPoolQuery query, bool isQuery = false, int pageNum = 1, bool isExcel = false)
        {
            if (isQuery)
            {
                // 根据query条件查询结果
                var ls = from a in dbContext.Reqs
                         select a;
                if (query.SysId != 0)
                {
                    ls = ls.Where(p => p.SysId == query.SysId);
                }
                if (!string.IsNullOrEmpty(query.Version))
                {
                    // 版本号
                    string[] vers = query.Version.Split(',');
                    ls = from b in ls
                         where vers.Contains(b.Version)
                         select b;
                }
                if (!string.IsNullOrEmpty(query.MaintainYear))
                {
                    ls = ls.Where(p => p.AcptDate.Value.Year.ToString() == query.MaintainYear);
                }

                // 将查询结果转换为OutPoolResult
                List<OutPoolResult> resultList = new List<OutPoolResult>();
                foreach (Req req in ls)
                {
                    OutPoolResult res = new OutPoolResult()
                    {
                        AcptMonth = req.AcptDate.Value.ToString("yyyy/M"),
                        SysName = req.SysName,
                        Version = req.Version,
                        ReqNo = req.ReqNo,
                        ReqDetailNo = req.ReqDetailNo,
                        ReqReason = req.ReqReason,
                        ReqDesc = req.ReqDesc,
                        DevWorkload = req.DevWorkload,
                        ReqDevPerson = req.ReqDevPerson,
                        ReqBusiTestPerson = req.ReqBusiTestPerson,
                        ReqType = req.ReqType,
                        PlanRlsDate = req.PlanRlsDate,
                        RlsDate = req.RlsDate,
                        Remark = req.Remark
                    };
                    resultList.Add(res);
                }
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "零售条线出池计划";
                    if (query.SysId != 0)
                        targetFileName += "_" + resultList[0].SysName;
                    if (!string.IsNullOrEmpty(query.Version))
                        targetFileName += "_" + query.Version;
                    if (!string.IsNullOrEmpty(query.MaintainYear))
                        targetFileName += "_" + query.MaintainYear;
                    return this.makeExcel<OutPoolResult>("OutPoolReportT", targetFileName, resultList);
                }
                else
                {
                    // 分页
                    query.ResultList = resultList.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE);
                }
            }
            else
            {
                query = new OutPoolQuery();
            }

            // 系统列表下拉
            List<RetailSystem> ls1 = this.GetSysList();
            // 加上“全部”
            ls1.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            SelectList sl1 = new SelectList(ls1, "SysID", "SysName", query.SysId);
            ViewBag.SysList = sl1;

            return View(query);
        }


        // 自动生成维护需求编号
        private string GetReqNum(string[] firstReqNum, int changeNum)
        {
            if (firstReqNum == null)
                return "";
            firstReqNum[1] = changeNum.ToString().PadLeft(4, '0');
            StringBuilder sb = new StringBuilder(firstReqNum[0]);
            for (int i = 1; i < firstReqNum.Length; i++)
            {
                sb.Append("-").Append(firstReqNum[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 根据数组自动生成where in的条件
        /// </summary>
        /// <param name="reqs"></param>
        /// <returns></returns>
        private string GetWhereIn(string reqs)
        {
            // 拆分reqs
            string[] reqArr = reqs.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (string s in reqArr)
            {
                sb.Append(string.Format("'{0}',", s));
            }
            // 去掉最后一个逗号
            string result = sb.ToString();
            result = result.Substring(0, result.Length - 1);
            return result;
        }

        /// <summary>
        /// 生成用于Excel输出的list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<ReqExcel> GetExcelList(IQueryable<Req> list)
        {
            List<ReqExcel> rl = new List<ReqExcel>();
            foreach (Req s in list)
            {
                ReqExcel reqExcel = new ReqExcel()
                {
                    SysName = s.SysName,
                    AcptDate = s.AcptDate,
                    ReqNo = s.ReqNo,
                    ReqReason = s.ReqReason,
                    ReqFromDept = s.ReqFromDept,
                    ReqFromPerson = s.ReqFromPerson,
                    ReqAcptPerson = s.ReqAcptPersonNamePhone,
                    ReqDevPerson = s.ReqDevPerson,
                    ReqBusiTestPerson = s.ReqBusiTestPerson,
                    DevAcptDate = s.DevAcptDate,
                    DevEvalDate = s.DevEvalDate,
                    ReqDetailNo = s.ReqDetailNo,
                    Version = s.Version,
                    BusiReqNo = s.BusiReqNo,
                    ReqDesc = s.ReqDesc,
                    ReqType = s.ReqType,
                    DevWorkload = s.DevWorkload,
                    ReqStat = s.ReqStat,
                    OutDate = s.OutDate,
                    PlanRlsDate = s.PlanRlsDate,
                    RlsDate = s.RlsDate,
                    RlsNo = s.RlsNo,
                    IsSysAsso = (s.IsSysAsso && s.IsSysAsso) ? "是" : "",
                    AssoSysName = s.AssoSysName,
                    AssoReqNo = s.AssoReqNo,
                    AssoRlsDesc = s.AssoRlsDesc,
                    Remark = s.Remark
                };
                rl.Add(reqExcel);
            }
            return rl;
        }

    }
}
