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
        [StringLength(16, ErrorMessage = "不能超过16位")]
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
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string Person { get; set; }       
       
        [Required]
        [Display(Name = "完成情况")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public string WorkStat { get; set; }

        [Required]
        [Display(Name = "占用工时（人小时）")]
        public double WorkTime { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }

        // 后台字段，表示该任务是否附属于“重点任务”
        public bool IsWithMain { get; set; }
    }
}