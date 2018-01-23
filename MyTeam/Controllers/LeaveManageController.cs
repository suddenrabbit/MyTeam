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

        public ActionResult Index(string year, int pageNum = 1)
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            // 分页
            var ls = dbContext.Leaves.Where(p => p.LeaveDate.Year.ToString() == year).OrderByDescending(a => a.LeaveID).ToPagedList(pageNum, Constants.PAGE_SIZE);

            // 权限控制：外协人员不可以编辑
            var user = GetSessionCurrentUser();
            ViewBag.CanEdit = user.UserType != (int)UserTypeEnums.外协;

            ViewBag.year = year;

            return View(ls);
        }

        //
        // GET: /LeaveManage/Create

        public ActionResult Create()
        {
            var ls = this.GetUserList().Where(a => a.UserType == (int)UserTypeEnums.外协).ToList();
            ls.Insert(0, new User { UID = 0, Realname = "请选择..." });

            SelectList sl = new SelectList(ls, "UID", "Realname");

            ViewBag.PersonList = sl;

            return View();
        }

        //
        // POST: /LeaveManage/Create

        [HttpPost]
        public string Create(Leave leave, int RestTimes)
        {
            try
            {
                var days = leave.LeaveDays;

                if (days < 1 && days != 0.5)
                {
                    return "<p class='alert alert-danger'>请填写正确的请假天数！（0.5、1 或 其他正整数）</p>";
                }

                if (days != 0.5 && leave.IsDeducted)
                {
                    return "<p class='alert alert-danger'>只有请假半天的时候可以抵扣！</p>";
                }

                if (RestTimes < 1 && leave.IsDeducted)
                {
                    return "<p class='alert alert-danger'>当前无可抵扣次数！</p>";
                }

                // 若请假天数大于1，则批量处理
                if (days > 1)
                {
                    int multiDays = (int)days;
                    var leaveDate = leave.LeaveDate;
                    for (var i = 0; i < multiDays; i++)
                    {
                        var eachLeave = new Leave
                        {
                            LeaveDate = leaveDate,
                            PersonID = leave.PersonID,
                            LeaveDays = 1
                        };
                        dbContext.Leaves.Add(eachLeave);
                        // 日期加一天
                        leaveDate = leaveDate.AddDays(1);
                    }
                }
                else
                {
                    dbContext.Leaves.Add(leave);
                }

                // 对于抵扣的半天，在加班记录中减去4个小时
                if (days == 0.5 && leave.IsDeducted)
                {
                    var ot = new OT
                    {
                        OTDate = leave.LeaveDate,
                        PersonID = leave.PersonID,
                        OTHours = -4,
                        Remark = "请假半天抵扣（此处加班日期代表请假日期）",
                        IsReserved = true
                    };
                    dbContext.OTs.Add(ot);
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
                Leave leave = dbContext.Leaves.Where(a => a.LeaveID == id).FirstOrDefault();
                dbContext.Entry(leave).State = System.Data.Entity.EntityState.Deleted;

                // 若有抵扣，则需要在加班时间中相应增加4个小时
                if (leave.IsDeducted)
                {
                    var ot = new OT
                    {
                        OTDate = leave.LeaveDate,
                        PersonID = leave.PersonID,
                        OTHours = 4,
                        Remark = "删除加班抵扣的请假记录（此处加班日期代表请假日期）",
                        IsReserved = true
                    };
                    dbContext.OTs.Add(ot);
                }

                dbContext.SaveChanges();

                return "<p class='alert alert-success'>删除成功</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 导出excel
        [HttpGet]
        public ActionResult Export(string year)
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            // 转换格式
            List<LeaveResult> r = new List<LeaveResult>();
            foreach (var a in dbContext.Leaves.Where(p => p.LeaveDate.Year.ToString() == year).ToList<Leave>())
            {
                r.Add(new LeaveResult { LeaveDate = a.LeaveDate.ToString("yyyy/M/d"), PersonName = a.PersonName, LeaveDays = a.LeaveDays });
            }
            return this.MakeExcel<LeaveResult>("LeaveReportT", "外协人员请假统计表＿" + year + "年度", r);
        }

        // 统计年度请假情况
        [HttpGet]
        public ActionResult SumUp(string year)
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            var ls = dbContext.Database.SqlQuery<LeaveSumUp>("SELECT PersonID, sum(LeaveDays) as LeaveDays FROM Leaves WHERE year(LeaveDate) = @p0  GROUP BY PersonID", year).ToList();

            return View(ls);
        }

        // 接口：统计员工可以抵扣的请假次数
        [HttpGet]
        public int GetRestTimes(int uid)
        {
            // 获取该员工当前可以抵扣的请假天数：
            var otList = dbContext.OTs.Where(p => p.PersonID == uid);

            if (otList == null || otList.Count() < 1)
            {
                return 0;
            }

            int otHoursInt = (int)otList.Sum(p => p.OTHours);
            while (otHoursInt % 4 > 0)
            {
                otHoursInt--;
            }
            return otHoursInt / 4;
        }

    }


}
