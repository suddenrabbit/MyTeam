using MyTeam.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    /// <summary>
    /// 跟踪任务详情对应的需求
    /// </summary>
    public class TrackReq
    {
        [Key]
        public int TrackReqID { get; set; }

        [ForeignKey("TrackDetail")]
        public int TrackDetailID { get; set; }
        public virtual TrackDetail TrackDetail { get; set; } // TrackDetail 外键


        [Display(Name = "需求申请编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string TrackReqNo { get; set; }

        [Display(Name = "维护需求编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string TrackReqDetailNo { get; set; }        

        [Display(Name = "计划下发日期")]
        public DateTime? TrackPlanReleaseDate { get; set; }

        [Display(Name = "下发通知编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string TrackReleaseNo { get; set; }



    }
}