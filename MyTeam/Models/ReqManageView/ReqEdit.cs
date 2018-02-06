using System;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class ReqEdit
    {
        public ReqMain reqMain { get; set; }
        public ReqDetail reqDetail { get; set; }
        public bool isUpdateMain { get; set; }

        public ReleaseForReqEdit reqRelease { get; set; }
    }

    public class ReleaseForReqEdit
    {
        [Display(Name = "下发通知编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        [RegularExpression(@"^[0-9a-zA-Z]+$", ErrorMessage = "下发通知编号只能是英文和数字<br />")]
        public string ReleaseNo { get; set; } // 唯一索引

        [Display(Name = "计划下发日期")]
        [DataType(DataType.Date)]
        public DateTime PlanReleaseDate { get; set; }

        [Display(Name = "实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        public string OldReleaseNo { get; set; }
    }
}