using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    // 外协人员请假情况
    public class LeaveResult
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