using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;

namespace MyTeam.Controllers
{
    public class ProjController : BaseController
    {
        //
        // GET: /Proj/
        public ActionResult Index(ProjQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {            
            if (isQuery)
            {
                var ls = from a in dbContext.Projs
                         select a;

                if(query.ProjID != 0)
                {
                    ls = ls.Where(p => p.ProjID == query.ProjID);
                }

                if (!string.IsNullOrEmpty(query.ProjName))
                {
                    ls = ls.Where(p => p.ProjName.Contains(query.ProjName));
                }
                if (!string.IsNullOrEmpty(query.ProAcptDateStart))
                {
                    DateTime startDate = DateTime.Parse(query.ProAcptDateStart);
                    ls = ls.Where(p => p.ProAcptDate >= startDate);
                }
                if (!string.IsNullOrEmpty(query.ProAcptDateEnd))
                {
                    DateTime endDate = DateTime.Parse(query.ProAcptDateEnd);
                    ls = ls.Where(p => p.ProAcptDate <= endDate);
                }
                if (!string.IsNullOrEmpty(query.RulesPublishDateStart))
                {
                    DateTime startDate = DateTime.Parse(query.RulesPublishDateStart);
                    ls = ls.Where(p => p.RulesPublishDate >= startDate);
                }
                if (!string.IsNullOrEmpty(query.RulesPublishDateEnd))
                {
                    DateTime endDate = DateTime.Parse(query.RulesPublishDateEnd);
                    ls = ls.Where(p => p.RulesPublishDate <= endDate);
                }

                var result = ls.ToList();
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "项目状态跟踪表查询_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 需要对list修改以适应Excel模板
                    List<ProjResult> excelList = this.GetExcelList(ls);
                    return this.MakeExcel<ProjResult>("ProjReportT", targetFileName, excelList, 2);
                }
                else
                {
                    // 分页
                    query.ResultList = result.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE); ;
                }
            }
            else
            {
                query = new ProjQuery();
            }

            return View(query);
        }

        // GET: /Proj/Details/5

        public ActionResult Details(int id)
        {
            Proj proj = this.GetProjList().Find(a => a.ProjID == id);

            if (proj == null)
            {
                return View();
            }
            return View(proj);
        }

        //
        // GET: /Proj/Create

        public ActionResult Create()
        {
            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname");
            }

            ViewBag.ReqAnalysisList = sl2;

            // 主办部门
            ViewBag.ReqFromDeptList = MyTools.GetSelectListBySimpleEnum(typeof(Enums.ReqFromDeptEnums));

            // 项目状态
            ViewBag.ProjStatList = MyTools.GetSelectList(Constants.ProjStatList);

            return View();
        }

        //
        // POST: /Proj/Create

        [HttpPost]
        public string Create(Proj proj)
        {
            // 判断是否有重复的项目名称，如有重复不允许新增
            Proj p = this.GetProjList().Find(a => a.ProjName == proj.ProjName);
            if (p != null)
            {
                return "<p class='alert alert-danger'>出错了: " + proj.ProjName + "的项目跟踪状态已存在，不允许重复添加！" + "</p>";
            }

            try
            {

                dbContext.Projs.Add(proj);
                dbContext.SaveChanges();

                // 更新内存
                this.Update(3);

                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        //
        // GET: /Proj/Edit/5

        public ActionResult Edit(int id)
        {
            Proj proj = this.GetProjList().Find(a => a.ProjID == id);

            if (proj == null)
            {
                return View();
            }

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "Realname");
            }

            ViewBag.ReqAnalysisList = sl2;

            // 主办部门
            ViewBag.ReqFromDeptList = MyTools.GetSelectListBySimpleEnum(typeof(Enums.ReqFromDeptEnums));

            // 项目状态
            ViewBag.ProjStatList = MyTools.GetSelectList(Constants.ProjStatList);

            proj.OldProjName = proj.ProjName;
            return View(proj);
        }

        //
        // POST: /Proj/Edit/5

        [HttpPost]
        public string Edit(Proj proj)
        {
            if (proj.ProjName != proj.OldProjName)
            {
                // 若项目名称改变，则判断新改的系统名称是否有重复，如有重复不允许新增
                Proj p = this.GetProjList().Find(a => a.ProjName == proj.ProjName);
                if (p != null)
                {
                    return "<p class='alert alert-danger'>出错了: " + proj.ProjName + "的项目跟踪状态已存在，不允许更新！" + "</p>";
                }
            }

            try
            {
                dbContext.Entry(proj).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                // 更新内存
                this.Update(3);

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
                Proj proj = this.GetProjList().Find(a => a.ProjID == id);

                dbContext.Entry(proj).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                // 更新内存
                this.Update(3);

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        /// <summary>
        /// 生成用于Excel输出的list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<ProjResult> GetExcelList(IQueryable<Proj> list)
        {
            List<ProjResult> rl = new List<ProjResult>();

            List<User> userLs = this.GetUserList();
            User user = new User();

            foreach (Proj s in list)
            {
                user = userLs.Find(a => a.UID == s.ReqAnalysisID);

                ProjResult ProjResult = new ProjResult()
                {
                    ProjName = s.ProjName,
                    ProjNo = s.ProjNo,
                    HostDept = s.HostDept,
                    ProjLevel = s.ProjLevel,
                    ReqAnalysisID = user.Realname,
                    BusiPerson = s.BusiPerson,
                    ProjManager = s.ProjManager,
                    Architect = s.Architect,
                    ProAcptDate = s.ProAcptDate,
                    SurveyGroupFoundDate = s.SurveyGroupFoundDate,
                    SurveyFinishDate = s.SurveyFinishDate,
                    SurveyRemark = s.SurveyRemark,
                    OutlineWriter = s.OutlineWriter,
                    OutlineStartDate = s.OutlineStartDate,
                    OutlineEndDate = s.OutlineEndDate,
                    OutlineAuditPerson = s.OutlineAuditPerson,
                    OutlinePublishDate = s.OutlinePublishDate,
                    OutlineRemark = s.OutlineRemark,
                    ReqWriter = s.ReqWriter,
                    ReqStartDate = s.ReqStartDate,
                    ReviewAcptDate = s.ReviewAcptDate,
                    ReviewMeetingDate = s.ReviewMeetingDate,
                    ReqPublishDate = s.ReqPublishDate,
                    ReqRemark = s.ReqRemark,
                    RulesStartDate = s.RulesStartDate,
                    RulesPublishDate = s.RulesPublishDate,
                    RulesRemark = s.RulesRemark,
                    ProjCheckAcptDate = s.ProjCheckAcptDate,
                    ProjPublishDate = s.ProjPublishDate,
                    CheckResult = s.CheckResult,
                    Remark = s.Remark
                };
                rl.Add(ProjResult);
            }
            return rl;
        }
    }
}
