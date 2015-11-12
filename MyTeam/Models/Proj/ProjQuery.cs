using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

namespace MyTeam.Models
{
    public class ProjQuery
    {
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Display(Name = "项目调研受理日期")]
        public string ProAcptDateStart { get; set; } // from

        public string ProAcptDateEnd { get; set; } // to

        [Display(Name = "项目章程发布日期")]
        public string RulesPublishDateStart { get; set; } // from

        public string RulesPublishDateEnd { get; set; } // to

        public IPagedList<Proj> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&ProAcptDateStart=").Append(this.ProAcptDateStart)
                .Append("&ProAcptDateEnd=").Append(this.ProAcptDateEnd)
                .Append("&RulesPublishDateStart=").Append(this.RulesPublishDateStart)
                .Append("&RulesPublishDateEnd=").Append(this.RulesPublishDateEnd)
                .Append("&isQuery=True").ToString();
        }
        
    }
}