using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{
    public class ProjPlanQuery
    {
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Display(Name = "创新计划年度")]
        public string PlanYear { get; set; }

        public IPagedList<ProjPlan> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&PlanYear=").Append(this.PlanYear)
                .Append("&isQuery=True").ToString();
        }
    }
}