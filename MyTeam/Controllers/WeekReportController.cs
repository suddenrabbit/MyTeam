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
            // 按照『进度』升序、『计划完成日期』降序
            ls = ls.OrderBy(a => a.Progress).OrderByDescending(a => a.PlanDeadLine);
            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 填报:重点工作
        public ActionResult AddMain()
        {
            // 工作类型下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkTypeList);
            ViewBag.WorkTypeList = sl;


            // 默认加上当前的用户UID和姓名
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                user = Constants.UserList.FirstOrDefault<User>(); // 若获取不到则去第一个
            }

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
            SelectList sl = MyTools.GetSelectList(Constants.WorkTypeList, false, true, main.WorkType);
            ViewBag.WorkTypeList = sl;

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
            // 当前用户
            User user = this.GetSessionCurrentUser();
           
            // RptDate备选（取最近的5个）            
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl = MyTools.GetSelectList(ls);

            ViewBag.RptDateList = sl;


            // 工作类型下拉列表
            SelectList sl2 = MyTools.GetSelectList(Constants.WorkTypeList);
            ViewBag.WorkTypeList = sl2;

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
                    Person = user.Realname,
                    RptPersonID = user.UID,
                    Progress = 100,
                    IsWithMain = false
                };
            }


            // 重点项目下拉
            var mainList = dbContext.WeekReportMains.Where(a => a.DoNotTrack != true);
            SelectList sl3 = new SelectList(mainList, "WRMainID", "WorkName");
            ViewBag.WorkNameList = sl3;

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
            SelectList sl = MyTools.GetSelectList(Constants.WorkTypeList, false, true, detail.WorkType);
            ViewBag.WorkTypeList = sl;

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl2 = MyTools.GetSelectList(ls, false, true, detail.RptDate);
            ViewBag.RptDateList = sl2;

            // 重点项目下拉
            var mainList = dbContext.WeekReportMains.Where(a => a.DoNotTrack != true);
            SelectList sl3 = new SelectList(mainList, "WRMainID", "WorkName");
            ViewBag.WorkNameList = sl3;

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
            // 默认加上当前的用户UID和姓名
            User user = this.GetSessionCurrentUser();
           
            // RptDate备选（取最近的5个）
            List<string> ls = this.GetRptDateList();
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
            List<string> ls = this.GetRptDateList();
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

        /*导出周报（因周报格式较为特殊，不使用通用方法导出）*/
        // 页面
        public ActionResult Export()
        {
            // 获取周报日期列表：取最近的5个
            SelectList sl = MyTools.GetSelectList(this.GetRptDateList());
            ViewBag.RptDateList = sl;

            return View();
        }

        [HttpPost]
        public ActionResult Export(string nextWeek, string thisWeek)
        {
            // 读取模板
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/WeekReportT.xlsx");
            // POIFSFileSystem fs = new POIFSFileSystem(new FileStream(tmpFilePath, FileMode.OpenOrCreate));
            using (ExcelPackage ep = new ExcelPackage(new FileInfo(tmpFilePath)))
            {
                ExcelWorkbook wb = ep.Workbook;

                procWorkbook(wb, thisWeek, nextWeek, 1);

                // 下载
                // 文件名中文处理
                string targetFileName = HttpUtility.UrlEncode("零售团队工作周报" + nextWeek.Substring(0, 4) + nextWeek.Substring(nextWeek.Length - 4));

                return File(ep.GetAsByteArray(), "application/excel", targetFileName + ".xlsx");
            }
        }

        // 生成ExcelWorkBook
        private void procWorkbook(ExcelWorkbook wb, string thisWeek, string nextWeek, int sheetNum)
        {
            // 根据sheetNum处理
            ExcelWorksheet sheet = wb.Worksheets[sheetNum];
            sheet.Name = thisWeek; //sheet名称设为报表日期

            // 填报周期
            sheet.Cells[1, 4].Value = "填报周期：" + thisWeek + " - " + nextWeek;

            // 分别读取年度重点任务、每周工作、重点工作、项目风险

            // 游标：标记目前需要操作的行号
            int cursor = 4; //从第4行开始操作

            // 【1】重点任务 
            var yearMissionList = dbContext.YearMissions.Where(p => p.DoNotTrack != true).ToList();

            int size = yearMissionList.Count();
            int num = 1;
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            foreach (var s in yearMissionList)
            {
                // 第一列是序号
                sheet.Cells[cursor, 1].Value = num;
                sheet.Cells[cursor, 2].Value = s.MissionDate;
                sheet.Cells[cursor, 3].Value = s.MissionSource;
                sheet.Cells[cursor, 4].Value = s.WorkMission;
                sheet.Cells[cursor, 5].Value = s.WorkStage;
                sheet.Cells[cursor, 6].Value = s.Person;
                sheet.Cells[cursor, 7].Value = s.OutSource;
                sheet.Cells[cursor, 8].Value = s.Progress;
                sheet.Cells[cursor, 9].Value = s.PlanDeadLine;
                sheet.Cells[cursor, 10].Value = s.Remark;

                // 下移一行
                cursor++;

                num++;
            }

            // 游标下移3行，因为有1个空行2个标题行
            cursor += 3;

            // 【2】重点工作（取所有不为100%的、以及WorkYear是本年的）
            var mainList = (from a in dbContext.WeekReportMains
                            where a.DoNotTrack != true
                            orderby a.Person, a.OutSource
                            select a).ToList();

            size = mainList.Count();
            num = 1;
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            foreach (var s in mainList)
            {
                // 第一列是序号
                sheet.Cells[cursor, 1].Value = num;
                sheet.Cells[cursor, 2].Value = s.WorkType;
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

            // 【3】风险
            // 风险提示取本周和下周的
            var riskList = dbContext.WeekReportRisks.Where(a => a.RptDate == thisWeek || a.RptDate == nextWeek);
            size = riskList.Count();
            if (size > 0)
            {
                // 在cursor+1位置插入size-1行
                sheet.InsertRow(cursor + 1, size - 1, cursor);

                // 插入数据
                num = 1; //序号
                foreach (var s in riskList)
                {
                    // 需要对第2-4单元格进行合并
                    //sheet.Cells[cursor, 2, cursor, 4].Merge = true;
                    // 5-10单元格合并
                    //sheet.Cells[cursor, 5, cursor, 10].Merge = true;   //似乎有问题，以后处理

                    // 第一列是序号
                    sheet.Cells[cursor, 1].Value = num;
                    sheet.Cells[cursor, 2].Value = s.RiskDetail;
                    sheet.Cells[cursor, 5].Value = s.Solution;

                    // 下移一行
                    cursor++;
                    // 序号+1
                    num++;
                }

                // 游标下移3行，因为有3个标题行
                cursor += 3;
            }
            else
            {
                // 游标下移4行，因为有一个空行
                cursor += 4;
            }

            // 【4】本周工作
            var thisWeekDetailList = (from a in dbContext.WeekReportDetails
                                      where a.RptDate == thisWeek
                                      orderby a.Person, a.OutSource
                                      select a).ToList();

            size = thisWeekDetailList.Count();
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            foreach (var s in thisWeekDetailList)
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

                sheet.Cells[cursor, 1].Value = s.Priority;
                sheet.Cells[cursor, 2].Value = s.WorkType;
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

            // 游标下移3行，因为有3个标题行
            cursor += 3;

            // 【4】下周计划
            var nextWeekDetailList = (from a in dbContext.WeekReportDetails
                                      where a.RptDate == nextWeek
                                      orderby a.Person, a.OutSource
                                      select a).ToList();

            size = nextWeekDetailList.Count();
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            foreach (var s in nextWeekDetailList)
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

                sheet.Cells[cursor, 1].Value = s.Priority;
                sheet.Cells[cursor, 2].Value = s.WorkType;
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

    }
}
