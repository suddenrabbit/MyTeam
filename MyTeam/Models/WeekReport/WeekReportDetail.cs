using MyTeam.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    public class WeekReportDetail
    {
        [Key]
        public int WRDetailID { get; set; }

        [Required]
        [Display(Name = "周报日期")]
        [StringLength(16, MinimumLength = 14,ErrorMessage = "请填入正确的周报日期")]
        public string RptDate { get; set; }

        [Display(Name = "工作内容")]
        public string WorkMission { get; set; }          
       
        [Display(Name = "工作内容")] // 作废
        public string WorkDetail { get; set; }

        [Display(Name = "交付物")]
        public string WorkTarget { get; set; }

        [Required]
        [Display(Name = "负责人")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string Person { get; set; }      
       
        
        [Display(Name = "完成情况")]// 作废
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public string WorkStat { get; set; }

        [Required]
        [Display(Name = "工时（人小时）")]
        public double WorkTime { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }

        // 后台字段，表示该任务是否附属于“重点任务”
        public bool IsWithMain { get; set; }

        [Display(Name = "工作类型")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string WorkType { get; set; }

        [Display(Name = "现场技术服务人员")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string OutSource { get; set; }

        [Display(Name = "优先级")]
        [StringLength(2, ErrorMessage = "不能超过2位")]
        public string Priority { get; set; }

        [Display(Name = "完成率")]
        [Range(0, 100, ErrorMessage = "只能填0-100之间的数字")]
        public int Progress { get; set; }

        [Display(Name = "计划完成时间")]
        [DataType(DataType.Date)]
        public DateTime? PlanDeadLine { get; set; }

        [Display(Name = "任务名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string WorkName { get; set; }

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
                this.RptPersonName = value;
            }
        }
    }
}