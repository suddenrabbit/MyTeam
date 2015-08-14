using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class ReqTrack
    {

        [Key]
        public int TrackID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Required]
        [Display(Name = "业需编号")]
        public String ReqNo { get; set; }


        [Display(Name = "业需名称")]
        public String ReqName { get; set; }


        [Display(Name = "优先级")]
        public String Priority { get; set; }


        [Display(Name = "需求编写人")]
        public String ReqWriter { get; set; }


        [Display(Name = "计划完成日期")]
        [DataType(DataType.Date)]
        public DateTime PlanDeadLine { get; set; }


        [Display(Name = "实际完成日期")]
        [DataType(DataType.Date)]
        public DateTime RealDeadLine { get; set; }


        [Display(Name = "变更标识")]
        public string ChangeChar { get; set; }


        [Display(Name = "变更批准人")]
        public String ApprovePerson { get; set; }


        [Display(Name = "批准日期")]
        [DataType(DataType.Date)]
        public DateTime ApproveDate { get; set; }


        [Display(Name = "软需编号")]
        public String SoftReqNo { get; set; }


        [Display(Name = "软需名称")]
        public String SoftReqName { get; set; }


        [Display(Name = "需求状态")]
        public String ReqStat { get; set; }

        [NotMapped]
        public string ProjName { get; set; }
    }
}