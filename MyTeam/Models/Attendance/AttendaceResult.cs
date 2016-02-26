using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    // 外协人员考勤（请假）情况
    public class AttendanceResult
    {
       
        [Display(Name = "请假日期")]
        public string LeaveDate { get; set; }

        [Required]
        [Display(Name = "请假人员")]
        public string PersonName { get; set; }

        [Display(Name = "请假天数")]
        public double LeaveDays { get; set; }     

       
    }
}