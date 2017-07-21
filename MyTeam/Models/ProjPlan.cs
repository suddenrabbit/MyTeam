using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class ProjPlan
    {
        [Key]
        public int PlanID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Display(Name = "业需联合开发小组成立时间")]
        [DataType(DataType.Date)]
        public DateTime? FoundGroupDate { get; set; }

        [Display(Name = "业务需求大纲编写开始日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlineStartDate { get; set; }

        [Display(Name = "业务需求大纲编写完成日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlineFinishDate { get; set; }

        [Display(Name = "业需开发开始日期")]
        [DataType(DataType.Date)]
        public DateTime? ReqStartDate { get; set; }

        [Display(Name = "业需开发完成日期")]
        [DataType(DataType.Date)]
        public DateTime? ReqFinishDate { get; set; }

        [Display(Name = "业需评审开始日期")]
        [DataType(DataType.Date)]
        public DateTime? ReviewStartDate { get; set; }

        [Display(Name = "业需评审结束日期")]
        [DataType(DataType.Date)]
        public DateTime? ReviewFinishDate { get; set; }

        [Display(Name = "业务可行性论证开始日期")]
        [DataType(DataType.Date)]
        public DateTime? BusiFeasiStartDate { get; set; }

        [Display(Name = "业务可行性论证结束日期")]
        [DataType(DataType.Date)]
        public DateTime? BusiFeasiFinishDate { get; set; }

        [Display(Name = "技术可行性论证开始日期")]
        [DataType(DataType.Date)]
        public DateTime? TechFeasiStartDate { get; set; }

        [Display(Name = "技术可行性论证结束日期")]
        [DataType(DataType.Date)]
        public DateTime? TechFeasiFinishDate { get; set; }

        [Display(Name = "技术可行性分析报告评审开始日期")]
        [DataType(DataType.Date)]
        public DateTime? TechFeasiReviewStartDate { get; set; }

        [Display(Name = "技术可行性分析报告评审结束日期")]
        [DataType(DataType.Date)]
        public DateTime? TechFeasiReviewFinishDate { get; set; }

        [Display(Name = "软件实施投入预算开始时间")]
        [DataType(DataType.Date)]
        public DateTime? SoftBudgetStartDate { get; set; }

        [Display(Name = "软件实施投入预算结束时间")]
        [DataType(DataType.Date)]
        public DateTime? SoftBudgetFinishDate { get; set; }

        [Display(Name = "实施方案开始时间")]
        [DataType(DataType.Date)]
        public DateTime? ImplementPlansStartDate { get; set; }

        [Display(Name = "实施方案结束时间")]
        [DataType(DataType.Date)]
        public DateTime? ImplementPlansFinishDate { get; set; }

        
        [Display(Name = "创新计划年度")]
        [MaxLength(4, ErrorMessage="请输入正确的年份信息（必须为4位数字）")]
        public string PlanYear { get; set; }

        [NotMapped]
        public string ProjName { get; set; }

        [NotMapped]
        public int OldProjID { get; set; }
    }
}