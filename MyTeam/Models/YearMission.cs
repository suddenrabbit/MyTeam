﻿using System.ComponentModel.DataAnnotations;

// 年度重点任务
namespace MyTeam.Models
{
    public class YearMission
    {
        [Key]
        public int YMID { get; set; }

        [Required]
        [Display(Name = "任务时间")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string MissionDate { get; set; }

        [Display(Name = "任务来源")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string MissionSource { get; set; }

        [Required]
        [Display(Name = "工作内容")]
        public string WorkMission { get; set; }

        [Display(Name = "工作进展")]
        [StringLength(1024, ErrorMessage = "不能超过1024位")]
        public string WorkStage { get; set; }

        [Required]
        [Display(Name = "负责人")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string Person { get; set; }

        [Display(Name = "现场技术服务人员")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string OutSource { get; set; }

        [Display(Name = "整体完成率")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string Progress { get; set; }

        [Display(Name = "计划完成时间")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        [MinLength(4, ErrorMessage="至少要填4位")]
        public string PlanDeadLine { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Display(Name = "不再跟踪")]
        public bool DoNotTrack { get; set; }
    }
}