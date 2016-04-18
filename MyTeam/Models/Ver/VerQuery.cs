using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{
    public class VerQuery
    {
        [Display(Name = "系统名称")]
        public int SysId { get; set; }

        [Display(Name = "版本年度")]
        public string VerYear { get; set; }

        public IPagedList<Ver> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&SysId=").Append(this.SysId)
                .Append("&VerYear=").Append(this.VerYear)
                .Append("&isQuery=True").ToString();
        }
    }
}