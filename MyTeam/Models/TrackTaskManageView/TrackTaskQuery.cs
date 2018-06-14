using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{

    public class TrackTaskQuery
    {
        [Display(Name = "任务名称")]
        [StringLength(64, ErrorMessage = "任务名称不能超过64位")]
        public string TrackTaskName { get; set; }

        [Display(Name = "任务干系人")]
        public int TaskPersonID { get; set; }

        [Display(Name = "任务状态")]  // 1-进行中  2-已暂停 3-已完成
        public int TrackTaskStat { get; set; }

        public IPagedList<TrackTask> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&TrackTaskName=").Append(this.TrackTaskName)
                .Append("&TaskPersonID=").Append(this.TaskPersonID)
                .Append("&TrackTaskStat=").Append(this.TrackTaskStat)
                .Append("&isQuery=True").ToString();
        }
    }
}