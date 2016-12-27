using MyTeam.Models;
using MyTeam.Utils;
using MyTeam.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using PagedList;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 维护需求管理Controller
    /// </summary>
    /* 包括如下功能：
     * 1、批量登记
     * 2、需求入池
     * 3、查询
     * 4、批处理：批量出池、更新下发通知编号、下发日期、批量删除
     * 5、单笔新增
     * 6、单笔修改
     * 7、单笔删除
     * 8、出池计划导出
     * 9、按照查询条件导出
     */
    public class ReqManageController : BaseController
    {
        /*
         * 【1】批量登记
         */

        // 登记受理        
        public ActionResult Reg()
        {
            RegReq regReq = new RegReq();
            // 1、生成系统列表
            SelectList sl1 = new SelectList(this.GetNormalSysList(), "SysID", "SysName");

            ViewBag.SysList = sl1;

            // 2、生成需求受理人列表，默认当前用户为需求受理人 
            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                //sl = new SelectList(this.GetFormalUserList(), "UID", "NamePhone", user.UID);
                regReq.ReqAcptPerson = user.UID;
            }
            else
            {
                //sl = new SelectList(this.GetFormalUserList(), "UID", "NamePhone");
                regReq.ReqAcptPerson = 1;
            }

            ViewBag.UserList = new SelectList(this.GetFormalUserList(), "UID", "NamePhone");

            // 3、需求发起单位 
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            // 4、需求数量
            List<int> reqAmtLs = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                reqAmtLs.Add(i);
            }

            ViewBag.ReqAmtList = new SelectList(reqAmtLs);

            // 5、需求受理日期自动置为今天
            regReq.AcptDate = DateTime.Now;

            // 6、预先生成detail部分
            regReq.DetailRegReqs = new List<DetailRegReq>();
            for (int i = 0; i < 10; i++)
            {
                regReq.DetailRegReqs.Add(new DetailRegReq());
            }

            return View(regReq);
        }

        // 受理登记完成
        [HttpPost]
        public ActionResult RegResult(RegReq regReq)
        {
            List<Req> reqList = new List<Req>();
            for (int i = 0; i < regReq.ReqAmt; i++)
            {
                if (string.IsNullOrEmpty(regReq.DetailRegReqs[i].ReqDesc))
                {
                    continue;
                }

                Req newReq = new Req()
                {
                    SysID = regReq.SysID,
                    AcptDate = regReq.AcptDate,
                    ReqNo = regReq.ReqNo.Trim(), // 去空格
                    ReqReason = regReq.ReqReason.Trim(), // 去空格
                    ReqFromDept = regReq.ReqFromDept,
                    ReqFromPerson = regReq.ReqFromPerson,
                    ReqAcptPerson = regReq.ReqAcptPerson,
                    ReqBusiTestPerson = regReq.ReqBusiTestPerson,
                    DevWorkload = 0,

                    ReqDesc = regReq.DetailRegReqs[i].ReqDesc,
                    Remark = regReq.DetailRegReqs[i].Remark,
                };
                reqList.Add(newReq);
            }

            string r = "";

            try
            {
                // 登记
                foreach (Req req in reqList)
                {
                    // 状态默认「待评估」
                    req.ReqStat = (int)ReqStatEnums.待评估;
                    dbContext.Reqs.Add(req);

                    // 加入创建时间和更新时间
                    req.CreateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    req.UpdateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                }
                dbContext.SaveChanges();

                r = "<p class='alert alert-success'>登记入库成功！</p><p>您可以：</p><p><ul><li><a href='/ReqManage'>返回</a></li><li><a href='/ReqManage/MainReg'>继续登记</a></li></ul></p>";
            }
            catch (Exception e1)
            {
                r = "<p class='alert alert-danger'>登记入库失败！" + e1.Message + "</p>";
            }

            ViewBag.Msg = r;
            return View();
        }

        /*
         * 入池：【第一步】申请编号，研发联系人，研发受理日期，完成评估日期，【第二步】维护需求编号，关联业需编号，研发评估工作量
         * 
         * */

        public ActionResult InPool()
        {
            return View();
        }

        // 第二步
        [HttpPost]
        public ActionResult InPoolStep2(InPoolReq inPoolReq)
        {
            // 根据申请编号，找到相应的记录
            var rs = dbContext.Reqs.Where(a => a.ReqNo == inPoolReq.ReqNo);

            // 通用字段赋值，生成list
            List<Req> reqList = new List<Req>();
            foreach (Req req in rs)
            {
                req.DevAcptDate = inPoolReq.DevAcptDate;
                req.DevEvalDate = inPoolReq.DevEvalDate;
                req.ReqDevPerson = inPoolReq.ReqDevPerson;
                reqList.Add(req);
            }

            return View(reqList);
        }

        [HttpPost]
        public ActionResult InPoolResult(List<Req> reqList)
        {
            string r = "";

            string fail = ""; // 记录因需求编号重复入池失败的信息

            try
            {
                // 入库
                foreach (Req req in reqList)
                {
                    req.ReqDetailNo = req.ReqDetailNo.Trim();// 去空格

                    // 判断需求编号是否已存在
                    var checkReq = dbContext.Reqs.Where(p => p.ReqDetailNo == req.ReqDetailNo).Count();
                    if (checkReq > 0)
                    {
                        fail += req.ReqDetailNo + " ";
                        continue;
                    }

                    // 根据需求编号确定需求类型
                    req.ReqType = 0;
                    try
                    {
                        req.ReqType = int.Parse(req.ReqDetailNo.Split('-')[2]);
                    }
                    catch
                    {
                        // do nothing
                    }


                    // 状态默认为「入池」
                    req.ReqStat = (int)ReqStatEnums.入池;
                    // 更新时间
                    req.UpdateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 直接执行sql更新
                    string sql = "update Reqs set ReqDevPerson = @p0, DevAcptDate=@p1, DevEvalDate=@p2, ReqDetailNo=@p3, BusiReqNo=@p4, DevWorkload=@p5, ReqStat=@p6, ReqDesc=@p7, UpdateTime=@p8, ReqType=@p9 where RID=@p10";
                    dbContext.Database.ExecuteSqlCommand(sql, req.ReqDevPerson, req.DevAcptDate, req.DevEvalDate, req.ReqDetailNo, req.BusiReqNo, req.DevWorkload, (int)ReqStatEnums.入池, req.ReqDesc, req.UpdateTime, req.ReqType, req.RID);
                }

                if (fail == "")
                {
                    r = "<p class='alert alert-success'>入池成功！</p>";
                }
                else
                {
                    r = "<p class='alert alert-warning'>未能全部入池成功，" + fail + "需求编号重复，未入池。</p>";
                }
                r += "<p>您可以：</p><p><ul><li><a href='/ReqManage'>返回</a></li><li><a href='/ReqManage/InPool'>继续入池</a></li></ul></p>";
            }
            catch (Exception e1)
            {
                r = "<p class='alert alert-danger'>入池失败！" + e1.Message + "</p>";
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
                if (query.SysID != 0)
                {
                    ls = ls.Where(p => p.SysID == query.SysID);
                }
                if (!string.IsNullOrEmpty(query.AcptYear))
                {
                    ls = ls.Where(p => p.AcptDate.Value.Year.ToString() == query.AcptYear);
                }
                if (!string.IsNullOrEmpty(query.AcptMonth))
                {
                    ls = ls.Where(p => p.AcptDate.Value.Month.ToString() == query.AcptMonth);
                }
                if (!string.IsNullOrEmpty(query.ReqNo))
                {
                    ls = ls.Where(p => p.ReqNo == query.ReqNo.Trim());
                }
                if (!string.IsNullOrEmpty(query.ReqDetailNo))
                {
                    ls = ls.Where(p => p.ReqDetailNo == query.ReqDetailNo.Trim());
                }
                if (!string.IsNullOrEmpty(query.AnyRlsNo))
                {
                    ls = ls.Where(p => p.RlsNo == query.AnyRlsNo || p.SecondRlsNo == query.AnyRlsNo);
                }

                if (!string.IsNullOrEmpty(query.ReqStat))
                {
                    // 分『等于』和『不等于』2种情况
                    if (query.NotEqual)
                    {
                        ls = ls.Where(p => p.ReqStat.ToString() != query.ReqStat);
                    }
                    else
                    {
                        ls = ls.Where(p => p.ReqStat.ToString() == query.ReqStat);
                    }
                }
                if (query.ReqAcptPerson != 0)
                {
                    ls = ls.Where(p => p.ReqAcptPerson == query.ReqAcptPerson);
                }

                // 特殊查询：0-无 1-超过3个月未出池 2-超过8天未入池
                if (query.SpecialQuery == 1)
                {
                    DateTime time = DateTime.Now.AddMonths(-3);
                    ls = ls.Where(p => p.AcptDate.Value.CompareTo(time) <= 0);
                }

                else if (query.SpecialQuery == 2)
                {
                    DateTime time = DateTime.Now.AddDays(-8);
                    ls = ls.Where(p => p.AcptDate.Value.CompareTo(time) <= 0);
                }

                // 统一按照受理日期倒序
                ls = ls.OrderByDescending(p => p.AcptDate);

                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    RetailSystem s = new RetailSystem();
                    string targetFileName = "";

                    if (query.SysID != 0)
                    {
                        s = dbContext.RetailSystems.ToList().Find(a => a.SysID == query.SysID);
                        targetFileName = "维护需求查询_" + s.SysName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    else
                    {
                        targetFileName = "维护需求查询_所有系统_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    // 需要对list修改以适应Excel模板
                    List<ReqExcel> excelList = this.GetExcelList(ls);
                    return this.MakeExcel<ReqExcel>("ReqReportT", targetFileName, excelList);
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
            List<RetailSystem> sysList = this.GetSysList(); // 仅查询时可以选择所有系统
            // 加上“全部”
            sysList.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            ViewBag.SysList = new SelectList(sysList, "SysID", "SysName", query.SysID);

            // 需求受理人下拉
            List<User> userList = this.GetFormalUserList();
            // 加上“全部”
            userList.Insert(0, new User() { UID = 0, Realname = "全部" });
            ViewBag.ReqAcptPerson = new SelectList(userList, "UID", "Realname", query.ReqAcptPerson);

            // 需求状态下拉
            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(typeof(ReqStatEnums), true, true, query.ReqStat);

            return View(query);
        }

        /*
         * 【3】批处理
         */

        // 批处理统一入口
        public ActionResult Bat()
        {
            // 需求状态下拉
            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(enumType: typeof(ReqStatEnums), forQuery: true, emptyText: "不更新");
            return View();
        }

        // 2016年8月16日：批量功能统一为「批量更新」
        // Ajax调用，批量更新       
        [HttpPost]
        public string BatProc(string reqs, string version, string outDate, string planRlsDate, string rlsNo, string secondRlsNo, string rlsDate, string secondRlsDate,
            string remark, string reqStat, string acptDate, bool clearAcptDate)
        {
            // 若填写了下发日期，则下发状态应该为「办结」
            if ((!string.IsNullOrEmpty(rlsDate) || !string.IsNullOrEmpty(secondRlsDate)) && reqStat != ReqStatEnums.办结.ToString())
            {
                return "<p class='alert alert-danger'>出错了：填写了下发日期的情况下，需求状态必须为「办结」</p>"; 
            }

            // 拼出sql中的in条件
            string whereIn = this.GetWhereIn(reqs);

            // 仅更新不为空的
            int updateFiledNum = 0;

            StringBuilder sb = new StringBuilder("update Reqs set SysID=SysID");

            if (clearAcptDate)
            {
                sb.Append(", AcptDate=NULL");
                updateFiledNum++;
            }
            else if (!string.IsNullOrEmpty(acptDate))
            {
                sb.Append(", AcptDate='" + acptDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(version))
            {
                sb.Append(", Version='" + version + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(outDate))
            {
                sb.Append(", OutDate='" + outDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(planRlsDate))
            {
                sb.Append(", PlanRlsDate='" + planRlsDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(rlsNo))
            {
                sb.Append(", RlsNo='" + rlsNo + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(secondRlsNo))
            {
                sb.Append(", SecondRlsNo='" + secondRlsNo + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(rlsDate))
            {
                sb.Append(", RlsDate='" + rlsDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(secondRlsDate))
            {
                sb.Append(", SecondRlsDate='" + secondRlsDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(remark))
            {
                sb.Append(", Remark=N'" + remark + "'");
                updateFiledNum++;
            }

            if (reqStat != "不更新")
            {
                sb.Append(", ReqStat=N'" + reqStat + "'");
                updateFiledNum++;
            }

            if (updateFiledNum == 0)
            {
                return "<p class='alert alert-info'>因所有输入框为空，未更新任何信息！</p>";
            }

            sb.Append(string.Format(", UpdateTime='{0}' {1}", DateTime.Now.ToString("yyyyMMddHHmmss"), whereIn));

            try
            {
                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sb.ToString());

                return "<p class='alert alert-success'>已更新" + r + "条记录！</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        // Ajax调用，批量删除       
        [HttpPost]
        public string BatDel(string reqs)
        {

            // 拼出sql中的in条件
            string whereIn = this.GetWhereIn(reqs);

            string sql = string.Format("delete from Reqs {0}", whereIn);

            try
            {
                // 批量删除，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                return "<p class='alert alert-success'>已删除" + r + "条记录!<p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        /*
         * 【4】单笔新增
         */
        public ActionResult Create(int id = 0)
        {
            // 1、生成系统列表
            List<RetailSystem> ls1 = this.GetNormalSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName", id); // 选中传进来的值

            ViewBag.SysList = sl1;

            // 2、生成需求受理人列表，默认当前用户为需求受理人 
            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetFormalUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetFormalUserList(), "UID", "NamePhone");
            }

            ViewBag.UserList = sl2;

            // 3、需求发起单位 
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            // 4、需求类型下拉列表
            //ViewBag.ReqTypeList = MyTools.GetSelectListByEnum(typeof(ReqTypeEnum), false, true);

            // 5、需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(typeof(ReqStatEnums));

            return View();
        }

        // ajax调用
        [HttpPost]
        public string Create(Req req)
        {
            try
            {
                // 若填写了下发日期，则下发状态应该为「办结」
                if ((req.RlsDate != null || req.SecondRlsDate != null) && req.ReqStat != (int)ReqStatEnums.办结)
                {
                    throw new Exception("填写了下发日期的情况下，需求状态必须为「办结」");
                }

                // 判断是否有重复的维护需求编号，如有重复不允许新增
                if (!string.IsNullOrEmpty(req.ReqDetailNo))
                {
                    Req r = dbContext.Reqs.ToList().Find(a => a.ReqDetailNo == req.ReqDetailNo);
                    if (r != null)
                    {
                        throw new Exception("维护需求编号" + req.ReqDetailNo + "已存在，不允许重复添加！");
                    }
                }

                // 去除空格：
                string reqNo = req.ReqNo;
                string reqDetailNo = req.ReqDetailNo;

                req.ReqNo = reqNo.Trim();
                req.ReqDetailNo = string.IsNullOrEmpty(reqDetailNo) ? "" : reqDetailNo.Trim();

                // 根据需求编号确定需求类型
                req.ReqType = 0;
                try
                {
                    req.ReqType = int.Parse(req.ReqDetailNo.Split('-')[2]);
                }
                catch
                {
                    // do nothing
                }

                // 加入创建时间和更新时间
                req.CreateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                req.UpdateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                dbContext.Reqs.Add(req);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
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
                return View();
            }

            // 下拉框预处理
            // 1、生成系统列表
            List<RetailSystem> ls1 = this.GetNormalSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName", req.SysID);

            ViewBag.SysList = sl1;

            // 2、生成需求受理人列表
            SelectList sl2 = null;

            sl2 = new SelectList(this.GetFormalUserList(), "UID", "NamePhone", req.ReqAcptPerson);

            ViewBag.UserList = sl2;

            // 4、需求发起单位 
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList, false, true, req.ReqFromDept);

            // 5、需求类型下拉列表
            //ViewBag.ReqTypeList = MyTools.GetSelectListByEnum(typeof(ReqTypeEnum), false, true);

            // 6、需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(typeof(ReqStatEnums), false, true, req.ReqStat.ToString());

            req.OldReqDetailNo = req.ReqDetailNo;
            return View(req);
        }

        [HttpPost]
        public string Edit(Req req)
        {
            try
            {
                // 若填写了下发日期，则下发状态应该为「办结」
                if ((req.RlsDate != null || req.SecondRlsDate != null) && req.ReqStat != (int)ReqStatEnums.办结)
                {
                    throw new Exception("填写了下发日期的情况下，需求状态必须为「办结」");
                }

                // 判断有无重复需求编号
                if (!string.IsNullOrEmpty(req.ReqDetailNo) && req.ReqDetailNo != req.OldReqDetailNo)
                {
                    Req r = dbContext.Reqs.Where(a => a.ReqDetailNo == req.ReqDetailNo).FirstOrDefault();
                    if (r != null)
                    {
                        throw new Exception("维护需求编号" + r.ReqDetailNo + "已存在，不允许更新！");
                    }
                }


                // 根据需求编号确定需求类型
                req.ReqType = 0;
                try
                {
                    req.ReqType = int.Parse(req.ReqDetailNo.Split('-')[2]);
                }
                catch
                {
                    // do nothing
                }

                // 更新时间
                req.UpdateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                dbContext.Entry(req).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
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
                if (query.SysID != 0)
                {
                    ls = ls.Where(p => p.SysID == query.SysID);
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

                // 将查询结果转换为OutPoolResult和OutPoolResultExcel（避免多出来的short字段影响）
                List<OutPoolResult> resultList = new List<OutPoolResult>();
                List<OutPoolResultExcel> resultExcelList = new List<OutPoolResultExcel>();
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
                        ReqType = req.ReqTypeName,
                        PlanRlsDate = req.PlanRlsDate,
                        RlsDate = req.RlsDate,
                        Remark = req.Remark
                    };
                    resultList.Add(res);

                    OutPoolResultExcel resExcel = new OutPoolResultExcel()
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
                        ReqType = req.ReqTypeName,
                        PlanRlsDate = req.PlanRlsDate,
                        RlsDate = req.RlsDate,
                        Remark = req.Remark
                    };

                    resultExcelList.Add(resExcel);
                }
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "零售条线出池计划";
                    if (query.SysID != 0)
                        targetFileName += "_" + resultExcelList[0].SysName;
                    if (!string.IsNullOrEmpty(query.Version))
                        targetFileName += "_" + query.Version;
                    if (!string.IsNullOrEmpty(query.MaintainYear))
                        targetFileName += "_" + query.MaintainYear;
                    return this.MakeExcel<OutPoolResultExcel>("OutPoolReportT", targetFileName, resultExcelList);
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
            List<RetailSystem> ls1 = this.GetNormalSysList();
            // 加上“全部”
            ls1.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            SelectList sl1 = new SelectList(ls1, "SysID", "SysName", query.SysID);
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

            // 分别判断是申请编号还是维护需求编号
            StringBuilder reqNos = new StringBuilder();
            StringBuilder reqDetailNos = new StringBuilder();

            foreach (string s in reqArr)
            {
                if (s.Length == 15)
                {
                    reqNos.Append(string.Format("'{0}',", s));
                }
                else
                {
                    reqDetailNos.Append(string.Format("'{0}',", s));
                }

            }

            // 三种情况：1、只有申请编号；2、只有需求编号；3、两者皆有 （两者皆无会直接拦截）
            string sql = " where ";
            if (reqNos.Length > 0 && reqDetailNos.Length == 0)
            {
                string reqNosResult = reqNos.ToString();
                sql += string.Format("ReqNo in ({0})", reqNosResult.Substring(0, reqNosResult.Length - 1));
            }

            else if (reqNos.Length == 0 && reqDetailNos.Length > 0)
            {
                string reqDetailNosResult = reqDetailNos.ToString();
                sql += string.Format("ReqDetailNo in ({0})", reqDetailNosResult.Substring(0, reqDetailNosResult.Length - 1));
            }

            else
            {
                string reqNosResult = reqNos.ToString();
                string reqDetailNosResult = reqDetailNos.ToString();

                sql += string.Format("ReqNo in ({0}) or ReqDetailNo in ({1})", reqNosResult.Substring(0, reqNosResult.Length - 1),
                    reqDetailNosResult.Substring(0, reqDetailNosResult.Length - 1));
            }

            return sql;
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
                    ReqType = s.ReqTypeName,
                    DevWorkload = s.DevWorkload,
                    ReqStat = s.ReqStatName,
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

        public ActionResult Details(int id)
        {
            List<Req> ls = dbContext.Reqs.ToList();
            Req req = ls.Find(a => a.RID == id);

            if (req == null)
            {
                return View();
            }

            return View(req);
        }

        /*
         * 出池与下发功能改进
         * */

        public ActionResult QuickOutPool()
        {
            if (this.GetSessionCurrentUser() == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/ReqManage/QuickOutPool" });
            }

            // 提供系统列表
            List<RetailSystem> ls1 = this.GetNormalSysList();
            // 加上“请选择系统”
            ls1.Insert(0, new RetailSystem() { SysID = 0, SysName = "--请选择系统--" });
            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");
            ViewBag.SysList = sl1;

            return View();
        }

        [HttpPost]
        public string QuickOutPool(string Reqs, string Version, string OutDate, string PlanRlsDate, int SysID, bool IsPatch)
        {
            // 2016年8月10日修改：需要根据IsPatch分别进行不同的处理
            string realVersion = Version;
            if (!IsPatch) // 常规版本
            {
                Ver v = dbContext.Vers.Where(p => p.VerID.ToString() == Version).FirstOrDefault();
                realVersion = v.VerNo;
                // 更新版本计划信息
                v.DraftTime = DateTime.Parse(PlanRlsDate); // 制定时间改为计划下发日期
                v.DraftPersonID = this.GetSessionCurrentUser().UID;
                dbContext.Entry(v).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
            }
            else // 补丁版本
            {

                // 新增一条记录
                DateTime newTime = DateTime.Parse(PlanRlsDate);
                Ver v = new Ver()
                {
                    SysID = SysID,
                    VerNo = realVersion,
                    VerYear = DateTime.Now.Year.ToString(),
                    ReleaseFreq = 0, // 补丁版本的频率记为0
                    DraftTime = newTime,
                    PublishTime = newTime,// 发布时间、制定时间均为计划下发日期
                    DraftPersonID = this.GetSessionCurrentUser() == null ? 0 : this.GetSessionCurrentUser().UID,
                    VerType = "补丁版本"
                };
                dbContext.Vers.Add(v);
                dbContext.SaveChanges();
            }

            string sql = string.Format("update Reqs set Version='{0}', OutDate='{1}', PlanRlsDate='{2}', ReqStat={3}, UpdateTime='{4}' where RID in ({5})", realVersion, OutDate, PlanRlsDate, (int)ReqStatEnums.出池, DateTime.Now.ToString("yyyyMMddHHmmss"), Reqs);
            try
            {
                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                // 下载出池计划文档接口
                string downfile = string.Format("/ReqManage/OutPool?isQuery=True&isExcel=True&SysID={0}&Version={1}", SysID, realVersion);

                return string.Format("<p class='alert alert-success'>" + r + "条需求成功出池！<a href='{0}'>点击</a>导出出池计划文档<p>", downfile);
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger>出错了：" + e1.Message + "</p>";
            }
        }

        [HttpPost]
        public string QuickUpdateRlsNo(string Reqs, string RlsNo, string SecondRlsNo)
        {
            // 保证副下发为NULL
            if (!string.IsNullOrEmpty(SecondRlsNo))
            {
                SecondRlsNo = "SecondRlsNo='" + SecondRlsNo + "', ";
            }

            string sql = string.Format("update Reqs set RlsNo='{0}', {1} UpdateTime='{2}' where RID in ({3})", RlsNo, SecondRlsNo, DateTime.Now.ToString("yyyyMMddHHmmss"), Reqs);
            try
            {
                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sql);

                return "<p class='alert alert-success'>" + r + "条记录更新成功！";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger>出错了：" + e1.Message + "</p>";
            }
        }

        /// <summary>
        /// Ajax接口，根据SysID获取可以出池的需求列表
        /// </summary>
        /// <param name="sysID"></param>
        /// <returns></returns>
        public string GetReqsToOutPool(int sysID)
        {
            if (sysID == 0)
            {
                return "<select id=\"Reqs\" name=\"Reqs\" multiple=\"multiple\" class=\"form-control\" size=\"1\"><option>--请选择系统--</option></select>";
            }

            List<Req> list = dbContext.Reqs.Where(p => p.SysID == sysID && p.ReqStat == (int)ReqStatEnums.入池).ToList();


            int size = list.Count;

            if (size == 0)
            {
                return "<select id=\"Reqs\" name=\"Reqs\" multiple=\"multiple\" class=\"form-control\" size=\"1\"><option>--无可出池需求--</option></select>";
            }

            StringBuilder sb = new StringBuilder("<select id=\"Reqs\" name=\"Reqs\" multiple=\"multiple\" class=\"form-control\" size=\"5\">");

            foreach (Req r in list)
            {
                sb.Append(string.Format("<option value='{0}'>{1}</option>", r.RID, r.ReqDetailNo));
            }

            sb.Append("</select>");

            return sb.ToString();

        }

        // 2016年8月10日 新增

        /// <summary>
        /// Ajax接口，根据SysID获取可以出池的版本
        /// </summary>
        /// <param name="sysID"></param>
        /// <returns></returns>
        public string GetVersToOutPool(int sysID)
        {
            if (sysID == 0)
            {
                return "<option>--请选择系统--</option>";
            }

            List<Ver> list = dbContext.Vers.Where(p => p.SysID == sysID && p.VerYear == DateTime.Now.Year.ToString() && p.VerType == "计划版本").ToList();


            int size = list.Count;

            if (size == 0)
            {
                return "<option value=0>--未找到版本计划--</option>";
            }

            StringBuilder sb = new StringBuilder();

            foreach (Ver r in list)
            {
                sb.Append(string.Format("<option value='{0}'>{1}</option>", r.VerID, r.VerNo));
            }

            sb.Append("</select>");

            return sb.ToString();

        }

        // 2016年8月16日 新增：更新实际下发日期重做
        public ActionResult UpdateRlsDate()
        {
            return View();
        }

        [HttpPost]
        public string UpdateRlsDate(string rlsNo, string rlsDate, string secondRlsDate)
        {

            StringBuilder sb = new StringBuilder("update Reqs set SysID=SysID");

            if (!string.IsNullOrEmpty(rlsDate))
            {
                sb.Append(", RlsDate='" + rlsDate + "'");
            }

            if (!string.IsNullOrEmpty(secondRlsDate))
            {
                sb.Append(", SecondRlsDate='" + secondRlsDate + "'");
            }

            sb.Append(string.Format(", ReqStat = {3}, UpdateTime='{0}' where RlsNo = '{1}' or SecondRlsNo='{2}'", DateTime.Now.ToString("yyyyMMddHHmmss"), rlsNo, rlsNo, (int)ReqStatEnums.办结));

            try
            {
                // 批量更新，直接执行SQL
                int r = dbContext.Database.ExecuteSqlCommand(sb.ToString());

                return "<p class='alert alert-success'>已更新" + r + "条记录！</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }
    }
}
