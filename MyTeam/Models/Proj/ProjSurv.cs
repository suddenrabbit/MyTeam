using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class ProjSurv
    {

        [Key]
        public int SurvID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Required]
        [Display(Name = "调研方式")]
        public String SurveyWay { get; set; }


        [Display(Name = "开始日期")]
        [DataType(DataType.Date)]
        public DateTime StartTime { get; set; }


        [Display(Name = "结束日期")]
        [DataType(DataType.Date)]
        public DateTime EndTime { get; set; }


        [Display(Name = "调研使用人天")]
        public String SurveyTime { get; set; }


        [Display(Name = "调研内容")]
        public String SurveyContent { get; set; }


        [Display(Name = "调研对象")]
        public String SurveyTarget { get; set; }


        [Display(Name = "交付物")]
        public String WorkTarget { get; set; }


        [Display(Name = "主办人员")]
        public String HostPerson { get; set; }


        [Display(Name = "协办人员")]
        public String JoinPerson { get; set; }


        [Display(Name = "调研满意度")]
        public String SatisfactionDegree { get; set; }


        [Display(Name = "备注")]
        public String Remark { get; set; }

        [NotMapped]
        public string ProjName { get; set; }
    }
}