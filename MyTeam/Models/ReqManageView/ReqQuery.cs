using System.Text;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace MyTeam.Models
{
    /// <summary>
    /// 维护需求查询Model
    /// </summary>
    public class ReqQuery
    {

        [Display(Name = "系统名称")]
        public int SysID { get; set; }

        [Display(Name = "受理年度")]
        public string AcptYear { get; set; }

        [Display(Name = "受理月份")]
        public string AcptMonth { get; set; }

        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Display(Name = "需求编号")]
        public string ReqDetailNo { get; set; }

        [Display(Name = "下发通知编号")]
        public string AnyRlsNo { get; set; }

        [Display(Name = "需求状态")]
        public string ReqStat { get; set; }

        [Display(Name = "需求受理人")]
        public int ReqAcptPerson { get; set; }

        [Display(Name = "不等于")]
        public bool NotEqual { get; set; }

        // 特殊查询：0-无 1-超过3个月未出池 2-超过2周未入池
        [Display(Name="特殊查询")]
        public int SpecialQuery { get; set; }

        public IPagedList<Req> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&SysID=").Append(this.SysID)
                .Append("&AcptYear=").Append(this.AcptYear)
                .Append("&AcptMonth=").Append(this.AcptMonth)
                .Append("&ReqNo=").Append(this.ReqNo)
                .Append("&ReqDetailNo=").Append(this.ReqDetailNo)
                .Append("&AnyRlsNo=").Append(this.AnyRlsNo)
                .Append("&ReqStat=").Append(this.ReqStat)
                .Append("&ReqAcptPerson=").Append(this.ReqAcptPerson)
                .Append("&NotEqual=").Append(this.NotEqual)
                .Append("&isQuery=True").Append("&SpecialQuery=")
                .Append(this.SpecialQuery).ToString();
        }
    }
}
