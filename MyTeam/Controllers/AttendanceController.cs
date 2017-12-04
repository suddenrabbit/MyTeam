using MyTeam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class AttendanceController:BaseController
    {
        public ActionResult Index(string year)
        {
            if(string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            var leaveList = dbContext.Database.SqlQuery<LeaveSumUp>("SELECT PersonID, sum(LeaveDays) as LeaveDays FROM Leaves WHERE year(LeaveDate) = @p0  GROUP BY PersonID", year).ToList();
            var otList = dbContext.Database.SqlQuery<OTSumUp>("SELECT PersonID, sum(OTHours) as OTHours FROM OTs WHERE year(OTDate) = @p0  GROUP BY PersonID", year).ToList();

            var userList = GetUserList().Where(p => p.UserType == (int)Enums.UserTypeEnums.外协);

            var resultList = new List<AttendanceSumUp>();

            foreach(var u in userList)
            {
                var uid = u.UID;

                var leave = leaveList.Where(p => p.PersonID == uid).FirstOrDefault();
                var ot = otList.Where(p => p.PersonID == uid).FirstOrDefault();

                var leaveDays = leave == null ? 0 : leave.LeaveDays;
                var otHours = ot == null ? 0 : ot.OTHours;

                // 计算加班时间可以抵扣的请假天数
                var otAsDays = this.calOtAsDays(otHours);

                var sumUp = new AttendanceSumUp()
                {
                    PersonID = uid,
                    LeaveDays = leaveDays,
                    OTHours = otHours,
                    OTAsDays = otAsDays
                };
                resultList.Add(sumUp);
            }

            ViewBag.year = year;

            return View(resultList);
        }

        // 根据加班小时计算加班抵扣天数，满4小时为半天
        private double calOtAsDays(double otHours)
        {
            while(otHours % 4 > 0)
            {
                otHours--;
            }
            return otHours / 4 * 0.5;
        }
    }
}