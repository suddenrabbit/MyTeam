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
#if Release
    [Authorize]
#endif
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
                ls = ls.Where(a => a.Person.Contains(user.Realname));
            }
            // 按照『进度』升序
            ls = ls.OrderBy(a => a.Progress);
            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 填报:重点工作
        public ActionResult AddMain()
        {
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStageList);
            ViewBag.WorkStageList = sl;

            // 默认加上当前的用户UID和姓名
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddMain" });
            }

            WeekReportMain main = new WeekReportMain() { WorkTime = 0, RptPersonID = user.UID, PlanDeadLine = null, Person = user.Realname };
            return View(main);
        }

        [HttpPost]
        public ActionResult AddMain(WeekReportMain main)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.WeekReportMains.Add(main);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e1)
            {
                // 任务阶段下拉列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
                ViewBag.WorkStageList = sl;
                ModelState.AddModelError("", "出错了：" + e1.Message);
                return View(main);
            }
            return RedirectToAction("MainIndex");
        }

        // 修改：每周重点工作
        public ActionResult EditMain(int id)
        {
            WeekReportMain main = dbContext.WeekReportMains.ToList().Find(a => a.WRMainID == id);
            if (main == null)
            {
                ModelState.AddModelError("", "无此记录");
                main = new WeekReportMain();
            }
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
            ViewBag.WorkStageList = sl;
            return View(main);
        }

        [HttpPost]
        public ActionResult EditMain(WeekReportMain main)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(main).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    return RedirectToAction("MainIndex");
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 用户列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStageList, false, true, main.WorkStage);
                ViewBag.WorkStageList = sl;

            }
            return View(main);
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
                dbContext.Database.ExecuteSqlCommand("delete from WeekReportDetails where WorkMission = @p0 and IsWithMain = 1", id.ToString());

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }


        /* 每周工作 */

        // 每周工作页面
        public ActionResult DetailIndex(string id, bool forMain = false, int pageNum = 1)
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
                ls = ls.Where(a => a.Person.Contains(user.Realname));
            }
            // 根据forMain判断是否与重点任务有关
            if (forMain)
            {
                int mainId = 0;
                if (!int.TryParse(id, out mainId))
                {
                    ViewBag.ErrMsg = "参数不正确！";
                    return View();
                }
                WeekReportMain main = dbContext.WeekReportMains.ToList().Find(a => a.WRMainID == mainId);
                if (main == null)
                {
                    ViewBag.ErrMsg = "参数不正确！";
                    return View();
                }
                string workName = main.WorkName;
                // 此处只显示与“重点任务”有关的
                ls = ls.Where(a => a.IsWithMain == true && a.WorkMission == id.ToString());
                // 按照RptDate倒序显示
                ls = ls.OrderByDescending(a => a.RptDate);

                // workMission中的ID转为中文
                foreach (var r in ls)
                {
                    r.WorkMission = workName;
                }

                ViewBag.Title = workName + "-每周工作";
            }
            else
            {
                // 此处只显示与“重点任务”无关的
                ls = ls.Where(a => a.IsWithMain != true);
            }

            // 按照RptDate倒序显示
            ls = ls.OrderByDescending(a => a.RptDate);

            ViewBag.ForMain = forMain;

            ViewBag.MainId = id;

            return View(ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE));
        }

        // 添加每周工作
        public ActionResult AddDetail(string id, bool forMain = false)
        {
            // 当前用户
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddDetail" });
            }

            // RptDate备选（取最近的5个）
            var r =  from a in dbContext.WeekReportDetails
                     orderby a.RptDate descending
                     select a.RptDate;
            List<string> ls = r.Take(5).Distinct().ToList<string>();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl = MyTools.GetSelectList(ls);
            
            ViewBag.RptDateList = sl;

            // 工作任务：默认为ID，不允许填写            

            // 完成情况的下拉列表
            ViewBag.WorkStatList = MyTools.GetSelectList(Constants.WorkStatList);

            WeekReportDetail detail = new WeekReportDetail()
            {
                RptDate = DateTime.Now.Year + "年",
                Person = user.Realname,
                RptPersonID = user.UID,
                WorkMission = forMain ? id : "领导交办",
                IsWithMain = forMain
            };

            ViewBag.ForMain = forMain;

            ViewBag.MainId = id;

            return View(detail);
        }

        [HttpPost]
        public ActionResult AddDetail(WeekReportDetail detail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.WeekReportDetails.Add(detail);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了：" + e1.Message);
                // 完成情况的下拉列表
                ViewBag.WorkStatList = MyTools.GetSelectList(Constants.WorkStatList);
                return View(detail);
            }
            if (detail.IsWithMain)
            {
                // 自动计算工时
                if (detail.IsWithMain)
                {
                    this.UpdateWorkTime(detail.WorkMission);
                }
                return RedirectToAction("DetailIndex", new { id = detail.WorkMission, forMain = detail.IsWithMain });
            }
            return RedirectToAction("DetailIndex");
        }

        // 修改：每周重点工作
        public ActionResult EditDetail(int id)
        {
            WeekReportDetail detail = dbContext.WeekReportDetails.ToList().Find(a => a.WRDetailID == id);
            if (detail == null)
            {
                ViewBag.ErrMsg = "无此记录！";
                return View();
            }
            // 任务阶段下拉列表
            SelectList sl = MyTools.GetSelectList(Constants.WorkStatList, false, true, detail.WorkStat);
            ViewBag.WorkStatList = sl;
            
            // RptDate备选（取最近的5个）
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl2 = MyTools.GetSelectList(ls,false, true, detail.RptDate);
            ViewBag.RptDateList = sl2;

            return View(detail);
        }

        [HttpPost]
        public ActionResult EditDetail(WeekReportDetail detail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(detail).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    if (detail.IsWithMain)
                    {
                        // 自动计算工时
                        if (detail.IsWithMain)
                        {
                            this.UpdateWorkTime(detail.WorkMission);
                        }
                        return RedirectToAction("DetailIndex", new { id = detail.WorkMission, forMain = detail.IsWithMain });
                    }
                    return RedirectToAction("DetailIndex");
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 用户列表
                SelectList sl = MyTools.GetSelectList(Constants.WorkStatList, false, true, detail.WorkStat);
                ViewBag.WorkStatList = sl;
                // RptDate备选（取最近的5个）
                List<string> ls = this.GetRptDateList();
                ls.Insert(0, DateTime.Now.Year + "年");
                SelectList sl2 = MyTools.GetSelectList(ls, false, true, detail.RptDate);
                ViewBag.RptDateList = sl2;
            }
            return View(detail);
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
                if(detail.IsWithMain)
                {
                    this.UpdateWorkTime(detail.WorkMission);
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
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/WeekReport/AddRisk" });
            }

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl2 = MyTools.GetSelectList(ls);
            ViewBag.RptDateList = sl2;

            WeekReportRisk risk = new WeekReportRisk() { RptDate = DateTime.Now.Year + "年", RptPersonID = user.UID };
            return View(risk);
        }

        [HttpPost]
        public ActionResult AddRisk(WeekReportRisk risk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.WeekReportRisks.Add(risk);
                    dbContext.SaveChanges();
                    return RedirectToAction("RiskIndex");

                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了：" + e1.Message);

            }
            return View(risk);
        }

        // 修改：风险与待协调问题
        public ActionResult EditRisk(int id)
        {
            WeekReportRisk risk = dbContext.WeekReportRisks.ToList().Find(a => a.WRRiskID == id);
            if (risk == null)
            {
                ModelState.AddModelError("", "未找到该记录");
                risk = new WeekReportRisk();
            }

            // RptDate备选（取最近的5个）
            List<string> ls = this.GetRptDateList();
            ls.Insert(0, DateTime.Now.Year + "年");
            SelectList sl = MyTools.GetSelectList(ls, false, true, risk.RptDate);
            ViewBag.RptDateList = sl;

            return View(risk);
        }

        [HttpPost]
        public ActionResult EditRisk(WeekReportRisk risk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(risk).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();

                    return RedirectToAction("RiskIndex");
                }
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);

            }
            return View(risk);
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
            List<WeekReportDetail> details = dbContext.WeekReportDetails.ToList();
            WeekReportDetail temp = details.Find(a => a.WorkMission == mainId);
            if(temp == null){
                dbContext.Database.ExecuteSqlCommand("update WeekReportMains set WorkTime = 0");
            }
            else
            {
                dbContext.Database.ExecuteSqlCommand("update WeekReportMains set WorkTime = (select sum(d.WorkTime) from WeekReportDetails d left join WeekReportMains m on d.WorkMission = m.WRMainID) where WRMainID=" + mainId);
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
        public ActionResult Export(string thisWeek, string lastWeek)
        {
            // 读取模板
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/WeekReportT.xlsx");
            // POIFSFileSystem fs = new POIFSFileSystem(new FileStream(tmpFilePath, FileMode.OpenOrCreate));
            using (ExcelPackage ep = new ExcelPackage(new FileInfo(tmpFilePath)))
            {
                ExcelWorkbook wb = ep.Workbook;
                // 第一个sheet生成本周内容
                procWorkbook(wb, thisWeek, 1);
                // 第二个sheet生成上周内容
                if (!string.IsNullOrEmpty(lastWeek))
                {
                    procWorkbook(wb, lastWeek, 2);
                }

                // 下载
                // 文件名中文处理
                string targetFileName = HttpUtility.UrlEncode("零售团队工作周报" + thisWeek.Substring(0, 4) + thisWeek.Substring(thisWeek.Length - 4));

                return File(ep.GetAsByteArray(), "application/excel", targetFileName + ".xlsx");
            }
        }

        // 生成ExcelWorkBook
        private void procWorkbook(ExcelWorkbook wb, string week, int sheetNum)
        {
            // 分别读取每周工作、重点工作、项目风险三部分内容
            var detailList = dbContext.WeekReportDetails.Where(a => a.RptDate == week);
            // 重点工作通过每周工作读取
            string[] detailWithMainList = detailList.Where(a => a.IsWithMain == true).Select(a=>a.WorkMission).ToArray<string>();
            var mainList = from a in dbContext.WeekReportMains
                           where detailWithMainList.Contains(a.WRMainID.ToString())
                           select a;
            var riskList = dbContext.WeekReportRisks.Where(a => a.RptDate == week);
        

            // 根据sheetNum处理
            ExcelWorksheet sheet = wb.Worksheets[sheetNum];
            sheet.Name = week; //sheet名称设为报表日期

            // 游标：标记目前需要操作的行号
            int cursor = 3; //先从第三行开始操作

            // 1、每周工作
            int size = detailList.Count();
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            foreach (var s in detailList)
            {
                // 需要对第3、4个单元格进行合并
                sheet.Cells[cursor, 3, cursor, 4].Merge = true;
                // 9、10两个个单元格合并
                sheet.Cells[cursor, 9, cursor, 10].Merge = true;
                // 第一列是序号
                sheet.Cells[cursor, 1].Value = cursor - 2;
                sheet.Cells[cursor, 2].Value = s.IsWithMain ? mainList.Where(a => a.WRMainID.ToString() == s.WorkMission).FirstOrDefault().WorkName : s.WorkMission;
                sheet.Cells[cursor, 3].Value = s.WorkDetail;
                sheet.Cells[cursor, 5].Value = s.Person;
                sheet.Cells[cursor, 6].Value = s.WorkTarget;
                sheet.Cells[cursor, 7].Value = s.WorkStat;
                sheet.Cells[cursor, 8].Value = s.WorkTime;
                sheet.Cells[cursor, 9].Value = s.Remark;

                // 下移一行
                cursor++;
            }

            // 游标下移2行，因为有2个标题行
            cursor += 2;

            // 2、重点工作
            size = mainList.Count();
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);
            // 插入数据
            int num = 1; //序号
            foreach (var s in mainList)
            {
                // 第一列是序号
                sheet.Cells[cursor, 1].Value = num;
                sheet.Cells[cursor, 2].Value = s.WorkName;
                sheet.Cells[cursor, 3].Value = s.WorkStage;
                sheet.Cells[cursor, 4].Value = s.WorkMission;
                sheet.Cells[cursor, 5].Value = s.Person;
                sheet.Cells[cursor, 6].Value = s.WorkTarget;
                sheet.Cells[cursor, 7].Value = s.PlanDeadLine;
                sheet.Cells[cursor, 8].Value = s.WorkTime;
                sheet.Cells[cursor, 9].Value = s.Progress;
                sheet.Cells[cursor, 10].Value = s.Remark;

                // 下移一行
                cursor++;
                // 序号+1
                num++;
            }

            // 游标下移2行，因为有2个标题行
            cursor += 2;

            //3、项目风险
            size = riskList.Count();
            // 在cursor+1位置插入size-1行
            sheet.InsertRow(cursor + 1, size - 1, cursor);

            // 插入数据
            // 序号
            num = 1;
            foreach (var s in riskList)
            {
                // 需要对第2-4单元格进行合并
                sheet.Cells[cursor, 2, cursor, 4].Merge = true;
                // 5-10单元格合并
                sheet.Cells[cursor, 5, cursor, 10].Merge = true;
                // 第一列是序号
                sheet.Cells[cursor, 1].Value = num;
                sheet.Cells[cursor, 2].Value = s.RiskDetail;
                sheet.Cells[cursor, 5].Value = s.Solution;

                // 下移一行
                cursor++;
                // 序号+1
                num++;
            }

        }

        // 获取RptDate列表
        private List<string> GetRptDateList()
        {
            return dbContext.Database.SqlQuery<string>("select distinct top 5 RptDate from WeekReportDetails order by RptDate desc").ToList<string>();
        }
        
    }
}
