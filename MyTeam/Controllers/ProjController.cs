using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;
using PagedList;

namespace MyTeam.Controllers
{
#if RELEASE
    [Authorize]
#endif
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

                if (!string.IsNullOrEmpty(query.ProAcptDate))
                {
                    DateTime ProAcptDate = DateTime.Parse(query.ProAcptDate);
                    ls = ls.Where(p => p.ProAcptDate == ProAcptDate);
                }
                if (!string.IsNullOrEmpty(query.RulesPublishDate))
                {
                    DateTime RulesPublishDate = DateTime.Parse(query.RulesPublishDate);
                    ls = ls.Where(p => p.RulesPublishDate == RulesPublishDate);
                }
                var result = ls.ToList();
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "项目状态跟踪表查询_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 需要对list修改以适应Excel模板
                    List<ProjResult> excelList = this.GetExcelList(ls);
                    return this.makeExcel<ProjResult>("ProjReportT", targetFileName, excelList , 2);
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

            // 需求分析师显示用户名不是显示ID
            List<User> userLs = this.GetUserList();

            // 是否跟踪需求变更的正常显示

            return View(query);

        }
        
        // GET: /Proj/Details/5

        public ActionResult Details(int id)
        {
            
            List<Proj> ls = dbContext.Projs.ToList();
            Proj proj = ls.Find(a => a.ProjID == id);

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
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone");
            }

            ViewBag.ReqAnalysisList = sl2;

            // 主办部门
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            return View();
        }

        //
        // POST: /Proj/Create

        [HttpPost]
        public string Create(Proj proj)
        {
            // 判断是否有重复的项目名称，如有重复不允许新增
            List<Proj> projLs = dbContext.Projs.ToList();
            foreach(Proj p in projLs){
                if(p.ProjName == proj.ProjName){
                    return "<p class='alert alert-danger'>出错了: " + proj.ProjName + "的项目跟踪状态已存在，不允许重复添加！" + "</p>";
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Projs.Add(proj);
                    dbContext.SaveChanges();
                }
                return "<p class='alert alert-success'>新增成功</p>";
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
            List<Proj> ls = dbContext.Projs.ToList();
            Proj proj = ls.Find(a => a.ProjID == id);

            if (proj == null)
            {
                return View();
            }

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone");
            }

            ViewBag.ReqAnalysisList = sl2;

            // 主办部门
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            return View(proj);
        }

        //
        // POST: /Proj/Edit/5

        [HttpPost]
        public string Edit(Proj proj)
        {
            // 判断是否有重复的项目名称，如有重复不允许新增
            List<Proj> projLs = dbContext.Projs.ToList();
            foreach (Proj p in projLs)
            {
                if (p.ProjName == proj.ProjName)
                {
                    return "<p class='alert alert-danger'>出错了: " + "项目名称重复，不允许修改！" + "</p>";
                }
            }

            try
            {
                dbContext.Entry(proj).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return "<p class='alert alert-success'>更新成功</p>";
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
                List<Proj> ls = dbContext.Projs.ToList();
                Proj proj = ls.Find(a => a.ProjID == id);

                dbContext.Entry(proj).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                // 更新内存
                this.Update();

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
                    IsReqTrack = (s.IsReqTrack) ? "是" : "否",
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
                    Remark = s.Remark,
                    WorkTimeAtt = s.WorkTimeAtt
                };
                rl.Add(ProjResult);
            }
            return rl;
        }
    }
}
