using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{
    public class ProjMeetingQuery
    {
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Display(Name="会议类型")]
        public string MeetingType { get; set; }

        [Display(Name = "查询时间")]
        public string MeetingDateStart { get; set; }

        [Display(Name = "查询时间")]
        public string MeetingDateEnd { get; set; }

        public IPagedList<ProjMeeting> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&MeetingType=").Append(this.MeetingType)
                .Append("&MeetingDateStart=").Append(this.MeetingDateStart)
                .Append("&MeetingDateEnd=").Append(this.MeetingDateEnd)
                .Append("&isQuery=True").ToString();
        }
    }
}