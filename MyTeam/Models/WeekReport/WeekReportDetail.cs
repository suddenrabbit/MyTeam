using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class WeekReportDetail
    {
        [Key]
        public int WRDetailID { get; set; }

        [Required]
        [Display(Name = "周报日期")]
        public string RptDate { get; set; }
        
        [Required]
        [Display(Name = "工作任务")]
        public string WorkMission { get; set; }

        [Required]
        [Display(Name = "工作内容")]
        public string WorkDetail { get; set; }

        [Required]
        [Display(Name = "目标或交付物")]
        public string WorkTarget { get; set; }

        [Required]
        [Display(Name = "负责人")]
        public string Person { get; set; }       
       
        [Required]
        [Display(Name = "完成情况")]
        public string WorkStage { get; set; }

        [Required]
        [Display(Name = "占用工时（人小时）")]
        public double WorkTime { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }
    }
}