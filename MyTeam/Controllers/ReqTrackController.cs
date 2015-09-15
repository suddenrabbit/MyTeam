using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;
using PagedList;
using System.IO;
using OfficeOpenXml;

namespace MyTeam.Controllers
{
    public class ReqTrackController : BaseController
    {
        //
        // GET: /ReqTrack/
        public ActionResult Index(ReqTrackQuery query, int pageNum = 1, bool isQuery = false)
        {
            if (this.GetSessionCurrentUser() == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/ReqTrack" });
            }

            var ls = from a in dbContext.ReqTracks select a;
            if (isQuery)
            {
                if (query.ProjID != 0)
                {
                    ls = ls.Where(p => p.ProjID == query.ProjID);
                }
                // 分页
                query.ResultList = ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE);
            }
            else
            {
                query = new ReqTrackQuery();
            }

            // 项目列表
            var r = dbContext.Projs.ToList();
            // 添加全部
            r.Insert(0, new Proj() { ProjID = 0, ProjName = "全部" });
            ViewBag.ProjList = new SelectList(r, "ProjID", "ProjName", query.ProjID);

            return View(query);
        }

        //
        // GET: /ReqTrack/Details/5

        public ActionResult Details(int id)
        {
            List<ReqTrack> ls = dbContext.ReqTracks.ToList();
            ReqTrack reqTrack = ls.Find(a => a.TrackID == id);

            if (reqTrack == null)
            {
                return View();
            }

            return View(reqTrack);
        }

        //
        // GET: /ReqTrack/Create

        public ActionResult Create()
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl = null;
            sl = new SelectList(ls, "ProjID", "ProjName");

            ViewBag.ProjList = sl;

            // 优先级列表
            ViewBag.PriorityList = MyTools.GetSelectList(Constants.PriorityList);

            // 变更标识列表
            ViewBag.ChangeCharList = MyTools.GetSelectList(Constants.ChangeCharList);

            // 需求状态列表
            ViewBag.ReqSoftStatList = MyTools.GetSelectList(Constants.ReqSoftStatList);

            return View();
        }

        //
        // POST: /ReqTrack/Create

        [HttpPost]
        public string Create(ReqTrack reqTrack)
        {
            List<ReqTrack> list = dbContext.ReqTracks.ToList();
            ReqTrack track = list.Find(a => a.ReqNo == reqTrack.ReqNo && a.SoftReqNo == reqTrack.SoftReqNo);

            if(track != null){
                return "<p class='alert alert-danger'>出错了: 该记录已存在，不允许重复添加！" + "</p>";
            }

            try
            {
                
                    dbContext.ReqTracks.Add(reqTrack);
                    dbContext.SaveChanges();
                
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ReqTrack/Edit/5

        public ActionResult Edit(int id)
        {
            List<ReqTrack> rm = dbContext.ReqTracks.ToList();
            ReqTrack reqTrack = rm.Find(a => a.TrackID == id);

            //项目列表
            List<Proj> ls1 = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls1, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;

            if (reqTrack == null)
            {
                return View();
            }

            // 优先级列表
            ViewBag.PriorityList = MyTools.GetSelectList(Constants.PriorityList);

            // 变更标识列表
            ViewBag.ChangeCharList = MyTools.GetSelectList(Constants.ChangeCharList);

            // 需求状态列表
            ViewBag.ReqSoftStatList = MyTools.GetSelectList(Constants.ReqSoftStatList);

            return View(reqTrack);
        }

        //
        // POST: /ReqTrack/Edit/5

        [HttpPost]
        public string Edit(ReqTrack reqTrack)
        {
            try
            {
                dbContext.Entry(reqTrack).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // POST: /ReqTrack/Delete/5

        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                List<ReqTrack> ls = dbContext.ReqTracks.ToList();
                ReqTrack reqTrack = ls.Find(a => a.TrackID == id);

                dbContext.Entry(reqTrack).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
            // 项目列表
            var r = dbContext.Projs.ToList();

            ViewBag.ProjList = new SelectList(r, "ProjID", "ProjName");

            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file, int ProjID)
        {
            if (file == null)
            {
                ViewBag.Msg = "<p class='alert alert-danger'>未获取到文件</p>";
            }
            else
            {
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

                        int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                        int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号
                        var ls = dbContext.ReqTracks.ToList();

                        var tmpLs = new List<ReqTrack>(ls.ToArray());
                        
                        for (int row = rowStart + 1; row <= rowEnd; row++)
                        {
                            // 第一列为空则结束
                            if (worksheet.Cells[row, 1] == null) break;

                            string ReqNo = worksheet.Cells[row, 1].GetValue<string>();
                            string softReqNo = worksheet.Cells[row, 10].GetValue<string>();

                            // ProjID+BusiReqNo重复的不导入
                            if (tmpLs.Find(a => a.ProjID == ProjID && a.ReqNo == ReqNo && a.SoftReqNo == softReqNo) != null)
                            {
                                continue;
                            }

                            ReqTrack br = new ReqTrack();
                            // 按列赋值
                            br.ProjID = ProjID;
                            br.ReqNo = ReqNo;
                            br.ReqName = worksheet.Cells[row, 2].GetValue<string>();
                            br.Priority = worksheet.Cells[row, 3].GetValue<string>();
                            br.ReqWriter = worksheet.Cells[row, 4].GetValue<string>();
                            // 计划完成日期若为空则记录为当前时间
                            string PlanDeadLine = worksheet.Cells[row, 5].GetValue<string>();
                            
                            // 实际完成日期若为空则记录为当前时间
                            string RealDeadLine = worksheet.Cells[row, 6].GetValue<string>();
                            
                            br.ChangeChar = worksheet.Cells[row, 7].GetValue<string>();
                            br.ApprovePerson = worksheet.Cells[row, 8].GetValue<string>();
                            // 批准日期若为空则记录为当前时间
                            string ApproveDate = worksheet.Cells[row, 9].GetValue<string>();
                           
                            br.SoftReqNo = worksheet.Cells[row, 10].GetValue<string>();
                            br.SoftReqName = worksheet.Cells[row, 11].GetValue<string>();
                            br.ReqStat = worksheet.Cells[row, 12].GetValue<string>();

                            dbContext.ReqTracks.Add(br);
                            tmpLs.Add(br);
                        }
                        // 保存
                        int realNum = dbContext.SaveChanges();

                        string s = string.Format("<p class='alert alert-success'>《{0}》处理成功！共{1}条数据，实际导入{2}条数据</p>", file.FileName, rowEnd - rowStart, realNum);
                        
                        ViewBag.Msg = s;
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
            }

            // 项目列表
            var r = dbContext.Projs.ToList();

            ViewBag.ProjList = new SelectList(r, "ProjID", "ProjName", ProjID);

            return View();
        }

        /// <summary>
        /// 下载模板
        /// </summary>
        /// <returns></returns>
        public FileResult DownTemplate()
        {
            string fileName = "ReqTrackImportT.xlsx";
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/" + fileName);
            return File(tmpFilePath, "application/excel", fileName);
        }

    }
}
