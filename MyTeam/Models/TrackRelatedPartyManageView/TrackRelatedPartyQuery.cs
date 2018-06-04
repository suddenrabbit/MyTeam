using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{

    public class TrackRelatedPartyQuery
    {
        [Display(Name = "关联方名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RelatedPartyName { get; set; }

     
        [Display(Name = "关联方类型")] //1-总行系统 2-分行 3-集团子公司
        public int RelatedPartyType { get; set; }

        public IPagedList<TrackRelatedParty> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&RelatedPartyName=").Append(this.RelatedPartyName)
                .Append("&RelatedPartyType=").Append(this.RelatedPartyType)
                .Append("&isQuery=True").ToString();
        }
    }
}