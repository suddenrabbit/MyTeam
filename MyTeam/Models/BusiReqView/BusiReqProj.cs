using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class BusiReqProj
    {
        [Key]
        public int BRProjID { get; set; }

        public virtual ICollection<BusiReq> BusiReqs { get; set; }

        [Display(Name = "业需项目名称")]
        public string BRProjName { get; set; }

        [Display(Name = "需求分析师")]
        public int ReqAnalysisID { get; set; }

        [NotMapped]
        public string ReqAnalysisName
        {
            get
            {
                return Utils.MyTools.GetUserName(ReqAnalysisID);
            }
            set { this.ReqAnalysisName = value; }
        }

        [NotMapped]
        public string OldBRProjName { get; set; }
    }
}