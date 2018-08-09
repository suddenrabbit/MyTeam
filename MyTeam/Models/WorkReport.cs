using MyTeam.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    public class WorkReport
    {
        [Key]
        public int WorkReportID { get; set; }

        [Required]
        [Display(Name = "填报周期")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RptDate { get; set; }

        [Display(Name = "工作类型")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string WorkType { get; set; }

        [Display(Name = "工作任务/项目名称")]
        public string WorkMission { get; set; }          
       
        [Display(Name = "工作内容")] 
        public string WorkDetail { get; set; }

        [Required]
        [Display(Name = "负责人")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string Person { get; set; }

        [Display(Name = "现场人员")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string OutSource { get; set; }

        [Display(Name = "工作进展")]
        [StringLength(128, ErrorMessage = "不能超过128位")]
        public string WorkStage { get; set; }

        [Display(Name = "整体完成率")]
        [StringLength(128, ErrorMessage = "不能超过128位")]
        public string Progress { get; set; }
        
        [Display(Name = "本周完成")]
        public string WorkOfThisWeek { get; set; }

        [Display(Name = "本周交付")]
        public string DeliveryOfThisWeek { get; set; }

        [Display(Name = "下周计划")]
        public string WorkOfNextWeek { get; set; }

        [Display(Name = "下周交付")]
        public string DeliveryOfNextWeek { get; set; }

        [Display(Name = "计划完成时间")]
        [DataType(DataType.Date)]
        public DateTime? PlanDeadLine { get; set; }

        [Display(Name = "工时（人小时）")]
        public double WorkTime { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }

        [Required]
        [Display(Name = "重点工作")]
        public bool IsMain { get; set; }

        [NotMapped]
        public string RptPersonName
        {
            get
            {
                var s = (from a in Constants.UserList
                         where a.UID == this.RptPersonID
                         select a.Realname).FirstOrDefault();
                return s == null ? "未知" : s.ToString();
            }
            set
            {
                RptPersonName = value;
            }
        }        

    }
}