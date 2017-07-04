using MyTeam.Models;
using MyTeam.Utils;
using MyTeam.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using PagedList;
using System.Collections;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 20170622新维护需求管理
    /// </summary>
    /**
     * 1、第一步：需求登记
     * 2、第二步：需求入池
     * 3、第三步：需求出池与下发
     * 4、第四步：更新下发日期
     * 
     * 5、需求管理
     * 6、下发管理
     * 7、导出下发报表
     **/
    public class ReqManageController : BaseController
    {
        /**
         * 1、需求登记
         **/
        #region 需求登记

        // GET: /ReqManage/Reg
        //
        [HttpGet]
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
            ViewBag.ReqFromDeptList = MyTools.GetSelectListBySimpleEnum(typeof(ReqFromDeptEnums));

            // 4、需求数量
            List<int> reqAmtLs = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                reqAmtLs.Add(i);
            }

            ViewBag.ReqAmtList = new SelectList(reqAmtLs);

            // 5、需求受理日期自动置为今天
            // regReq.AcptDate = DateTime.Now; //del:现在默认先不填受理日期了

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
            string r = ""; //记录处理结果
            try
            {
                //判断ReqNo是否重复
                var checkMain = dbContext.ReqMains.Where(p => p.ReqNo == regReq.ReqNo).FirstOrDefault();
                if (checkMain != null)
                {
                    throw new Exception(string.Format("需求申请编号{0}已经存在！", regReq.ReqNo));
                }

                // 分别登记ReqMain和ReqDetail                

                //登记Main
                ReqMain reqMain = new ReqMain
                {
                    SysID = regReq.SysID,
                    AcptDate = regReq.AcptDate,
                    ReqNo = regReq.ReqNo.Trim(), // 去空格
                    ReqReason = regReq.ReqReason.Trim(), // 去空格
                    ReqFromDept = regReq.ReqFromDept,
                    ReqFromPerson = regReq.ReqFromPerson,
                    ReqAcptPerson = regReq.ReqAcptPerson,
                    ReqBusiTestPerson = regReq.ReqBusiTestPerson
                };
                dbContext.ReqMains.Add(reqMain);

                // 登记Detail
                List<ReqDetail> reqList = new List<ReqDetail>();
                for (int i = 0; i < regReq.ReqAmt; i++)
                {
                    ReqDetail newReq = new ReqDetail()
                    {
                        DevWorkload = 0,
                        ReqDesc = regReq.DetailRegReqs[i].ReqDesc,
                        Remark = regReq.DetailRegReqs[i].Remark,
                        // 状态默认「待评估」
                        ReqStat = (int)ReqStatEnums.待评估,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        ReqMain = reqMain
                    };

                    dbContext.ReqDetails.Add(newReq);
                }

                dbContext.SaveChanges();

                r = string.Format("<p class='alert alert-success'>共{0}条需求登记入库成功！</p><p>您可以：</p><p><ul><li><a href='/ReqManage'>返回</a></li><li><a href='/ReqManage/Reg'>继续登记</a></li></ul></p>", regReq.ReqAmt);
            }
            catch (Exception e1)
            {
                r = string.Format("<p class='alert alert-danger'>登记入库失败！错误信息：{0}</p>", e1.Message);
            }

            ViewBag.Msg = r;
            return View();
        }

        #endregion


        /**
         * 2、需求入池
         * 补充如下信息：
         * 申请编号，研发联系人，研发受理日期，完成评估日期，  
         * 维护需求编号，关联业需编号，研发评估工作量（多条）
         * （页面选择系统与未入池的需求申请，载入详情，补充信息）
         * */
        #region 需求入池
        // GET: /ReqManage/InPool
        //
        [HttpGet]
        public ActionResult InPool()
        {
            // 提供系统列表
            List<RetailSystem> ls1 = this.GetNormalSysList();
            // 加上“请选择系统”
            ls1.Insert(0, new RetailSystem() { SysID = 0, SysName = "--请选择系统--" });
            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");
            ViewBag.SysList = sl1;

            return View();
        }

        [HttpGet]
        public ActionResult InPoolDetail(string reqNo)
        {
            var main = dbContext.ReqMains.Where(a => a.ReqNo == reqNo).FirstOrDefault();
            if (main == null)
            {
                ViewBag.ErrMsg = "找不到对应的需求申请记录，申请编号：" + reqNo;
            }

            InPoolReq model = new InPoolReq { ReqMain = main, ReqDetails = main.ReqDetails.ToList() };

            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(typeof(ReqStatEnums), false, true, "2");

            return View(model);
        }

        [HttpPost]
        public ActionResult InPoolResult(InPoolReq inPoolReq)
        {
            string msg = "";
            string fail = "";
            string repeat = "";

            try
            {
                var main = dbContext.ReqMains.Find(inPoolReq.ReqMain.ReqMainID);

                // 判断是否更新需求申请编号与受理日期
                if (!string.IsNullOrEmpty(inPoolReq.NewReqNo))
                {
                    //判断ReqNo是否重复
                    var checkMain = dbContext.ReqMains.Where(p => p.ReqNo == inPoolReq.NewReqNo).FirstOrDefault();
                    if (checkMain != null)
                    {
                        throw new Exception(string.Format("需求申请编号{0}已经存在！", inPoolReq.NewReqNo));
                    }
                    main.ReqNo = inPoolReq.NewReqNo;
                }

                if (inPoolReq.NewAcptDate != null)
                {
                    main.AcptDate = inPoolReq.NewAcptDate;
                }

                main.ReqDevPerson = inPoolReq.ReqMain.ReqDevPerson;
                main.DevAcptDate = inPoolReq.ReqMain.DevAcptDate;
                main.DevEvalDate = inPoolReq.ReqMain.DevEvalDate;

                dbContext.Entry(main).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                var reqList = inPoolReq.ReqDetails;

                foreach (var detail in reqList)
                {
                    var model = dbContext.ReqDetails.Find(detail.ReqDetailID);
                    var detailNo = detail.ReqDetailNo.Trim();// 去空格

                    var checkReq = dbContext.ReqDetails.Where(p => p.ReqDetailNo == detailNo).FirstOrDefault();

                    // 判断需求编号是否已存在
                    if (checkReq != null)
                    {
                        repeat += detailNo + " ";
                        continue;
                    }

                    model.ReqDetailNo = detailNo;

                    // 根据需求编号确定需求类型
                    // model.ReqType = 0;
                    try
                    {
                        model.ReqType = int.Parse(model.ReqDetailNo.Split('-')[2]);
                    }
                    catch
                    {
                        fail += model.ReqDetailNo + " ";
                        continue;
                    }

                    model.ReqStat = detail.ReqStat;
                    model.DevWorkload = detail.DevWorkload;


                    // 更新时间
                    model.UpdateTime = DateTime.Now;

                    dbContext.Entry(model).State = System.Data.Entity.EntityState.Modified;

                    dbContext.SaveChanges();
                }


                if (fail + repeat == "")
                {
                    msg = "<p class='alert alert-success'>入池成功！</p>";
                }
                else
                {
                    msg = string.Format("<p class='alert alert-warning'>未能全部入池成功：<br /> 需求编号重复：{0}<br />需求编号格式错误：{1}</p>", repeat, fail);
                }
                msg += "<p>您可以：</p><p><ul><li><a href='/ReqManage'>返回</a></li><li><a href='/ReqManage/InPool'>继续入池</a></li></ul></p>";

            }
            catch (Exception e1)
            {
                msg = string.Format("<p class='alert alert-danger'>需求入池失败！错误信息：{0}</p>", e1.Message);
            }

            ViewBag.Msg = msg;

            return View();
        }

        /// <summary>
        /// Ajax接口，根据SysID获取可以入池的需求申请编号列表
        /// </summary>
        /// <param name="sysID"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetReqNosToInPool(int sysID)
        {
            if (sysID == 0)
            {
                return "<option value=''>--请选择系统--</option>";
            }


            List<string> list = dbContext.ReqMains.Where(p => p.SysID == sysID && p.DevEvalDate == null).Select(p => p.ReqNo).ToList();

            int size = list.Count;

            if (size == 0)
            {
                return "<option value=''>--无待入池需求--</option>";
            }

            StringBuilder sb = new StringBuilder();

            foreach (var r in list)
            {
                sb.Append(string.Format("<option value='{0}'>{1}</option>", r, r));
            }

            return sb.ToString();

        }

        #endregion

        /**
         * 3、需求出池
         * */

        #region 需求出池
        // GET: /ReqManage/OutPool
        //
        [HttpGet]
        public ActionResult OutPool()
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
        public string OutPool(string Reqs, string Version, string OutDate, string PlanReleaseDate, int SysID, bool IsPatch)
        {
            try
            {
                // 2016年8月10日修改：需要根据IsPatch分别进行不同的处理
                string realVersion = Version;
                if (!IsPatch) // 常规版本
                {
                    Ver v = dbContext.Vers.Where(p => p.VerID.ToString() == Version).FirstOrDefault();
                    realVersion = v.VerNo;
                    // 更新版本计划信息
                    v.DraftTime = DateTime.Parse(PlanReleaseDate); // 制定时间改为计划下发日期
                    v.DraftPersonID = this.GetSessionCurrentUser().UID;
                    dbContext.Entry(v).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                }
                else // 补丁版本
                {

                    // 新增一条记录
                    DateTime newTime = DateTime.Parse(PlanReleaseDate);
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

                string sql = string.Format("Update ReqDetails set Version='{0}', OutDate='{1}', ReqStat='{2}', UpdateTime='{3}' where ReqDetailID in ({4})",
                    realVersion, OutDate, (int)ReqStatEnums.出池, DateTime.Now.ToString("yyyy/M/d hh:mm:ss"), Reqs);

                int updatedNum = dbContext.Database.ExecuteSqlCommand(sql);

                // 下载出池计划文档接口
                string downfile = string.Format("/ReqManage/OutPoolTable?isQuery=True&isExcel=True&SysID={0}&Version={1}", SysID, realVersion);

                return string.Format("<p class='alert alert-success'>{0}条需求成功出池！<a href='{1}'>点击</a>导出出池计划文档</p>", updatedNum, downfile);
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        /// <summary>
        /// Ajax接口，根据SysID获取可以出池的需求列表
        /// </summary>
        /// <param name="sysID"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetReqsToOutPool(int sysID)
        {
            if (sysID == 0)
            {
                return "<select id=\"Reqs\" name=\"Reqs\" multiple=\"multiple\" class=\"form-control\" size=\"1\"><option>--请选择系统--</option></select>";
            }

            var list = (from detail in dbContext.ReqDetails
                        join main in dbContext.ReqMains on detail.ReqMainID equals main.ReqMainID
                        where main.SysID == sysID && detail.ReqStat == (int)ReqStatEnums.入池
                        select detail).ToList();

            int size = list.Count;

            if (size == 0)
            {
                return "<select id=\"Reqs\" name=\"Reqs\" multiple=\"multiple\" class=\"form-control\" size=\"1\"><option>--无可出池需求--</option></select>";
            }

            if (size > 10)
            {
                size = 10;
            }

            StringBuilder sb = new StringBuilder("<select id=\"Reqs\" name=\"Reqs\" multiple=\"multiple\" class=\"form-control\" size=\"" + size + "\">");

            foreach (ReqDetail r in list)
            {
                sb.Append(string.Format("<option value='{0}'>{1}</option>", r.ReqDetailID, r.ReqDetailNo));
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
        [HttpGet]
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

        /// <summary>
        /// 更新下发编号
        /// </summary>
        /// <param name="Reqs"></param>
        /// <param name="ReleaseNo"></param>
        /// <param name="SideReleaseNo"></param>
        /// <param name="PlanReleaseDate"></param>
        /// <returns></returns>
        [HttpPost]
        public string UpdateReleaseNo(string Reqs, string ReleaseNo, string SideReleaseNo, string PlanReleaseDate)
        {
            string msg = "";

            string sideReleaseSql = ""; // 更新副下发的sql

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
                        ReleaseNo = ReleaseNo
                    };
                    dbContext.ReqReleases.Add(release);
                    dbContext.SaveChanges();

                    reqReleaseID = release.ReqReleaseID;

                    msg = "<p class='alert alert-success'>添加下发记录" + ReleaseNo + "成功</p>";
                }

                // 若存在副下发，则继续添加副下发的release记录
                int sideReqReleaseID;
                if (!string.IsNullOrEmpty(SideReleaseNo))
                {
                    checkRelease = dbContext.ReqReleases.Where(p => p.ReleaseNo == SideReleaseNo).FirstOrDefault();
                    if (checkRelease != null)
                    {
                        // 已存在则更新
                        checkRelease.PlanReleaseDate = DateTime.Parse(PlanReleaseDate);
                        checkRelease.ReleaseNo = SideReleaseNo;
                        checkRelease.IsSideRelease = true;
                        dbContext.Entry(checkRelease).State = System.Data.Entity.EntityState.Modified;
                        dbContext.SaveChanges();

                        sideReqReleaseID = checkRelease.ReqReleaseID;

                        msg += "<p class='alert alert-warning'>下发编号" + SideReleaseNo + "已存在，对现有记录进行了更新！</p>";
                    }
                    else
                    {
                        // 不存在则新增
                        var release = new ReqRelease
                        {
                            PlanReleaseDate = DateTime.Parse(PlanReleaseDate),
                            ReleaseNo = SideReleaseNo,
                            IsSideRelease = true,
                        };
                        dbContext.ReqReleases.Add(release);
                        dbContext.SaveChanges();

                        sideReqReleaseID = release.ReqReleaseID;

                        msg += "<p class='alert alert-success'>添加副下发记录" + SideReleaseNo + "成功</p>";
                    }

                    // 补充更新副下发的sql
                    sideReleaseSql = string.Format("SecondReqReleaseID = '{0}', ", sideReqReleaseID);
                }

                // 将下发ID更新到req中
                string sql = string.Format("update ReqDetails set ReqReleaseID = '{0}', {1} UpdateTime='{2}'  where ReqDetailID in ({3})", reqReleaseID, sideReleaseSql, DateTime.Now.ToString("yyyy/M/d hh:mm:ss"), Reqs);

                int updatedNum = dbContext.Database.ExecuteSqlCommand(sql);

                msg += "<p class='alert alert-success'>" + updatedNum + "条需求更新成功！</p>";

            }
            catch (Exception e1)
            {
                msg += "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }

            return msg;
        }

        #endregion

        /**
         * 4、更新下发日期
         * */
        #region 更新下发日期
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
                if (!r.IsSideRelease)
                {
                    var sql = string.Format("Update ReqDetails set ReqStat = '{0}' where ReqReleaseID = {1}", ReqStatEnums.办结, r.ReqReleaseID);
                    dbContext.Database.ExecuteSqlCommand(sql);
                    msg += "；同时已更新相关维护需求的状态为【办结】";
                }

                return "<p class='alert alert-success'>" + msg + "</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了：" + e1.Message + "</p>";
            }
        }

        #endregion

        /**
         * 5、维护需求管理
         * 增删改查，Main与Detail联立管理
         * */
        #region 维护需求管理

        // 默认页为查询页
        // 按照查询条件查询结果：为使用分页功能，GET模式查询 
        [HttpGet]
        public ActionResult Index(ReqQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {
            var releaseList = dbContext.ReqReleases.ToList(); // 下发记录，备用
            if (isQuery)
            {
                var ls = from a in dbContext.ReqDetails
                         select a;
                if (query.SysID != 0)
                {
                    ls = ls.Where(p => p.ReqMain.SysID == query.SysID);
                }
                if (!string.IsNullOrEmpty(query.AcptDateStart))
                {
                    var acptDateStart = DateTime.Parse(query.AcptDateStart);
                    ls = ls.Where(p => p.ReqMain.AcptDate.Value >= acptDateStart);
                }
                if (!string.IsNullOrEmpty(query.AcptDateEnd))
                {
                    var acptDateEnd = DateTime.Parse(query.AcptDateEnd);
                    ls = ls.Where(p => p.ReqMain.AcptDate.Value <= acptDateEnd);
                }
                if (!string.IsNullOrEmpty(query.ReqNo))
                {
                    ls = ls.Where(p => p.ReqMain.ReqNo.Contains(query.ReqNo.Trim()));
                }

                if (!string.IsNullOrEmpty(query.ReqStat))
                {
                    ls = ls.Where(p => p.ReqStat.ToString() == query.ReqStat);
                }
                if (query.ReqAcptPerson != 0)
                {
                    ls = ls.Where(p => p.ReqMain.ReqAcptPerson == query.ReqAcptPerson);
                }

                if (!string.IsNullOrEmpty(query.ReqDetailNo))
                {
                    ls = ls.Where(p => p.ReqDetailNo.Contains(query.ReqDetailNo.Trim()));
                }

                if (!string.IsNullOrEmpty(query.AnyReleaseNo))
                {
                    var rls = releaseList.Find(p => p.ReleaseNo == query.AnyReleaseNo);
                    var rlsID = rls == null ? -1 : rls.ReqReleaseID;
                    ls = ls.Where(p => p.ReqReleaseID == rlsID || p.SecondReqReleaseID == rlsID);
                }

                // 统一按照创建日期倒序
                ls = ls.OrderByDescending(p => p.CreateTime);

                var list = ls.ToList();

                // 补充信息
                foreach (var req in list)
                {
                    // 下发通知编号
                    if (req.ReqReleaseID == 0)
                    {
                        req.ReqReleaseNo = "暂无";
                    }
                    else
                    {
                        var rls = releaseList.Find(p => p.ReqReleaseID == req.ReqReleaseID);
                        if (rls == null)
                        {
                            req.ReqReleaseNo = "找不到对应下发记录";
                        }
                        else
                        {
                            req.ReqReleaseNo = rls.ReleaseNo;
                            // 找相关的副下发
                            var sideRls = releaseList.Find(p => p.ReqReleaseID == req.SecondReqReleaseID);
                            if (sideRls != null)
                            {
                                req.ReqReleaseNo += "（主）" + sideRls.ReleaseNo + "（副）";
                            }
                        }
                    }

                    // 维护需求编号
                    if (string.IsNullOrEmpty(req.ReqDetailNo))
                        req.ReqDetailNo = "暂无";

                }

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

                    // Excel使用FullReq
                    List<FullReq> fullList = new List<FullReq>();
                    foreach (var d in list)
                    {
                        var fullReq = new FullReq
                        {
                            reqMain = d.ReqMain,
                            reqDetail = d,
                            reqRelease = releaseList.Find(p => p.ReqReleaseID == d.ReqReleaseID)
                        };
                        fullList.Add(fullReq);
                    }
                    // 需要对list修改以适应Excel模板
                    List<ReqExcel> excelList = this.GetExcelList(fullList);
                    return this.MakeExcel<ReqExcel>("ReqReportT", targetFileName, excelList);
                }
                else
                {
                    // 分页
                    query.ResultList = list.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE);
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

        // GET: /ReqManage/Details
        // 详情查询
        [HttpGet]
        public ActionResult Details(int id)
        {
            var fullReq = new FullReq();

            var req = dbContext.ReqDetails.Find(id);

            if (req != null)
            {
                fullReq.reqDetail = req;
                fullReq.reqMain = req.ReqMain;
                fullReq.reqReleases = dbContext.ReqReleases.Where(p => p.ReqReleaseID == req.ReqReleaseID || p.ReqReleaseID == req.SecondReqReleaseID).ToList();
            }

            return View(fullReq);
        }

        // GET:/ReqManage/EditMain
        // 编辑需求申请部分
        [HttpGet]
        public ActionResult EditMain(int id)
        {
            var req = dbContext.ReqMains.Find(id);
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
            ViewBag.ReqFromDeptList = MyTools.GetSelectListBySimpleEnum(typeof(ReqFromDeptEnums));

            // 记录原始reqno
            req.OldReqNo = req.ReqNo;

            return View(req);
        }

        [HttpPost]
        public string EditMain(ReqMain req)
        {
            if (req.ReqNo != req.OldReqNo) // reqNo不可为空
            {
                var main = dbContext.ReqMains.Where(p => p.ReqNo == req.ReqNo).FirstOrDefault();
                if (main != null)
                {
                    return "<p class='alert alert-warning'>需求申请编号" + req.ReqNo + "已存在，无法修改！</p>";
                }
            }

            try
            {
                dbContext.Entry(req).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // GET:/ReqManage/EditDetail
        // 编辑需求主体部分
        [HttpGet]
        public ActionResult EditDetail(int id)
        {
            var req = dbContext.ReqDetails.Find(id);
            if (req == null)
            {
                return View();
            }

            // 下拉框预处理

            // 需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(typeof(ReqStatEnums), false, true, req.ReqStat.ToString());

            // 下发通知编号下拉列表
            var ls = dbContext.ReqReleases.ToList();
            ls.Insert(0, new ReqRelease { ReqReleaseID = 0, ReleaseNo = "无" }); //在最开始插入『无』选项
            ViewBag.ReqReleaseList = new SelectList(ls.Where(p => p.IsSideRelease != true), "ReqReleaseID", "ReleaseNo", req.ReqReleaseID);
            ViewBag.SecondReqReleaseList = new SelectList(ls.Where(p => p.IsSideRelease == true || p.ReqReleaseID == 0), "ReqReleaseID", "ReleaseNo", req.SecondReqReleaseID);

            req.OldReqDetailNo = req.ReqDetailNo;
            return View(req);
        }

        [HttpPost]
        public string EditDetail(ReqDetail req)
        {
            try
            {

                // 判断有无重复需求编号
                if (!string.IsNullOrEmpty(req.ReqDetailNo) && req.ReqDetailNo != req.OldReqDetailNo)
                {
                    var r = dbContext.ReqDetails.Where(a => a.ReqDetailNo == req.ReqDetailNo).FirstOrDefault();
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
                catch (Exception e1)
                {
                    throw new Exception("维护需求编号" + req.ReqDetailNo + "格式错误！（错误信息：" + e1.Message + "）");
                }

                // 更新时间
                req.UpdateTime = DateTime.Now;

                dbContext.Entry(req).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // GET:/ReqManage/CreateDetail
        // 在现有需求申请中新增一条需求
        [HttpGet]
        public ActionResult CreateDetail(int id)
        {
            var req = new ReqDetail { ReqMainID = id };

            // 下拉框预处理

            // 需求状态下拉列表
            ViewBag.ReqStatList = MyTools.GetSelectListByEnum(typeof(ReqStatEnums), false, true, req.ReqStat.ToString());

            // 下发通知编号下拉列表
            var ls = dbContext.ReqReleases.ToList();
            ls.Insert(0, new ReqRelease { ReqReleaseID = 0, ReleaseNo = "无" }); //在最开始插入『无』选项
            ViewBag.ReqReleaseList = new SelectList(ls.Where(p => p.IsSideRelease != true), "ReqReleaseID", "ReleaseNo", 0);
            ViewBag.SecondReqReleaseList = new SelectList(ls.Where(p => p.IsSideRelease == true || p.ReqReleaseID == 0), "ReqReleaseID", "ReleaseNo", 0);

            ViewBag.ReqNo = dbContext.ReqMains.Find(id).ReqNo;

            return View(req);
        }

        [HttpPost]
        public string CreateDetail(ReqDetail req)
        {
            try
            {

                // 判断有无重复需求编号
                var r = dbContext.ReqDetails.Where(a => a.ReqDetailNo == req.ReqDetailNo).FirstOrDefault();
                if (r != null)
                {
                    throw new Exception("维护需求编号" + r.ReqDetailNo + "已存在，不允许新增！");
                }

                // 根据需求编号确定需求类型
                req.ReqType = 0;
                try
                {
                    req.ReqType = int.Parse(req.ReqDetailNo.Split('-')[2]);
                }
                catch (Exception e1)
                {
                    throw new Exception("维护需求编号" + req.ReqDetailNo + "格式错误！（错误信息：" + e1.Message + "）");
                }

                // 更新时间
                req.CreateTime = DateTime.Now;
                req.UpdateTime = DateTime.Now;

                dbContext.ReqDetails.Add(req);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 删除
        // type: 1-DelMain 2-DelDetail
        [HttpPost]
        public string Delete(int id, int type)
        {

            try
            {
                if (type == 1)
                {
                    var main = dbContext.ReqMains.Find(id);
                    dbContext.Entry(main).State = System.Data.Entity.EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                else
                {
                    var detail = dbContext.ReqDetails.Find(id);
                    dbContext.Entry(detail).State = System.Data.Entity.EntityState.Deleted;
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

            return "删除成功";
        }



        #endregion

        #region 出池下发计划
        // 出池计划查询与导出
        [HttpGet]
        public ActionResult OutPoolTable(OutPoolTableQuery query, bool isQuery = false, int pageNum = 1, bool isExcel = false)
        {
            string errMsg = "";

            if (isQuery)
            {
                // 根据query条件查询结果
                var ls = from a in dbContext.ReqDetails
                         select a;
                if (query.SysID != 0)
                {
                    ls = ls.Where(p => p.ReqMain.SysID == query.SysID);
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
                    ls = ls.Where(p => p.ReqMain.AcptDate.Value.Year.ToString() == query.MaintainYear);
                }

                // 将查询结果转换为OutPoolTableResult和OutPoolTableResultExcel（避免多出来的short字段影响）
                List<OutPoolTableResult> resultList = new List<OutPoolTableResult>();
                List<OutPoolTableResultExcel> resultExcelList = new List<OutPoolTableResultExcel>();
                foreach (var req in ls.ToList())
                {
                    try
                    {
                        var rls = dbContext.ReqReleases.Find(req.ReqReleaseID);

                        OutPoolTableResult res = new OutPoolTableResult()
                        {
                            AcptMonth = req.ReqMain.AcptDate == null ? "" : req.ReqMain.AcptDate.Value.ToString("yyyy/M"),
                            SysName = req.ReqMain.SysName,
                            Version = req.Version,
                            ReqNo = req.ReqMain.ReqNo,
                            ReqDetailNo = req.ReqDetailNo,
                            ReqReason = req.ReqMain.ReqReason,
                            ReqDesc = req.ReqDesc,
                            DevWorkload = req.DevWorkload,
                            ReqDevPerson = req.ReqMain.ReqDevPerson,
                            ReqBusiTestPerson = req.ReqMain.ReqBusiTestPerson,
                            ReqType = req.ReqTypeName,
                            PlanReleaseDate = rls == null ? "" : rls.PlanReleaseDate.ToShortDateString(),
                            ReleaseDate = rls == null ? "" : rls.ReleaseDate.Value.ToShortDateString(),
                            Remark = req.Remark
                        };
                        resultList.Add(res);

                        OutPoolTableResultExcel resExcel = new OutPoolTableResultExcel
                        {
                            AcptMonth = req.ReqMain.AcptDate == null ? "" : req.ReqMain.AcptDate.Value.ToString("yyyy/M"),
                            SysName = req.ReqMain.SysName,
                            Version = req.Version,
                            ReqNo = req.ReqMain.ReqNo,
                            ReqDetailNo = req.ReqDetailNo,
                            ReqReason = req.ReqMain.ReqReason,
                            ReqDesc = req.ReqDesc,
                            DevWorkload = req.DevWorkload,
                            ReqDevPerson = req.ReqMain.ReqDevPerson,
                            ReqBusiTestPerson = req.ReqMain.ReqBusiTestPerson,
                            ReqType = req.ReqTypeName,
                            PlanReleaseDate = rls == null ? "" : rls.PlanReleaseDate.ToShortDateString(),
                            ReleaseDate = rls == null ? "" : rls.ReleaseDate.Value.ToShortDateString(),
                            Remark = req.Remark
                        };

                        resultExcelList.Add(resExcel);
                        
                    }


                    catch (Exception e1)
                    {
                        errMsg += "出错了:" + e1.Message + "; ReqDetailID=" + req.ReqDetailID;
                    }
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
                    return this.MakeExcel<OutPoolTableResultExcel>("OutPoolReportT", targetFileName, resultExcelList);
                }
                else
                {
                    // 分页
                    query.ResultList = resultList.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE);
                }
            }
            else
            {
                query = new OutPoolTableQuery();
            }

            // 系统列表下拉
            List<RetailSystem> ls1 = this.GetNormalSysList();
            // 加上“全部”
            ls1.Insert(0, new RetailSystem() { SysID = 0, SysName = "全部" });
            SelectList sl1 = new SelectList(ls1, "SysID", "SysName", query.SysID);
            ViewBag.SysList = sl1;

            ViewBag.ErrMsg = errMsg;

            return View(query);
        }
        #endregion

        /*
         * 【3】批处理


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
        public string BatProc(string reqs, string version, string outDate, string planReleaseDate, string ReleaseNo, string SideReleaseNo, string ReleaseDate, string secondReleaseDate,
            string remark, string reqStat, string acptDate, bool clearAcptDate)
        {
            // 若填写了下发日期，则下发状态应该为「办结」
            if ((!string.IsNullOrEmpty(ReleaseDate) || !string.IsNullOrEmpty(secondReleaseDate)) && reqStat != ReqStatEnums.办结.ToString())
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

            if (!string.IsNullOrEmpty(planReleaseDate))
            {
                sb.Append(", PlanReleaseDate='" + planReleaseDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(ReleaseNo))
            {
                sb.Append(", ReleaseNo='" + ReleaseNo + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(SideReleaseNo))
            {
                sb.Append(", SideReleaseNo='" + SideReleaseNo + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(ReleaseDate))
            {
                sb.Append(", ReleaseDate='" + ReleaseDate + "'");
                updateFiledNum++;
            }

            if (!string.IsNullOrEmpty(secondReleaseDate))
            {
                sb.Append(", SecondReleaseDate='" + secondReleaseDate + "'");
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

    */

        #region Helper
        /// <summary>
        /// 生成用于Excel输出的list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<ReqExcel> GetExcelList(List<FullReq> list)
        {
            List<ReqExcel> rl = new List<ReqExcel>();
            foreach (var s in list)
            {
                ReqExcel reqExcel = new ReqExcel
                {
                    SysName = s.reqMain.SysName,
                    AcptDate = s.reqMain.AcptDate,
                    ReqNo = s.reqMain.ReqNo,
                    ReqReason = s.reqMain.ReqReason,
                    ReqFromDept = s.reqMain.ReqFromDept,
                    ReqFromPerson = s.reqMain.ReqFromPerson,
                    ReqAcptPerson = s.reqMain.ReqAcptPersonNamePhone,
                    ReqDevPerson = s.reqMain.ReqDevPerson,
                    ReqBusiTestPerson = s.reqMain.ReqBusiTestPerson,
                    DevAcptDate = s.reqMain.DevAcptDate,
                    DevEvalDate = s.reqMain.DevEvalDate,
                    ReqDetailNo = s.reqDetail.ReqDetailNo,
                    Version = s.reqDetail.Version,
                    ReqDesc = s.reqDetail.ReqDesc,
                    ReqType = s.reqDetail.ReqTypeName,
                    DevWorkload = s.reqDetail.DevWorkload,
                    ReqStat = s.reqDetail.ReqStatName,
                    OutDate = s.reqDetail.OutDate,
                    PlanReleaseDate = s.reqRelease == null ? "" : s.reqRelease.PlanReleaseDate.ToShortDateString(),
                    ReleaseDate = s.reqRelease == null ? null : s.reqRelease.ReleaseDate,
                    ReleaseNo = s.reqDetail.ReqReleaseNo,
                    IsSysAsso = s.reqDetail.IsSysAsso ? "是" : "",
                    AssoSysName = s.reqDetail.AssoSysName,
                    AssoReqNo = s.reqDetail.AssoReqNo,
                    AssoReleaseDesc = s.reqDetail.AssoReleaseDesc,
                    Remark = s.reqDetail.Remark
                };
                rl.Add(reqExcel);
            }
            return rl;
        }
        #endregion

    }

}
