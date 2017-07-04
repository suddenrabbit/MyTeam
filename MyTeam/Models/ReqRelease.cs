using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class ReqRelease
    {
        // 需求下发信息

        [Key]
        public int ReqReleaseID { get; set; }

        [Required]
        [Display(Name = "下发通知编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ReleaseNo { get; set; } // 唯一索引

        [Display(Name = "计划下发日期")]
        [DataType(DataType.Date)]
        public DateTime PlanReleaseDate { get; set; }

        [Display(Name = "实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; } 
        
        [Display(Name = "是否副下发")]
        [System.ComponentModel.DefaultValue(false)]
        public bool IsSideRelease { get; set; }

        /*[Display(Name = "对应主下发编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RelatedMainReleaseNo { get; set; }*/  
        
        [NotMapped]
        public string OldReleaseNo { get; set; }

    }
}