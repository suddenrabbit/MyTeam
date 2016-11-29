using MyTeam.Models;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace MyTeam.Controllers
{
    /// <summary>
    /// 各种导入功能（主要供上线时初始化数据使用）
    /// </summary>
    public class ImportController : BaseController
    {
        //
        // GET: /Import/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, int type)
        {
            if (file == null)
            {
                ViewBag.Msg = "<p class='alert alert-danger'>未获取到文件</p>";
                return View();
            }

            // 判断文件夹是否存在，不存在则创建文件夹
            var dir = HttpContext.Server.MapPath("~/Upload/temp");
            if (Directory.Exists(dir) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(dir);
            }
            string filePath = Path.Combine(dir, Path.GetFileName(file.FileName)); 
            try
            {
                file.SaveAs(filePath);
                // 读取Excel文件
                FileInfo excelFile = new FileInfo(filePath);

                using (ExcelPackage ep = new ExcelPackage(excelFile))
                {
                    ExcelWorksheet worksheet = ep.Workbook.Worksheets[1];

                    string result = "";

                    // 根据type进行不同的处理
                    // 1 维护需求
                    // 2 项目跟踪
                    // 3 项目会议
                    switch (type)
                    {
                        case 1:
                            result = this.ReqExcel(worksheet, file.FileName);
                            break;
                        case 2:
                            result = this.ProjExcel(worksheet, file.FileName);
                            break;
                        case 3:
                            result = this.ProjMeetingExcel(worksheet, file.FileName);
                            break;
                        default:
                            result = "<p class='alert alert-danger'>未知的文件类型！type=" + type + "</p>";
                            break;
                    }

                    ViewBag.Msg = result;
                }
            }
            catch (Exception e1)
            {
                ViewBag.Msg = "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
            finally
            {
                // 完成后删除文件
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return View();
        }

        /// <summary>
        /// 维护需求导入
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        private string ReqExcel(ExcelWorksheet worksheet, string fileName)
        {
            int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
            int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

            var ls = dbContext.Reqs.ToList();

            for (int row = rowStart + 1; row <= rowEnd; row++)
            {
                // 第一列为空则结束
                if (worksheet.Cells[row, 1] == null) break;

                // 第一列，系统名称，转成SysID
                string sysName = worksheet.Cells[row, 1].GetValue<string>();
                var sys = this.GetSysList().Find(a => a.SysName == sysName);
                int sysID = sys == null ? 0 : sys.SysID;

                // 第11列，需求受理人，转成UID
                string reqPerson = worksheet.Cells[row, 11].GetValue<string>();
                var u = this.GetUserList().Find(a => reqPerson.Contains(a.Realname));
                int uid = u == null ? 0 : u.UID;

                // 第4列，维护需求编号
                string reqDetailNo = worksheet.Cells[row, 4].GetValue<string>();               

                Req r = new Req();
                // 按列赋值
                r.SysID = sysID;
                r.AcptDate = worksheet.Cells[row, 2].GetValue<DateTime>();
                r.ReqNo = worksheet.Cells[row, 3].GetValue<string>();
                r.ReqDetailNo = reqDetailNo;
                r.Version = worksheet.Cells[row, 5].GetValue<string>();
                r.BusiReqNo = worksheet.Cells[row, 6].GetValue<string>();
                r.ReqReason = worksheet.Cells[row, 7].GetValue<string>();
                r.ReqDesc = worksheet.Cells[row, 8].GetValue<string>();
                r.ReqFromDept = worksheet.Cells[row, 9].GetValue<string>();
                r.ReqFromPerson = worksheet.Cells[row, 10].GetValue<string>();
                r.ReqAcptPerson = uid;
                r.ReqDevPerson = worksheet.Cells[row, 12].GetValue<string>();
                r.ReqBusiTestPerson = worksheet.Cells[row, 13].GetValue<string>();
                r.ReqType = worksheet.Cells[row, 14].GetValue<string>();

                var workload = worksheet.Cells[row, 17].GetValue<string>();
                r.DevWorkload = string.IsNullOrEmpty(workload) ? 0 : int.Parse(workload);
                r.ReqStat = worksheet.Cells[row, 18].GetValue<string>();

                r.RlsNo = worksheet.Cells[row, 22].GetValue<string>();
                r.IsSysAsso = worksheet.Cells[row, 23].GetValue<string>() == "是";
                r.AssoSysName = worksheet.Cells[row, 24].GetValue<string>();
                r.AssoReqNo = worksheet.Cells[row, 25].GetValue<string>();
                r.AssoRlsDesc = worksheet.Cells[row, 26].GetValue<string>();
                r.Remark = worksheet.Cells[row, 27].GetValue<string>();

                // 对于可能为空的日期单独处理
                string devAcptDate = worksheet.Cells[row, 15].GetValue<string>();
                string devEvalDate = worksheet.Cells[row, 16].GetValue<string>();
                string outDate = worksheet.Cells[row, 19].GetValue<string>();
                string planRlsDate = worksheet.Cells[row, 20].GetValue<string>();
                string rlsDate = worksheet.Cells[row, 21].GetValue<string>();

                if (!string.IsNullOrEmpty(devAcptDate)) r.DevAcptDate = DateTime.Parse(devAcptDate);
                if (!string.IsNullOrEmpty(devEvalDate)) r.DevEvalDate = DateTime.Parse(devEvalDate);
                if (!string.IsNullOrEmpty(outDate)) r.OutDate = DateTime.Parse(outDate);
                if (!string.IsNullOrEmpty(planRlsDate)) r.PlanRlsDate = DateTime.Parse(planRlsDate);
                if (!string.IsNullOrEmpty(rlsDate)) r.RlsDate = DateTime.Parse(rlsDate);

                // ReqDetailNo 重复的跳过
                if(reqDetailNo.Length > 10 && ls.Find(a => a.ReqDetailNo == reqDetailNo) !=null)
                {  
                    continue;                    
                }
               
                else
                {
                   dbContext.Reqs.Add(r);
                }
            }

            // 保存
            int realNum = dbContext.SaveChanges();

            return string.Format("<p class='alert alert-success'>《{0}》处理成功！共{1}条数据，实际导入{2}条数据</p>", fileName, rowEnd - rowStart, realNum);

        }

        /// <summary>
        /// 项目导入
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string ProjExcel(ExcelWorksheet worksheet, string fileName)
        {
            int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
            int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

            var ls = this.GetProjList();

            for (int row = rowStart + 1; row <= rowEnd; row++)
            {
                // 第一列为空则结束
                if (worksheet.Cells[row, 1] == null) break;

                // 第1列，项目名称
                string projName = worksheet.Cells[row, 1].GetValue<string>();
                // 项目名称 重复的不导入
                if (ls.Find(a => a.ProjName == projName) != null)
                {
                    continue;
                }

                // 第5列，需求分析师，转成UID
                string reqPerson = worksheet.Cells[row, 5].GetValue<string>();
                var u = this.GetUserList().Find(a => a.Realname == reqPerson);
                int uid = u == null ? 0 : u.UID;

                Proj r = new Proj();
                // 按列赋值
                r.ProjName = projName;
                r.ProjNo = worksheet.Cells[row, 2].GetValue<string>();
                r.HostDept = worksheet.Cells[row, 3].GetValue<string>();
                r.ProjLevel = worksheet.Cells[row, 4].GetValue<string>();
                r.ReqAnalysisID = uid;
                r.BusiPerson = worksheet.Cells[row, 6].GetValue<string>();
                r.ProjManager = worksheet.Cells[row, 7].GetValue<string>();
                r.Architect = worksheet.Cells[row, 8].GetValue<string>();

                r.SurveyRemark = worksheet.Cells[row, 12].GetValue<string>();
                r.OutlineWriter = worksheet.Cells[row, 13].GetValue<string>();

                r.OutlineAuditPerson = worksheet.Cells[row, 16].GetValue<string>();

                r.OutlineRemark = worksheet.Cells[row, 18].GetValue<string>();
                r.ReqWriter = worksheet.Cells[row, 19].GetValue<string>();

                r.ReqRemark = worksheet.Cells[row, 24].GetValue<string>();

                r.RulesRemark = worksheet.Cells[row, 27].GetValue<string>();

                r.CheckResult = worksheet.Cells[row, 30].GetValue<string>();
                r.Remark = worksheet.Cells[row, 31].GetValue<string>();
                //r.IsReqTrack = false; // 默认赋值false  

                // 对于可能为空的日期单独处理
                string ProAcptDate = worksheet.Cells[row, 9].GetValue<string>();
                string SurveyGroupFoundDate = worksheet.Cells[row, 10].GetValue<string>();
                string SurveyFinishDate = worksheet.Cells[row, 11].GetValue<string>();

                string OutlineStartDate = worksheet.Cells[row, 14].GetValue<string>();
                string OutlineEndDate = worksheet.Cells[row, 15].GetValue<string>();

                string OutlinePublishDate = worksheet.Cells[row, 17].GetValue<string>();

                string ReqStartDate = worksheet.Cells[row, 20].GetValue<string>();
                string ReviewAcptDate = worksheet.Cells[row, 21].GetValue<string>();
                string ReviewMeetingDate = worksheet.Cells[row, 22].GetValue<string>();
                string ReqPublishDate = worksheet.Cells[row, 23].GetValue<string>();

                string RulesStartDate = worksheet.Cells[row, 25].GetValue<string>();
                string RulesPublishDate = worksheet.Cells[row, 26].GetValue<string>();

                string ProjCheckAcptDate = worksheet.Cells[row, 28].GetValue<string>();
                string ProjPublishDate = worksheet.Cells[row, 29].GetValue<string>();

                if (!string.IsNullOrEmpty(ProAcptDate)) r.ProAcptDate = DateTime.Parse(worksheet.Cells[row, 9].GetValue<string>());
                if (!string.IsNullOrEmpty(SurveyGroupFoundDate)) r.SurveyGroupFoundDate = DateTime.Parse(worksheet.Cells[row, 10].GetValue<string>());
                if (!string.IsNullOrEmpty(SurveyFinishDate)) r.SurveyFinishDate = DateTime.Parse(worksheet.Cells[row, 11].GetValue<string>());

                if (!string.IsNullOrEmpty(OutlineStartDate)) r.OutlineStartDate = DateTime.Parse(worksheet.Cells[row, 14].GetValue<string>());
                if (!string.IsNullOrEmpty(OutlineEndDate)) r.OutlineEndDate = DateTime.Parse(worksheet.Cells[row, 15].GetValue<string>());

                if (!string.IsNullOrEmpty(OutlinePublishDate)) r.OutlinePublishDate = DateTime.Parse(worksheet.Cells[row, 17].GetValue<string>());

                if (!string.IsNullOrEmpty(ReqStartDate)) r.ReqStartDate = DateTime.Parse(worksheet.Cells[row, 20].GetValue<string>());
                if (!string.IsNullOrEmpty(ReviewAcptDate)) r.ReviewAcptDate = DateTime.Parse(worksheet.Cells[row, 21].GetValue<string>());
                if (!string.IsNullOrEmpty(ReviewMeetingDate)) r.ReviewMeetingDate = DateTime.Parse(worksheet.Cells[row, 22].GetValue<string>());
                if (!string.IsNullOrEmpty(ReqPublishDate)) r.ReqPublishDate = DateTime.Parse(worksheet.Cells[row, 23].GetValue<string>());

                if (!string.IsNullOrEmpty(RulesStartDate)) r.RulesStartDate = DateTime.Parse(worksheet.Cells[row, 25].GetValue<string>());
                if (!string.IsNullOrEmpty(RulesPublishDate)) r.RulesPublishDate = DateTime.Parse(worksheet.Cells[row, 26].GetValue<string>());

                if (!string.IsNullOrEmpty(ProjCheckAcptDate)) r.ProjCheckAcptDate = DateTime.Parse(worksheet.Cells[row, 28].GetValue<string>());
                if (!string.IsNullOrEmpty(ProjPublishDate)) r.ProjPublishDate = DateTime.Parse(worksheet.Cells[row, 29].GetValue<string>());

                dbContext.Projs.Add(r);
            }

            // 保存
            int realNum = dbContext.SaveChanges();

            // 导入完了要更新内存
            this.Update(3);

            return string.Format("<p class='alert alert-success'>《{0}》处理成功！共{1}条数据，实际导入{2}条数据</p>", fileName, rowEnd - rowStart, realNum);

        }

        /// <summary>
        /// 需求会议导入
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string ProjMeetingExcel(ExcelWorksheet worksheet, string fileName)
        {
            int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
            int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

            var ls = this.GetProjList();

            for (int row = rowStart + 1; row <= rowEnd; row++)
            {
                // 第一列为空则结束
                if (worksheet.Cells[row, 1] == null) break;

                // 第1列，项目名称
                string projName = worksheet.Cells[row, 1].GetValue<string>();
                // 项目名称转ProjID
                var p = ls.Find(a => a.ProjName == projName);
                int projId = p == null ? 0 : p.ProjID;

                ProjMeeting r = new ProjMeeting();
                r.ProjID = projId;
                r.MeetingTopic = worksheet.Cells[row, 2].GetValue<string>();
                r.MeetingType = worksheet.Cells[row, 3].GetValue<string>();
                r.HostDept = worksheet.Cells[row, 4].GetValue<string>();
                r.HostPerson = worksheet.Cells[row, 5].GetValue<string>();
                r.ReviewExpert = worksheet.Cells[row, 6].GetValue<string>();
                r.Participants = worksheet.Cells[row, 7].GetValue<string>();

                r.NoticeNo = worksheet.Cells[row, 9].GetValue<string>();
                r.ReviewConclusion = worksheet.Cells[row, 10].GetValue<string>();
                r.MeetingConclusion = worksheet.Cells[row, 11].GetValue<string>();
                r.Remark = worksheet.Cells[row, 12].GetValue<string>();

                string meetingDate = worksheet.Cells[row, 8].GetValue<string>();
                if (!string.IsNullOrEmpty(meetingDate)) r.MeetingDate = DateTime.Parse(meetingDate);

                dbContext.ProjMeetings.Add(r);
            }
            // 保存
            int realNum = dbContext.SaveChanges();

            return string.Format("<p class='alert alert-success'>《{0}》处理成功！共{1}条数据，实际导入{2}条数据</p>", fileName, rowEnd - rowStart, realNum);

        }
    }
}
