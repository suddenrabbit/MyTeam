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
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public String ReqNo { get; set; }


        [Display(Name = "业需名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public String ReqName { get; set; }


        [Display(Name = "优先级")]
        [StringLength(4, ErrorMessage = "不能超过4位")]
        public String Priority { get; set; }


        [Display(Name = "需求编写人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public String ReqWriter { get; set; }


        [Display(Name = "计划完成日期")]
        [DataType(DataType.Date)]
        public DateTime? PlanDeadLine { get; set; }


        [Display(Name = "实际完成日期")]
        [DataType(DataType.Date)]
        public DateTime? RealDeadLine { get; set; }


        [Display(Name = "变更标识")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public string ChangeChar { get; set; }


        [Display(Name = "变更批准人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public String ApprovePerson { get; set; }


        [Display(Name = "批准日期")]
        [DataType(DataType.Date)]
        public DateTime? ApproveDate { get; set; }


        [Display(Name = "软需编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public String SoftReqNo { get; set; }


        [Display(Name = "软需名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public String SoftReqName { get; set; }


        [Display(Name = "需求状态")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public String ReqStat { get; set; }

        [NotMapped]
        public string ProjName
        {
            get
            {
                // ProjID转name
                using (MyTeamContext dbContext = new MyTeamContext())
                {
                    var p = dbContext.Projs.ToList().Find(a => a.ProjID == this.ProjID);
                    return p == null ? "未知" : p.ProjName;
                }
            }

            set { this.ProjName = value; }
        }
    }
}