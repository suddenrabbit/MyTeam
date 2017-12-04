using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    // 外协人员考勤（请假）情况
    public class Leave
    {
        [Key]
        public int LeaveID { get; set; }

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
                return Utils.MyTools.GetUserName(PersonID);
            }
            set
            {
                this.PersonName = value;
            }
        } //用于显示UID对应的名字

       
    }
}