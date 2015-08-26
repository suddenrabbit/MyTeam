using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

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
                var r = (from a in Constants.UserList
                         where a.UID == this.ReqAnalysisID
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.ReqAnalysisName = value; }
        }

        [NotMapped]
        public string OldBRProjName { get; set; }
    }
}