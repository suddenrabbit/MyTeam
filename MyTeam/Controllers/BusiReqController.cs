using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using MyTeam.Utils;
using MyTeam.Models;
using System.IO;
using OfficeOpenXml;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 业需明细管理
    /// </summary>
    public class BusiReqController : BaseController
    {
        //
        // GET: /BusiReq/
        public ActionResult Index(BusiReqQuery query, int pageNum = 1, bool isQuery = false)
        {
            if (this.GetSessionCurrentUser() == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/BusiReq" });
            }

            var ls = from a in dbContext.BusiReqs select a;
            if (isQuery)
            {
                if (query.BRProjID != 0)
                {
                    ls = ls.Where(br => br.BRProjID == query.BRProjID);
                }
                // 分页
                query.ResultList = ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE);
            }
            else
            {
                query = new BusiReqQuery();
            }

            List<BusiReqProj> brProjLs = dbContext.BusiReqProjs.ToList();
            // 加上“全部”
            brProjLs.Insert(0, new BusiReqProj() { BRProjID = 0, BRProjName = "全部" });
            ViewBag.BRProjLs = new SelectList(brProjLs, "BRProjID", "BRProjName", query.BRProjID);

            return View(query);
        }

        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {
            // 需求来源及状态的下拉列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.BusiReqStat);

            List<BusiReqProj> brProjLs = dbContext.BusiReqProjs.ToList();
            ViewBag.BRProjLs = new SelectList(brProjLs, "BRProjID", "BRProjName");

            return View();
        }

        //
        // POST: /SysManage/Create

        [HttpPost]
        public string Create(BusiReq br)
        {
            try
            {
                var ls = dbContext.BusiReqs.ToList();
                // ProjID+BusiReqNo重复的不导入
                if (ls.Find(a => a.BRProjID == br.BRProjID && a.BusiReqNo == br.BusiReqNo) != null)
                {
                    return "<p class='alert alert-danger'>出错了: 记录已存在，不允许重复添加！" + "</p>";
                }

                dbContext.BusiReqs.Add(br);
                dbContext.SaveChanges();
                
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /SysManage/Edit/5

        public ActionResult Edit(int id)
        {
            BusiReq br = dbContext.BusiReqs.ToList().Find(a => a.BRID == id);

            if (br == null)
            {
                return View();
            }

            List<BusiReqProj> brProjLs = dbContext.BusiReqProjs.ToList();
            ViewBag.BRProjLs = new SelectList(brProjLs, "BRProjID", "BRProjName");

            // 需求来源及状态的下拉列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.BusiReqStat, false, true, br.Stat);

            return View(br);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public string Edit(BusiReq br)
        {
            try
            {
                dbContext.Entry(br).State = System.Data.Entity.EntityState.Modified;
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
                BusiReq br = dbContext.BusiReqs.ToList().Find(a => a.BRID == id);
                dbContext.Entry(br).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /// <summary>
        /// 导出跟踪表
        /// </summary>
        /// <param name="BRProjID"></param>
        /// <returns></returns>
        public ActionResult Export(string BRProjID)
        {
            //List<Proj> projLs = this.GetProjList().Where(a => a.IsReqTrack == true).ToList();
            //string pName = " ";
            //foreach(Proj p in projLs){
            //    if(p.ProjID.ToString() == ProjID){
            //        pName = p.ProjName;
            //    }
            // }

            // 联立查询 BusiReqs 和 Reqs
            var ls = from br in dbContext.BusiReqs
                     join req in dbContext.Reqs
                     on br.BusiReqNo equals req.BusiReqNo
                     where br.BRProjID.ToString() == BRProjID

                     select new BusiReqExcel
                     {
                         ProjName = "".ToString(),
                         BusiReqNo = br.BusiReqNo,
                         BusiReqName = br.BusiReqName,
                         Desc = br.Desc,
                         CreateDate = br.CreateDate,
                         Stat = br.Stat,
                         AcptDate = req.AcptDate,
                         ReqNo = req.ReqNo,
                         ReqDetailNo = req.ReqDetailNo,
                         Version = req.Version,
                         ReqReason = req.ReqReason,
                         ReqDesc = req.ReqDesc,
                         ReqFromPerson = req.ReqFromPerson,
                         ReqType = req.ReqType,
                         RlsDate = req.RlsDate
                     };

            return this.MakeExcel<BusiReqExcel>("BusiReqReportT", "业务需求变更跟踪", ls.ToList<BusiReqExcel>(), 2);

        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
            List<BusiReqProj> brProjLs = dbContext.BusiReqProjs.ToList();
            ViewBag.BRProjLs = new SelectList(brProjLs, "BRProjID", "BRProjName");

            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file, int BRProjID)
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

                        //int colStart = worksheet.Dimension.Start.Column;  //工作区开始列
                        //int colEnd = worksheet.Dimension.End.Column;       //工作区结束列
                        int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                        int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                        var ls = dbContext.BusiReqs.ToList();

                        for (int row = rowStart + 1; row <= rowEnd; row++)
                        {
                            string busiReqNo = worksheet.Cells[row, 1].GetValue<string>();

                            // ProjID+BusiReqNo重复的不导入
                            if (ls.Find(a => a.BRProjID == BRProjID && a.BusiReqNo == busiReqNo) != null)
                            {
                                continue;
                            }

                            BusiReq br = new BusiReq();
                            // 按列赋值
                            br.BRProjID = BRProjID;
                            br.BusiReqNo = busiReqNo;
                            br.BusiReqName = worksheet.Cells[row, 2].GetValue<string>();
                            br.Desc = worksheet.Cells[row, 3].GetValue<string>();
                            // 创建日期若为空则记录为当前时间
                            string createDate = worksheet.Cells[row, 4].GetValue<string>();
                            DateTime tmp = new DateTime();
                            if (!string.IsNullOrEmpty(createDate) && DateTime.TryParse(createDate, out tmp))
                            {
                                br.CreateDate = tmp;
                            }
                            else
                            {
                                br.CreateDate = DateTime.Now;
                            }
                            br.Stat = worksheet.Cells[row, 5].GetValue<string>();

                            dbContext.BusiReqs.Add(br);
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

            List<BusiReqProj> brProjLs = dbContext.BusiReqProjs.ToList();
            ViewBag.BRProjLs = new SelectList(brProjLs, "BRProjID", "BRProjName");

            return View();
        }

        /// <summary>
        /// 下载模板
        /// </summary>
        /// <returns></returns>
        public FileResult DownTemplate()
        {
            string fileName = "BusiReqImportT.xlsx";
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/" + fileName);
            return File(tmpFilePath, "application/excel", fileName);
        }
    }
}
