using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class Proj
    {

        [Key]
        public int ProjID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ProjName { get; set; }

        [Display(Name = "项目编号")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public String ProjNo { get; set; }

        [Display(Name = "主办部门")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public String HostDept { get; set; }

        [Display(Name = "项目等级")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public String ProjLevel { get; set; }

        [Display(Name = "需求分析师")]
        public int? ReqAnalysisID { get; set; }

        [Display(Name = "业务人员")]
        public String BusiPerson { get; set; }

        [Display(Name = "项目经理")]
        public String ProjManager { get; set; }

        [Display(Name = "架构师")]
        public String Architect { get; set; }

        [Display(Name = "项目调研受理日期")]
        [DataType(DataType.Date)]
        public DateTime? ProAcptDate { get; set; }

        [Display(Name = "调研小组成立日期")]
        [DataType(DataType.Date)]
        public DateTime? SurveyGroupFoundDate { get; set; }

        [Display(Name = "项目调研完成日期")]
        [DataType(DataType.Date)]
        public DateTime? SurveyFinishDate { get; set; }

        [Display(Name = "项目调研备注")]
        public String SurveyRemark { get; set; }

        [Display(Name = "需求大纲编写人员")]
        public String OutlineWriter { get; set; }

        [Display(Name = "需求大纲编写开始日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlineStartDate { get; set; }

        [Display(Name = "需求大纲编写完成日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlineEndDate { get; set; }

        [Display(Name = "需求大纲审核人员")]
        public String OutlineAuditPerson { get; set; }

        [Display(Name = "需求大纲发布日期")]
        [DataType(DataType.Date)]
        public DateTime? OutlinePublishDate { get; set; }

        [Display(Name = "需求大纲备注")]
        public String OutlineRemark { get; set; }

        [Display(Name = "业需编写人员")]
        public String ReqWriter { get; set; }


        [Display(Name = "业需开发开始日期")]
        [DataType(DataType.Date)]
        public DateTime? ReqStartDate { get; set; }


        [Display(Name = "评审受理日期")]
        [DataType(DataType.Date)]
        public DateTime? ReviewAcptDate { get; set; }


        [Display(Name = "评审会召开日期")]
        [DataType(DataType.Date)]
        public DateTime? ReviewMeetingDate { get; set; }


        [Display(Name = "业需发布日期")]
        [DataType(DataType.Date)]
        public DateTime? ReqPublishDate { get; set; }


        [Display(Name = "业需备注")]
        public String ReqRemark { get; set; }

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

        [Display(Name = "章程发起日期")]
        [DataType(DataType.Date)]
        public DateTime? RulesStartDate { get; set; }

        [Display(Name = "章程发布日期")]
        [DataType(DataType.Date)]
        public DateTime? RulesPublishDate { get; set; }

        [Display(Name = "章程备注")]
        public String RulesRemark { get; set; }

        [Display(Name = "项目验收受理日期")]
        [DataType(DataType.Date)]
        public DateTime? ProjCheckAcptDate { get; set; }

        [Display(Name = "项目发布日期")]
        [DataType(DataType.Date)]
        public DateTime? ProjPublishDate { get; set; }

        [Display(Name = "验收结果")]
        public String CheckResult { get; set; }

        [Display(Name = "备注")]
        public String Remark { get; set; }

        [NotMapped]
        public string ReqAnalysisName
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.ReqAnalysisID
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.ReqAnalysisName = value; }
        }

        [NotMapped]
        public string OldProjName { get; set; }

        [Display(Name = "项目状态")]
        [StringLength(10)]
        public string ProjStat { get; set; }
    }
}