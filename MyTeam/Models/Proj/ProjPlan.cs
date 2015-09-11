using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class ProjPlan
    {
        [Key]
        public int PlanID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Required]
        [Display(Name = "项目调研开始日期")]
        [DataType(DataType.Date)]
        public DateTime? SurveyStartDate { get; set; }

        [Required]
        [Display(Name = "项目调研完成日期")]
        [DataType(DataType.Date)]
        public DateTime? SurveyFinishDate { get; set; }

        [Required]
        [Display(Name = "需求大纲编写开始日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlineStartDate { get; set; }

        [Required]
        [Display(Name = "需求大纲编写完成日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlineFinishDate { get; set; }

        [Required]
        [Display(Name = "业需开发开始日期")]
        [DataType(DataType.Date)]
        public DateTime? ReqStartDate { get; set; }

        [Required]
        [Display(Name = "业需完成日期")]
        [DataType(DataType.Date)]
        public DateTime? ReqFinishDate { get; set; }

        [Required]
        [Display(Name = "业需评审开始日期")]
        [DataType(DataType.Date)]
        public DateTime? ReviewStartDate { get; set; }

        [Required]
        [Display(Name = "业需评审结束日期")]
        [DataType(DataType.Date)]
        public DateTime? ReviewFinishDate { get; set; }

        [Required]
        [Display(Name = "章程开始日期")]
        [DataType(DataType.Date)]
        public DateTime? RulesStartDate { get; set; }

        [Required]
        [Display(Name = "章程结束日期")]
        [DataType(DataType.Date)]
        public DateTime? RulesFinishDate { get; set; }

        [NotMapped]
        public string ProjName { get; set; }

        [NotMapped]
        public int OldProjID { get; set; }
    }
}