using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    // 外协人员考勤（请假）情况
    public class Attendance
    {
        [Key]
        public int AID { get; set; }

        [Required]
        [Display(Name = "请假日期")]
        [DataType(DataType.Date)]
        public DateTime LeaveDate { get; set; }

        [Required]
        [Display(Name = "请假人员")]
        public int PersonID { get; set; }

        [Display(Name = "请假天数")]
        public double LeaveDays { get; set; }

        [NotMapped]
        public string PersonName
        {
            get
            {
                var s = (from a in Constants.UserList
                         where a.UID == this.PersonID
                         select a.Realname).FirstOrDefault();
                return s == null ? "未知" : s.ToString();
            }
            set
            {
                this.PersonName = value;
            }
        } //用于显示UID对应的名字

       
    }
}