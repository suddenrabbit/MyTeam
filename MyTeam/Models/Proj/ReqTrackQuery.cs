using System.ComponentModel.DataAnnotations;
using PagedList;
using System.Text;

namespace MyTeam.Models
{
    public class ReqTrackQuery
    {
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        public IPagedList<ReqTrack> ResultList { get; set; }


        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&isQuery=True").ToString();
        }
    }
}