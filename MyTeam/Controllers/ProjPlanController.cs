using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;
using System.IO;
using OfficeOpenXml;

namespace MyTeam.Controllers
{
    public class ProjPlanController : BaseController
    {
        //
        // GET: /ProjPlan/
        public ActionResult Index(ProjPlanQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {
            if (isQuery)
            {
                var ls = from a in dbContext.ProjPlans select a;
                if (query.ProjID != 0)
                {
                    ls = ls.Where(p => p.ProjID == query.ProjID);
                }
                if (!string.IsNullOrEmpty(query.PlanYear))
                {
                    ls = ls.Where(p => p.PlanYear == query.PlanYear);
                }

                // 排序
                ls = ls.OrderByDescending(p => p.PlanYear).OrderByDescending(p => p.PlanID);

                var result = ls.ToList();
                // 分页
                query.ResultList = result.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE);
            }
            else
            {
                query = new ProjPlanQuery();
            }

            // 为了保证查询部分正常显示，对下拉列表处理           
            // 获取项目下拉列表
            List<Proj> projLs = dbContext.Projs.ToList();
            // 加上“全部”
            projLs.Insert(0, new Proj() { ProjID = 0, ProjName = "全部" });
            ViewBag.ProjList = new SelectList(projLs, "ProjID", "ProjName", query.ProjID);

            List<ProjPlan> list = dbContext.ProjPlans.ToList();
            foreach (ProjPlan rs in list)
            {
                Proj p = projLs.Find(a => a.ProjID == rs.ProjID);
                rs.ProjName = p == null ? "未知" : p.ProjName;
            }

            return View(query);
        }


        // GET: /Proj/Details/5

        public ActionResult Details(int id)
        {
            ProjPlan projPlan = dbContext.ProjPlans.Where(a => a.ProjID == id).FirstOrDefault();

            if (projPlan == null)
            {
                return View();
            }
            return View(projPlan);
        }

        //
        // GET: /ProjPlan/Create

        public ActionResult Create(int id = 0)
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl = null;

            if(id == 0)
            {
                sl = new SelectList(ls, "ProjID", "ProjName");
            }
            else
            {
                sl = new SelectList(ls, "ProjID", "ProjName", id);
            }

            ViewBag.ProjList = sl;

            return View();
        }

        //
        // POST: /ProjPlan/Create

        [HttpPost]
        public string Create(ProjPlan projPlan)
        {
            // 判断是否有重复的项目名称，如有重复不允许新增
            ProjPlan plan = dbContext.ProjPlans.Where(a => a.ProjID == projPlan.ProjID).FirstOrDefault();
            if (plan != null)
            {
                return "<p class='alert alert-danger'>出错了: " + projPlan.ProjName + "的项目计划表已存在，不允许重复添加！" + "</p>";
            }

            try
            {
                dbContext.ProjPlans.Add(projPlan);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ProjPlan/Edit/5

        public ActionResult Edit(int id)
        {
            ProjPlan projPlan = dbContext.ProjPlans.Where(a => a.PlanID == id).FirstOrDefault();

            if (projPlan == null)
            {
                return View();
            }

            //项目列表
            List<Proj> ls1 = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls1, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;
            projPlan.OldProjID = projPlan.ProjID;

            return View(projPlan);
        }

        //
        // POST: /ProjPlan/Edit/5

        [HttpPost]
        public string Edit(ProjPlan projPlan)
        {
            if (projPlan.ProjID != projPlan.OldProjID)
            {
                // 若项目名称改变，则判断新改的系统名称是否有重复，如有重复不允许新增
                ProjPlan plan = dbContext.ProjPlans.ToList().Find(a => a.ProjName == projPlan.ProjName);
                if (plan != null)
                {
                    return "<p class='alert alert-danger'>出错了: " + projPlan.ProjName + "的项目计划表已存在，不允许更新！" + "</p>";
                }
            }

            try
            {
                dbContext.Entry(projPlan).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /ProjPlan/Delete/5

        public string Delete(int id)
        {
            try
            {
                ProjPlan projPlan = dbContext.ProjPlans.Where(a => a.PlanID == id).FirstOrDefault();

                dbContext.Entry(projPlan).State = System.Data.Entity.EntityState.Deleted;
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
                        var ls = dbContext.ProjPlans.ToList();

                        var tmpLs = new List<ProjPlan>(ls.ToArray());

                        for (int row = rowStart + 1; row <= rowEnd; row++)
                        {
                            // 第一列为空则结束
                            if (worksheet.Cells[row, 1] == null) break;

                            // ProjID重复的不导入
                            if (tmpLs.Find(a => a.ProjID == ProjID) != null)
                            {
                                continue;
                            }

                            ProjPlan p = new ProjPlan();
                            // 按列赋值
                            p.ProjID = ProjID;
                            string FoundGroupDate = worksheet.Cells[row, 1].GetValue<string>();
                            string OutlineStartDate = worksheet.Cells[row, 2].GetValue<string>();
                            string OutlineFinishDate = worksheet.Cells[row, 3].GetValue<string>();
                            string ReqStartDate = worksheet.Cells[row, 4].GetValue<string>();
                            string ReqFinishDate = worksheet.Cells[row, 5].GetValue<string>();
                            string ReviewStartDate = worksheet.Cells[row, 6].GetValue<string>();
                            string ReviewFinishDate = worksheet.Cells[row, 7].GetValue<string>();
                            string BusiFeasiStartDate = worksheet.Cells[row, 8].GetValue<string>();
                            string BusiFeasiFinishDate = worksheet.Cells[row, 9].GetValue<string>();
                            string TechFeasiStartDate = worksheet.Cells[row, 10].GetValue<string>();
                            string TechFeasiFinishDate = worksheet.Cells[row, 11].GetValue<string>();
                            string TechFeasiReviewStartDate = worksheet.Cells[row, 12].GetValue<string>();
                            string TechFeasiReviewFinishDate = worksheet.Cells[row, 13].GetValue<string>();
                            string SoftBudgetStartDate = worksheet.Cells[row, 14].GetValue<string>();
                            string SoftBudgetFinishDate = worksheet.Cells[row, 15].GetValue<string>();
                            string ImplementPlansStartDate = worksheet.Cells[row, 16].GetValue<string>();
                            string ImplementPlansFinishDate = worksheet.Cells[row, 17].GetValue<string>();

                            if (!string.IsNullOrEmpty(FoundGroupDate)) p.FoundGroupDate = DateTime.Parse(worksheet.Cells[row, 1].GetValue<string>());
                            if (!string.IsNullOrEmpty(OutlineStartDate)) p.OutlineStartDate = DateTime.Parse(worksheet.Cells[row, 2].GetValue<string>());
                            if (!string.IsNullOrEmpty(OutlineFinishDate)) p.OutlineFinishDate = DateTime.Parse(worksheet.Cells[row, 3].GetValue<string>());
                            if (!string.IsNullOrEmpty(ReqStartDate)) p.ReqStartDate = DateTime.Parse(worksheet.Cells[row, 4].GetValue<string>());
                            if (!string.IsNullOrEmpty(ReqFinishDate)) p.ReqFinishDate = DateTime.Parse(worksheet.Cells[row, 5].GetValue<string>());
                            if (!string.IsNullOrEmpty(ReviewStartDate)) p.ReviewStartDate = DateTime.Parse(worksheet.Cells[row, 6].GetValue<string>());
                            if (!string.IsNullOrEmpty(ReviewFinishDate)) p.ReviewFinishDate = DateTime.Parse(worksheet.Cells[row, 7].GetValue<string>());
                            if (!string.IsNullOrEmpty(BusiFeasiStartDate)) p.BusiFeasiStartDate = DateTime.Parse(worksheet.Cells[row, 8].GetValue<string>());
                            if (!string.IsNullOrEmpty(BusiFeasiFinishDate)) p.BusiFeasiFinishDate = DateTime.Parse(worksheet.Cells[row, 9].GetValue<string>());
                            if (!string.IsNullOrEmpty(TechFeasiStartDate)) p.TechFeasiStartDate = DateTime.Parse(worksheet.Cells[row, 10].GetValue<string>());
                            if (!string.IsNullOrEmpty(TechFeasiFinishDate)) p.TechFeasiFinishDate = DateTime.Parse(worksheet.Cells[row, 11].GetValue<string>());
                            if (!string.IsNullOrEmpty(TechFeasiReviewStartDate)) p.TechFeasiReviewStartDate = DateTime.Parse(worksheet.Cells[row, 12].GetValue<string>());
                            if (!string.IsNullOrEmpty(TechFeasiReviewFinishDate)) p.TechFeasiReviewFinishDate = DateTime.Parse(worksheet.Cells[row, 13].GetValue<string>());
                            if (!string.IsNullOrEmpty(SoftBudgetStartDate)) p.SoftBudgetStartDate = DateTime.Parse(worksheet.Cells[row, 14].GetValue<string>());
                            if (!string.IsNullOrEmpty(SoftBudgetFinishDate)) p.SoftBudgetFinishDate = DateTime.Parse(worksheet.Cells[row, 15].GetValue<string>());
                            if (!string.IsNullOrEmpty(ImplementPlansStartDate)) p.ImplementPlansStartDate = DateTime.Parse(worksheet.Cells[row, 16].GetValue<string>());
                            if (!string.IsNullOrEmpty(ImplementPlansFinishDate)) p.ImplementPlansFinishDate = DateTime.Parse(worksheet.Cells[row, 17].GetValue<string>());

                            dbContext.ProjPlans.Add(p);
                            tmpLs.Add(p);
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
            string fileName = "ProjPlanImportT.xlsx";
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/" + fileName);
            return File(tmpFilePath, "application/excel", fileName);
        }

        /// <summary>
        /// 项目管理页面直接管理项目计划
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditPlanFromProjIndex(int id)
        {
            var opType = 1; //0-add 1-edit
            var projPlan = dbContext.ProjPlans.Where(p=>p.ProjID == id).FirstOrDefault();
            if(projPlan == null)
            {
                projPlan = new ProjPlan { ProjID = id, PlanYear = DateTime.Now.Year.ToString() };
                opType = 0;
            }

            // Get ProjName
            var proj = Constants.ProjList.Find(p => p.ProjID == id);
            projPlan.ProjName = proj.ProjName;

            ViewBag.OpType = opType;

            return View(projPlan);
        }

        [HttpPost]
        public string EditPlanFromProjIndex(ProjPlan projPlan, int opType)
        {
            try
            {
                if(opType == 0)
                {
                    dbContext.ProjPlans.Add(projPlan);
                }
                else
                {
                    dbContext.Entry(projPlan).State = System.Data.Entity.EntityState.Modified;
                }
                
                dbContext.SaveChanges();

                return "<p class='alert alert-success'>更新成功！<a href='#' data-dismiss='modal'>关闭</a></p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

    }
}
