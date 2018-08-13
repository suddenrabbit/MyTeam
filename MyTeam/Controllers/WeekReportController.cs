using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Models;
using MyTeam.Utils;
using OfficeOpenXml;
using System.IO;
using PagedList;
using MyTeam.Enums;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 周报管理
    /// </summary>

    public class WeekReportController : BaseController
    {
        /* 重点工作 */

        // 重点工作页面
        public ActionResult MainIndex(int pageNum = 1)
        {
            var ls = from a in dbContext.WeekReportMains select a;
            // 若非管理员只显示负责人中含有自己姓名的记录
            if (!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if (user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/MainIndex" });
                }
                ls = ls.Where(a => a.Person.Contains(user.Realname) || a.OutSource.Contains(user.Realname));
            }
            // 按照『计划完成日期』降序
            ls = ls.OrderByDescending(a => a.PlanDeadLine);
            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 填报:重点工作
        public ActionResult AddMain()
        {
            User user = this.GetSessionCurrentUser();
            if (user == null || user.UserType == (int)UserTypeEnums.系统用户)
            {
                return Content("<p class='text-danger'>当前用户不允许填写周报。请重新以普通用户身份登录！</p>");
            }

            // 工作类型下拉列表            
            ViewBag.WorkTypeList = new SelectList(GetWorkTypeList(), "ParamValue", "ParamName");

            // 填报人下拉列表
            ViewBag.RptPersonIDList = new SelectList(GetStaffList(), "UID", "Realname");

            // 默认加上当前的用户UID和姓名          
            WeekReportMain main = new WeekReportMain() { WorkTime = 0, RptPersonID = user.UID, Person = user.Realname, WorkYear = DateTime.Now.Year.ToString() };
            return View(main);
        }

        [HttpPost]
        public string AddMain(WeekReportMain main)
        {
            try
            {
                dbContext.WeekReportMains.Add(main);
                dbContext.SaveChanges();
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 修改：每周重点工作
        public ActionResult EditMain(int id)
        {
            WeekReportMain main = dbContext.WeekReportMains.ToList().Find(a => a.WRMainID == id);
            if (main == null)
            {
                return View();
            }

            // 若workYear为空，自动填上今年的日期
            if (string.IsNullOrEmpty(main.WorkYear))
            {
                main.WorkYear = DateTime.Now.Year.ToString();
            }

            // 工作类型下拉列表
            ViewBag.WorkTypeList = new SelectList(GetWorkTypeList(), "ParamValue", "ParamName", main.WorkType);

            // 填报人下拉列表
            ViewBag.RptPersonIDList = new SelectList(GetStaffList(), "UID", "Realname", main.RptPersonID);

            return View(main);
        }

        [HttpPost]
        public string EditMain(WeekReportMain main)
        {
            try
            {
                dbContext.Entry(main).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /WeekReport/DeleteMain/5
        [HttpPost]
        public string DeleteMain(int id)
        {
            try
            {
                WeekReportMain main = dbContext.WeekReportMains.ToList().Find(a => a.WRMainID == id);
                dbContext.Entry(main).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                // 同时删除重点工作对应的每周工作
                dbContext.Database.ExecuteSqlCommand("delete from WeekReportDetails where WorkName = @p0 and IsWithMain = 1", id.ToString());

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }


        /* 每周工作 */

        // 每周工作页面
        public ActionResult DetailIndex(int pageNum = 1, int orderByType = 1)
        {
            var ls = from a in dbContext.WeekReportDetails select a;
            // 若非管理员只显示负责人中含有自己姓名的记录
            if (!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if (user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/DetailIndex" });
                }
                ls = ls.Where(a => a.Person.Contains(user.Realname) || a.OutSource.Contains(user.Realname));
            }
            // 对于IsWithMain的任务，WorkName转换成对应的重点任务名称
            var mainLs = dbContext.WeekReportMains.ToList();
            WeekReportMain main = null;
            foreach (var detail in ls)
            {
                if (detail.IsWithMain)
                {
                    main = mainLs.Find(a => a.WRMainID.ToString() == detail.WorkName);
                    detail.WorkName = main == null ? "未知" : main.WorkName;
                }
            }

            // 按照 orderByType 排序 (1-按照周报倒序；2-按照项目名称）
            if (orderByType == 2)
            {
                ls = ls.OrderBy(a => a.WorkName);
            }
            else
            {
                ls = ls.OrderByDescending(a => a.RptDate);
            }

            ViewBag.OrdeyByTypeParam = orderByType;

            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 添加每周工作
        public ActionResult AddDetail(int id = 0, bool isCopy = false)
        {
            User user = this.GetSessionCurrentUser();
            if (user == null || user.UserType == (int)UserTypeEnums.系统用户)
            {
                return Content("登录信息失效或您在使用系统用户，不允许填写周报。请重新以普通用户身份登录！");
            }

            // RptDate备选（取最近的5个）            
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl = MyTools.GetSelectList(ls);

            ViewBag.RptDateList = sl;


            // 工作类型下拉列表
            ViewBag.WorkTypeList = new SelectList(GetWorkTypeList(), "ParamValue", "ParamName");

            WeekReportDetail detail = null;

            // 若是复制则直接读取现有的
            if (isCopy)
            {
                detail = dbContext.WeekReportDetails.ToList().Find(a => a.WRDetailID == id);
            }
            else
            {
                detail = new WeekReportDetail()
                {
                    RptDate = DateTime.Now.Year + "年",
                    RptPersonID = user.UID,
                    Person = user.Realname,
                    Progress = 100,
                    IsWithMain = false
                };
            }

            // 重点项目下拉：如果是copy的，就显示所有的下拉列表；如果是完全新建的，则不显示「不导出周报」的部分 
            var mainList = dbContext.WeekReportMains.OrderByDescending(p => p.WRMainID).ToList();
            if (!isCopy)
            {
                mainList = mainList.Where(p => p.DoNotTrack != true).ToList();
            }

            SelectList sl3 = new SelectList(mainList, "WRMainID", "WorkName");
            ViewBag.WorkNameList = sl3;

            // 填报人下拉列表
            ViewBag.RptPersonIDList = new SelectList(GetStaffList(), "UID", "Realname");

            return View(detail);
        }

        [HttpPost]
        public string AddDetail(WeekReportDetail detail)
        {
            try
            {
                dbContext.WeekReportDetails.Add(detail);
                dbContext.SaveChanges();

                // 自动计算工时
                if (detail.IsWithMain)
                {
                    this.UpdateWorkTime(detail.WorkName);
                }

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 修改：每周工作
        public ActionResult EditDetail(int id)
        {
            WeekReportDetail detail = dbContext.WeekReportDetails.ToList().Find(a => a.WRDetailID == id);
            if (detail == null)
            {
                return View();
            }

            // 工作类型下拉列表
            ViewBag.WorkTypeList = new SelectList(GetWorkTypeList(), "ParamValue", "ParamName", detail.WorkType);

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl2 = MyTools.GetSelectList(ls, false, true, detail.RptDate);
            ViewBag.RptDateList = sl2;

            // 重点项目下拉
            var mainList = dbContext.WeekReportMains.OrderByDescending(p => p.WRMainID); // 2018年4月9日 调整：编辑的时候显示所有的
            SelectList sl3 = new SelectList(mainList, "WRMainID", "WorkName");
            ViewBag.WorkNameList = sl3;

            // 填报人下拉列表
            ViewBag.RptPersonIDList = new SelectList(GetStaffList(), "UID", "Realname", detail.RptPersonID);

            return View(detail);
        }

        [HttpPost]
        public string EditDetail(WeekReportDetail detail)
        {
            try
            {
                dbContext.Entry(detail).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                // 自动计算工时
                if (detail.IsWithMain)
                {
                    this.UpdateWorkTime(detail.WorkName);
                }
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
            return Constants.AJAX_EDIT_SUCCESS_RETURN;
        }

        // AJAX调用
        // POST: /WeekReport/DeleteDetail/5
        [HttpPost]
        public string DeleteDetail(int id)
        {
            try
            {
                WeekReportDetail detail = dbContext.WeekReportDetails.ToList().Find(a => a.WRDetailID == id);
                dbContext.Entry(detail).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                // 自动计算工时
                if (detail.IsWithMain)
                {
                    this.UpdateWorkTime(detail.WorkName);
                }

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /* 风险与待协调问题 */

        // 风险与待协调问题页面
        public ActionResult RiskIndex(int pageNum = 1)
        {
            var ls = from a in dbContext.WeekReportRisks select a;
            // 若非管理员只显示自己填报的记录
            if (!this.IsAdminNow())
            {
                User user = this.GetSessionCurrentUser();
                if (user == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/RiskIndex" });
                }
                ls = ls.Where(a => a.RptPersonID == user.UID);
            }
            // 按照RptDate倒序显示
            ls = ls.OrderByDescending(a => a.RptDate);
            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 填报:风险与待协调问题
        public ActionResult AddRisk()
        {
            User user = this.GetSessionCurrentUser();
            if (user == null || user.UserType == (int)UserTypeEnums.系统用户)
            {
                return Content("登录信息失效或您在使用系统用户，不允许填写周报。请重新以普通用户身份登录！");
            }

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetWorkReportRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl2 = MyTools.GetSelectList(ls);
            ViewBag.RptDateList = sl2;

            WeekReportRisk risk = new WeekReportRisk() { RptDate = DateTime.Now.Year + "年", RptPersonID = user.UID };
            return View(risk);
        }

        [HttpPost]
        public string AddRisk(WeekReportRisk risk)
        {
            try
            {
                dbContext.WeekReportRisks.Add(risk);
                dbContext.SaveChanges();
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 修改：风险与待协调问题
        public ActionResult EditRisk(int id)
        {
            WeekReportRisk risk = dbContext.WeekReportRisks.ToList().Find(a => a.WRRiskID == id);
            if (risk == null)
            {
                return View();
            }

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetWorkReportRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl = MyTools.GetSelectList(ls, false, true, risk.RptDate);
            ViewBag.RptDateList = sl;

            return View(risk);
        }

        [HttpPost]
        public string EditRisk(WeekReportRisk risk)
        {
            try
            {
                dbContext.Entry(risk).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /WeekReport/DeleteRisk/5
        [HttpPost]
        public string DeleteRisk(int id)
        {
            try
            {
                WeekReportRisk Risk = dbContext.WeekReportRisks.ToList().Find(a => a.WRRiskID == id);
                dbContext.Entry(Risk).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        // Main表的WorkTime根据Detail表计算，每次插入、编辑、删除Detail时均重新计算
        private void UpdateWorkTime(string mainId)
        {
            var details = dbContext.WeekReportDetails.Where(a => a.WorkMission == mainId).ToList();
            if (details.Count == 0)
            {
                dbContext.Database.ExecuteSqlCommand("update WeekReportMains set WorkTime = 0 where WRMainID = @p0", mainId);
            }
            else
            {
                dbContext.Database.ExecuteSqlCommand("update WeekReportMains set WorkTime = (select sum(d.WorkTime) from WeekReportDetails d left join WeekReportMains m on d.WorkName = m.WRMainID where d.WorkName = @p0) where WRMainID = @p1", mainId.ToString(), mainId);
            }
        }



        // 获取RptDate列表
        private List<string> GetRptDateList()
        {
            return dbContext.Database.SqlQuery<string>("select distinct top 5 RptDate from WeekReportDetails order by RptDate desc").ToList<string>();
        }


        /* 年度重点任务 */

        // 年度重点任务页面
        public ActionResult YearMissionIndex(int pageNum = 1)
        {
            var ls = from a in dbContext.YearMissions select a;

            // 按照『进度』升序、『计划完成日期』降序
            ls = ls.OrderBy(a => a.Progress).OrderByDescending(a => a.PlanDeadLine);
            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 填报:年度重点任务
        public ActionResult AddYearMission()
        {
            return View(new YearMission());
        }

        [HttpPost]
        public string AddYearMission(YearMission yearMission)
        {
            try
            {
                dbContext.YearMissions.Add(yearMission);
                dbContext.SaveChanges();
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 修改：年度重点任务
        public ActionResult EditYearMission(int id)
        {
            YearMission yearMission = dbContext.YearMissions.ToList().Find(a => a.YMID == id);
            if (yearMission == null)
            {
                return View();
            }

            return View(yearMission);
        }

        [HttpPost]
        public string EditYearMission(YearMission yearMission)
        {
            try
            {
                dbContext.Entry(yearMission).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /WeekReport/DeleteYearMission/5
        [HttpPost]
        public string DeleteYearMission(int id)
        {
            try
            {
                YearMission yearMission = dbContext.YearMissions.ToList().Find(a => a.YMID == id);
                dbContext.Entry(yearMission).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        // 2017.12.18 新增：按照年度和个人导出周报
        public ActionResult ExportByYear()
        {
            var ls = new List<string>();
            foreach (var u in GetStaffList())
            {
                ls.Add(u.Realname);
            }
            SelectList sl = MyTools.GetSelectList(ls);
            ViewBag.PersonList = sl;

            return View();
        }

        [HttpPost]
        public ActionResult ExportByYear(string year, string person)
        {
            try
            {
                // 读取模板
                string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/WeekReportByYearT.xlsx");
                // POIFSFileSystem fs = new POIFSFileSystem(new FileStream(tmpFilePath, FileMode.OpenOrCreate));
                using (ExcelPackage ep = new ExcelPackage(new FileInfo(tmpFilePath)))
                {
                    ExcelWorkbook wb = ep.Workbook;

                    ExcelWorksheet sheet = wb.Worksheets[1];
                    int cursor = 3; //从第3行开始操作
                                    // 重点工作

                    var mainList = (from a in dbContext.WeekReportMains
                                    where a.WorkYear == year
                                    && (a.Person.Contains(person) || a.OutSource.Contains(person))
                                    orderby a.PlanDeadLine
                                    select a).ToList();

                    var mainSize = mainList.Count;
                    var num = 1;
                    // 在cursor+1位置插入size-1行
                    sheet.InsertRow(cursor + 1, mainSize - 1, cursor);

                    // 插入数据
                    foreach (var s in mainList)
                    {
                        // 第一列是序号
                        sheet.Cells[cursor, 1].Value = num;
                        sheet.Cells[cursor, 2].Value = s.WorkTypeName;
                        sheet.Cells[cursor, 3].Value = s.WorkName;
                        sheet.Cells[cursor, 4].Value = s.WorkMission;
                        sheet.Cells[cursor, 5].Value = s.WorkStage;
                        sheet.Cells[cursor, 6].Value = s.Person;
                        sheet.Cells[cursor, 7].Value = s.OutSource;
                        sheet.Cells[cursor, 8].Value = s.Progress + "%";
                        sheet.Cells[cursor, 9].Value = s.PlanDeadLine == null ? "" : s.PlanDeadLine.Value.ToString("yyyy/M/d");
                        sheet.Cells[cursor, 10].Value = s.Remark;

                        // 下移一行
                        cursor++;

                        num++;
                    }

                    // 游标下移3行
                    cursor += 3;

                    // 每周工作
                    var weekDetailList = (from a in dbContext.WeekReportDetails
                                          where a.RptDate.StartsWith(year)
                                          && (a.Person.Contains(person) || a.OutSource.Contains(person))
                                          orderby a.RptDate ascending
                                          select a).ToList();

                    var detailSize = weekDetailList.Count;
                    // 在cursor+1位置插入size-1行
                    sheet.InsertRow(cursor + 1, detailSize - 1, cursor);

                    // 插入数据
                    foreach (var s in weekDetailList)
                    {
                        string workName = "";
                        if (s.IsWithMain)
                        {
                            WeekReportMain main = mainList.Find(a => a.WRMainID.ToString() == s.WorkName);
                            if (main != null)
                            {
                                workName = main.WorkName;
                            }
                        }
                        else
                        {
                            workName = s.WorkName;
                        }

                        sheet.Cells[cursor, 1].Value = s.RptDate;
                        sheet.Cells[cursor, 2].Value = s.WorkTypeName;
                        sheet.Cells[cursor, 3].Value = workName;
                        sheet.Cells[cursor, 4].Value = s.WorkMission;
                        sheet.Cells[cursor, 5].Value = s.WorkTarget;
                        sheet.Cells[cursor, 6].Value = s.Person;
                        sheet.Cells[cursor, 7].Value = s.OutSource;
                        sheet.Cells[cursor, 8].Value = s.Progress + "%";
                        sheet.Cells[cursor, 9].Value = s.PlanDeadLine == null ? "" : s.PlanDeadLine.Value.ToString("yyyy/M/d");
                        sheet.Cells[cursor, 10].Value = s.Remark;

                        // 下移一行
                        cursor++;
                    }

                    // 下载
                    // 文件名中文处理
                    string targetFileName = HttpUtility.UrlEncode("年度与个人工作周报_" + year + "_" + person);

                    return File(ep.GetAsByteArray(), "application/excel", targetFileName + ".xlsx");
                }
            }
            catch (Exception e1)
            {
                return Content("导出失败，错误信息：" + e1.Message);
            }

        }

        /******************************2018年8月7日新增：周报格式变更********************************/
        /// <summary>
        /// 新周报页面
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="showAll"></param>
        /// <returns></returns>
        public ActionResult WorkReportIndex(int pageNum = 1, bool showAll = false)
        {
            var ls = from a in dbContext.WorkReports select a;
            if(!showAll) // 若非showAll，则只显示本人的
            {
                var currentUser = GetSessionCurrentUser();
                if(currentUser == null)
                {
                    return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/WorkReportIndex" });
                }
                if(currentUser.UserType == (int)Enums.UserTypeEnums.外协)
                {
                    ls = ls.Where(a => a.OutSource.Contains(currentUser.Realname));
                }
                else
                {
                    ls = ls.Where(a => a.Person.Contains(currentUser.Realname));
                }
            }                     

            ls = ls.OrderByDescending(a => a.RptDate);

            ViewBag.showAll = showAll;

            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        /// <summary>
        /// 添加工作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isCopy"></param>
        /// <returns></returns>
        public ActionResult AddWorkReport(int id = 0, bool isCopy = false)
        {
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return Content("登录信息失效，请重新登录！");
            }

            // RptDate备选（取最近的5个）            
            List<string> ls = this.GetWorkReportRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl = MyTools.GetSelectList(ls);

            ViewBag.RptDateList = sl;


            // 工作类型下拉列表
            ViewBag.WorkTypeList = new SelectList(GetWorkTypeList(), "ParamName", "ParamName"); // 直接使用中文，不再进行码值转换

            WorkReport detail = null;

            // 若是复制则直接读取现有的
            if (isCopy)
            {
                detail = dbContext.WorkReports.ToList().Find(a => a.WorkReportID == id);
            }
            else
            {
                detail = new WorkReport()
                {
                    RptDate = DateTime.Now.Year + "年",
                    RptPersonID = user.UID,
                    Person = user.Realname,
                    Progress = "100%",
                    IsMain = false
                };
            }

            // 填报人下拉列表
            ViewBag.RptPersonIDList = new SelectList(GetStaffList(), "UID", "Realname");

            return View(detail);
        }

        [HttpPost]
        public string AddWorkReport(WorkReport detail)
        {
            try
            {
                dbContext.WorkReports.Add(detail);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        /// <summary>
        /// 修改工作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditWorkReport(int id)
        {
            WorkReport detail = dbContext.WorkReports.ToList().Find(a => a.WorkReportID == id);
            if (detail == null)
            {
                return View();
            }

            // 工作类型下拉列表
            ViewBag.WorkTypeList = new SelectList(GetWorkTypeList(), "ParamName", "ParamName", detail.WorkType);

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetWorkReportRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl2 = MyTools.GetSelectList(ls, false, true, detail.RptDate);
            ViewBag.RptDateList = sl2;

            // 填报人下拉列表
            ViewBag.RptPersonIDList = new SelectList(GetStaffList(), "UID", "Realname", detail.RptPersonID);

            return View(detail);
        }

        [HttpPost]
        public string EditWorkReport(WorkReport detail)
        {
            try
            {
                dbContext.Entry(detail).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
            return Constants.AJAX_EDIT_SUCCESS_RETURN;
        }

        /// <summary>
        /// 删除工作(AJAX接口)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string DeleteWorkReport(int id)
        {
            try
            {
                WorkReport detail = dbContext.WorkReports.ToList().Find(a => a.WorkReportID == id);
                dbContext.Entry(detail).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /// <summary>
        /// 新的获取周报日期列表接口
        /// </summary>
        /// <returns></returns>
        private List<string> GetWorkReportRptDateList()
        {
            return dbContext.Database.SqlQuery<string>("select distinct top 5 RptDate from WorkReports order by RptDate desc").ToList<string>();
        }

        /// <summary>
        /// 新的导出周报
        /// </summary>
        /// <returns></returns>
        public ActionResult Export()
        {
            // 获取周报日期列表：取最近的5个
            SelectList sl = MyTools.GetSelectList(this.GetWorkReportRptDateList());
            ViewBag.RptDateList = sl;

            return View();
        }

        [HttpPost]
        public ActionResult Export(string rptDate)
        {
            // 读取模板
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/WorkReportT.xlsx");
            using (ExcelPackage ep = new ExcelPackage(new FileInfo(tmpFilePath)))
            {
                ExcelWorkbook wb = ep.Workbook;

                procWorkbook(wb, rptDate, 1);

                // 下载
                // 文件名中文处理
                string rptDateName = rptDate.Substring(12);
                rptDateName = rptDateName.Replace("年", "").Replace("月", "").Replace("日", "");
                
                string targetFileName = HttpUtility.UrlEncode("零售团队工作周报_" + rptDateName);

                return File(ep.GetAsByteArray(), "application/excel", targetFileName + ".xlsx");
            }
        }

        /// <summary>
        /// 生成excel的workbook
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="rptDate"></param>
        /// <param name="sheetNum"></param>
        private void procWorkbook(ExcelWorkbook wb, string rptDate, int sheetNum)
        {
            // 根据sheetNum处理
            ExcelWorksheet sheet = wb.Worksheets[sheetNum];
            sheet.Name = rptDate; //sheet名称设为报表日期

            // 填报周期
            sheet.Cells[1, 4].Value = "填报周期：" + rptDate;

            // 分别读取年度每周工作、风险提示，并对每周工作按照是否重点工作进行区分

            // 游标：标记目前需要操作的行号
            int cursor = 4; //从第4行开始操作

            // 读取所有的本周工作
            var workListFull = dbContext.WorkReports.Where(p => p.RptDate == rptDate).ToList();

            // 【1】重点工作
            var mainList = from a in workListFull
                           where a.IsMain == true
                           orderby a.PlanDeadLine descending, a.RptPersonID ascending
                           select a;

            this.makeWorkReports(ref cursor, ref sheet, mainList.ToList());

            // 游标下移3行
            cursor += 3;

            // 【2】其他工作
            var otherList = from a in workListFull
                            where a.IsMain == false
                            orderby a.PlanDeadLine descending, a.RptPersonID ascending
                            select a;

            this.makeWorkReports(ref cursor, ref sheet, otherList.ToList());

            // 游标下移3行
            cursor += 3;

            // 【3】风险
            // 风险提示取本周和下周的
            var riskList = dbContext.WeekReportRisks.Where(a => a.RptDate == rptDate);
            var size = riskList.Count();
            if (size > 0)
            {
                // 在cursor+1位置插入size-1行
                sheet.InsertRow(cursor + 1, size - 1, cursor);

                // 插入数据
                var num = 1; //序号
                foreach (var s in riskList)
                {
                    // 合并单元格
                    sheet.Cells[cursor, 2, cursor, 3].Merge = true;
                    sheet.Cells[cursor, 4, cursor, 14].Merge = true;

                    // 第一列是序号
                    sheet.Cells[cursor, 1].Value = num;
                    sheet.Cells[cursor, 2].Value = s.RiskDetail;
                    sheet.Cells[cursor, 4].Value = s.Solution;                   

                    // 下移一行
                    cursor++;
                    // 序号+1
                    num++;
                }

                // 游标下移3行，因为有3个标题行
                cursor += 3;
            }

        }

        /// <summary>
        /// 每周工作的通用方法
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="sheet"></param>
        /// <param name="list"></param>
        private void makeWorkReports(ref int cursor, ref ExcelWorksheet sheet, List<WorkReport> list)
        {
            var size = list.Count();
            var num = 1;
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            foreach (var s in list)
            {
                // 第一列是序号
                sheet.Cells[cursor, 1].Value = num;
                sheet.Cells[cursor, 2].Value = s.WorkType;
                sheet.Cells[cursor, 3].Value = s.WorkMission;
                sheet.Cells[cursor, 4].Value = s.WorkDetail;
                sheet.Cells[cursor, 5].Value = s.Person;
                sheet.Cells[cursor, 6].Value = s.OutSource;
                sheet.Cells[cursor, 7].Value = s.WorkStage;
                sheet.Cells[cursor, 8].Value = s.Progress;
                sheet.Cells[cursor, 9].Value = s.WorkOfThisWeek;
                sheet.Cells[cursor, 10].Value = s.DeliveryOfThisWeek;
                sheet.Cells[cursor, 11].Value = s.WorkOfNextWeek;
                sheet.Cells[cursor, 12].Value = s.DeliveryOfNextWeek;
                sheet.Cells[cursor, 13].Value = s.PlanDeadLine == null ? "" : s.PlanDeadLine.Value.ToShortDateString();
                sheet.Cells[cursor, 14].Value = s.Remark;

                // 下移一行
                cursor++;

                num++;
            }
        }
    }
}
