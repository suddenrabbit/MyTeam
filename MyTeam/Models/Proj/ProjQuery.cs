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
        public string ProAcptDate { get; set; }

        [Display(Name = "项目章程发布日期")]
        public string RulesPublishDate { get; set; }

        public IPagedList<Proj> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&ProAcptDate=").Append(this.ProAcptDate)
                .Append("&RulesPublishDate=").Append(this.RulesPublishDate)
                .Append("&isQuery=True").ToString();
        }
        
    }
}