using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;

namespace MyTeam.Controllers
{
    public class ProjMeetingController : BaseController
    {
        //
        // GET: /projMeeting/

        public ActionResult Index(ProjMeetingQuery query, int pageNum = 1, bool isQuery = false, bool isExcel = false)
        {
            if (isQuery)
            {
                var ls = from a in dbContext.ProjMeetings
                         select a;
                if (query.ProjID != 0)
                {
                    ls = ls.Where(p => p.ProjID == query.ProjID);
                }
                if (query.MeetingType != "全部")
                {
                    ls = ls.Where(p => p.MeetingType == query.MeetingType);
                }
                if (!string.IsNullOrEmpty(query.MeetingDateStart) && !string.IsNullOrEmpty(query.MeetingDateEnd))
                {
                    DateTime startDate = DateTime.Parse(query.MeetingDateStart);
                    DateTime endDate = DateTime.Parse(query.MeetingDateEnd);
                    ls = ls.Where(p => p.MeetingDate.CompareTo(startDate) >= 0 && p.MeetingDate.CompareTo(endDate) <= 0);
                }
                var result = ls.OrderByDescending(a=>a.MeetingDate).ToList();
                
                // 若isExcel为true，导出Excel
                if (isExcel)
                {
                    string targetFileName = "项目会议查询_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 需要对list修改以适应Excel模板
                    List<ProjMeetingResult> excelList = this.GetExcelList(ls);
                    return this.MakeExcel<ProjMeetingResult>("ProjMeetingReportT", targetFileName, excelList);
                }
                else
                {
                    // 分页
                    query.ResultList = result.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE); 
                }
            }
            else
            {
                query = new ProjMeetingQuery();
            }

            // 为了保证查询部分正常显示，对下拉列表处理           
            // 获取项目下拉列表
            List<Proj> projLs = dbContext.Projs.ToList();
            // 加上“全部”
            projLs.Insert(0, new Proj() { ProjID = 0, ProjName = "全部" });
            ViewBag.ProjList = new SelectList(projLs, "ProjID", "ProjName", query.ProjID);
            // 会议类型列表
            ViewBag.MeetingTypeList = MyTools.GetSelectList(Constants.MeetingTypeList,true);

            return View(query);
        }

        //
        // GET: /projMeeting/Details/5

        public ActionResult Details(int id)
        {
            List<ProjMeeting> ls = dbContext.ProjMeetings.ToList();
            ProjMeeting projMeeting = ls.Find(a => a.MeetingID == id);

            if (projMeeting == null)
            {
                return View();
            }

            return View(projMeeting);
        }

        //
        // GET: /projMeeting/Create

        public ActionResult Create()
        {
            //项目列表
            List<Proj> ls = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;

            // 会议类型列表
            ViewBag.MeetingTypeList = MyTools.GetSelectList(Constants.MeetingTypeList);

            // 需求会议评审结果列表
            ViewBag.ReviewConclusionList = MyTools.GetSelectList(Constants.ReviewConclusionList);

            // 需求会议当前状态列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.StatList);

            return View();
        }

        //
        // POST: /projMeeting/Create

        [HttpPost]
        public string Create(ProjMeeting projMeeting)
        {
            try
            {
                dbContext.ProjMeetings.Add(projMeeting);
                dbContext.SaveChanges();
                
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /projMeeting/Edit/5

        public ActionResult Edit(int id)
        {
            List<ProjMeeting> rm = dbContext.ProjMeetings.ToList();
            ProjMeeting projMeeting = rm.Find(a => a.MeetingID == id);

            if (projMeeting == null)
            {
                return View();
            }

            //项目列表
            List<Proj> ls1 = dbContext.Projs.ToList();
            SelectList sl2 = null;
            sl2 = new SelectList(ls1, "ProjID", "ProjName");

            ViewBag.ProjList = sl2;

            // 会议类型列表
            ViewBag.MeetingTypeList = MyTools.GetSelectList(Constants.MeetingTypeList);

            // 需求会议评审结果列表
            ViewBag.ReviewConclusionList = MyTools.GetSelectList(Constants.ReviewConclusionList);

            // 需求会议当前状态列表
            ViewBag.StatList = MyTools.GetSelectList(Constants.StatList);

            return View(projMeeting);
        }

        //
        // POST: /projMeeting/Edit/5

        [HttpPost]
        public string Edit(ProjMeeting projMeeting)
        {
            try
            {
                dbContext.Entry(projMeeting).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /projMeeting/Delete/5

        public string Delete(int id)
        {
            try
            {
                List<ProjMeeting> ls = dbContext.ProjMeetings.ToList();
                ProjMeeting projMeeting = ls.Find(a => a.MeetingID == id);

                dbContext.Entry(projMeeting).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

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
        private List<ProjMeetingResult> GetExcelList(IQueryable<ProjMeeting> list)
        {
            List<ProjMeetingResult> rl = new List<ProjMeetingResult>();
            foreach (ProjMeeting s in list)
            {
                ProjMeetingResult ProjMeetingExcel = new ProjMeetingResult()
                {
                    ProjName = s.ProjName,
                    MeetingTopic = s.MeetingTopic,
                    MeetingType = s.MeetingType,
                    HostDept = s.HostDept,
                    HostPerson = s.HostPerson,
                    ReviewExpert = s.ReviewExpert,
                    Participants = s.Participants,
                    MeetingDate = s.MeetingDate,
                    NoticeNo = s.NoticeNo,
                    ReviewConclusion = s.ReviewConclusion,
                    MeetingConclusion = s.MeetingConclusion,
                    Stat = s.Stat,
                    Remark = s.Remark
                };
                rl.Add(ProjMeetingExcel);
            }
            return rl;
        }
    }
}
