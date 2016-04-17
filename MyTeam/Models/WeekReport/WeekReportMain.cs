using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class WeekReportMain
    {
        [Key]
        public int WRMainID { get; set; }

        [Display(Name = "任务年度")]
        [StringLength(4, ErrorMessage = "不能超过4位")]
        public string WorkYear { get; set; }

        [Required]
        [Display(Name = "任务名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string WorkName { get; set; }

        [Display(Name = "工作进展")]
        [StringLength(128, ErrorMessage = "不能超过128位")]
        public string WorkStage { get; set; }

        [Display(Name = "工作内容")]
        public string WorkMission { get; set; }

        [Required]
        [Display(Name = "负责人")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string Person { get; set; }        

        [Display(Name = "目标或交付物")]
        public string WorkTarget { get; set; }

        [Display(Name = "计划完成时间")]
        [DataType(DataType.Date)]
        public DateTime? PlanDeadLine { get; set; }

        [Display(Name = "整体完成率")]
        [Range(0,100, ErrorMessage="只能填0-100之间的数字")]
        public int Progress { get; set; }

        [Display(Name = "工时（人小时）")]
        public double WorkTime { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }

        [Display(Name = "工作类型")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string WorkType { get; set; }

        [Display(Name = "现场技术服务人员")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string OutSource { get; set; }

        [Display(Name = "不再跟踪")]
        public bool DoNotTrack { get; set; }
    }
}