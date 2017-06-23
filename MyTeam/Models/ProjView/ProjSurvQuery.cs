using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{
    public class ProjSurvQuery
    {
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        public IPagedList<ProjSurv> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&isQuery=True").ToString();
        }
    }
}