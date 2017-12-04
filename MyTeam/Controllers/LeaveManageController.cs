using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;
using MyTeam.Enums;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 外协人员请假管理Controller
    /// </summary>
    public class LeaveManageController : BaseController
    {
        //
        // GET: /LeaveManage/

        public ActionResult Index(int pageNum = 1)
        {           
            // 分页
            var ls = dbContext.Leaves.OrderByDescending(a => a.LeaveID).ToPagedList(pageNum, Constants.PAGE_SIZE);

            // 权限控制：外协人员不可以编辑
            var user = GetSessionCurrentUser();
            ViewBag.CanEdit = user.UserType != (int)UserTypeEnums.外协;

            return View(ls);
        }

        //
        // GET: /LeaveManage/Create

        public ActionResult Create()
        {
            SelectList sl = new SelectList(this.GetUserList().Where(a => a.UserType == (int)UserTypeEnums.外协), "UID", "Realname");

            ViewBag.PersonList = sl;

            return View();
        }

        //
        // POST: /LeaveManage/Create

        [HttpPost]
        public string Create(Leave Leave)
        {
            try
            {
                // 若请假天数大于1，则批量处理
                var days = Leave.LeaveDays;
                if(days > 1)
                {
                    int multiDays = (int)days;
                    var leaveDate = Leave.LeaveDate;
                    for(var i = 0; i<multiDays; i++)
                    {
                        var eachLeave = new Leave
                        {
                            LeaveDate = leaveDate,
                            PersonID = Leave.PersonID,
                            LeaveDays = 1                            
                        };
                        dbContext.Leaves.Add(eachLeave);
                        // 日期加一天
                        leaveDate = leaveDate.AddDays(1);
                    }
                }
                else
                {
                    dbContext.Leaves.Add(Leave);                    
                }

                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;

            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        //
        // GET: /LeaveManage/Edit/5

        public ActionResult Edit(int id)
        {
            Leave Leave = dbContext.Leaves.ToList().Find(a => a.LeaveID == id);

            if (Leave == null)
            {
                return View();
            }

            // 用户列表
            SelectList sl = new SelectList(this.GetUserList().Where(a => a.UserType == (int)UserTypeEnums.外协), "UID", "Realname", Leave.PersonID); // 选中当前值

            ViewBag.PersonList = sl;

            return View(Leave);
        }

        //
        // POST: /LeaveManage/Edit/5

        [HttpPost]
        public string Edit(Leave Leave)
        {
            try
            {
                dbContext.Entry(Leave).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /LeaveManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                Leave sys = dbContext.Leaves.ToList().Find(a => a.LeaveID == id);
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        // 导出excel
        [HttpGet]
        public ActionResult Export()
        {
            // 转换格式
            List<LeaveResult> r = new List<LeaveResult>();
            foreach(var a in dbContext.Leaves.ToList<Leave>() )
            {
                r.Add(new LeaveResult { LeaveDate = a.LeaveDate.ToString("yyyy/M/d"), PersonName = a.PersonName, LeaveDays = a.LeaveDays });
            }
            return this.MakeExcel<LeaveResult>("LeaveReportT", "外协人员请假统计表＿" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                r);
        }

        // 统计年度请假情况
        [HttpGet]
        public ActionResult SumUp()
        {
            var ls = dbContext.Database.SqlQuery<LeaveSumUp>("SELECT PersonID, sum(LeaveDays) as LeaveDays FROM Leaves WHERE year(LeaveDate) = @p0  GROUP BY PersonID", DateTime.Now.Year).ToList();

            return View(ls);
        }
    }


}
