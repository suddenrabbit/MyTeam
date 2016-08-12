using System.Text;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace MyTeam.Models
{
    /// <summary>
    /// 系统需求查询Model
    /// </summary>
    public class SysQuery
    {

        [Display(Name = "系统名称")]
        public string SysName { get; set; }

        [Display(Name = "需求受理人")]
        public int ReqPersonID { get; set; }

        [Display(Name = "系统状态")]
        public string SysStat { get; set; }      

        public IPagedList<RetailSystem> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&SysName=").Append(this.SysName)
                .Append("&ReqPersonID=").Append(this.ReqPersonID)
                .Append("&SysStat=").Append(this.SysStat)
                .Append("&isQuery=True").ToString();
        }
    }
}
