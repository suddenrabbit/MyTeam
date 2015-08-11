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

        [Required]
        [Display(Name = "任务名称")]
        public string WorkName { get; set; }

        [Required]
        [Display(Name = "任务阶段")]
        public string WorkStage { get; set; }

        [Required]
        [Display(Name = "工作任务")]
        public string WorkMission { get; set; }

        [Required]
        [Display(Name = "负责人")]
        public string Person { get; set; }        

        [Required]
        [Display(Name = "目标或交付物")]
        public string WorkTarget { get; set; }

        [Required]
        [Display(Name = "计划完成日期")]
        [DataType(DataType.Date)]
        public DateTime? PlanDeadLine { get; set; }

        [Required]
        [Display(Name = "总体进度（%）")]
        [Range(0,100, ErrorMessage="只能填0-100之间的数字")]
        public double Progress { get; set; }

        [Display(Name = "工时（人小时）")]
        public double WorkTime { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }
    }
}