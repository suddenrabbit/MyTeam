using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    /// <summary>
    /// 跟踪关联方
    /// </summary>
    public class TrackRelatedParty
    {
        [Key]
        public int RelatedPartyID { get; set; }

        [Required]
        [Display(Name = "关联方名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RelatedPartyName { get; set; }

        [Display(Name = "业务联系人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string BusiPerson { get; set; }

        [Display(Name = "业务联系人电话")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string BusiPersonPhone { get; set; }

        [Display(Name = "研发联系人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string DevPerson { get; set; }

        [Display(Name = "研发联系人电话")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string DevPersonPhone { get; set; }

        [Display(Name = "需求联系人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string ReqPerson { get; set; }

        [Display(Name = "需求联系人电话")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string ReqPersonPhone { get; set; }

        [Display(Name = "关联方类型")] //1-总行系统 2-分行 3-集团子公司
        public int RelatedPartyType { get; set; }

        [NotMapped]
        public string RelatedPartyTypeName
        {
            get
            {
                switch (RelatedPartyType)
                {
                    case 1:
                        return "总行系统";
                    case 2:
                        return "分行";
                    case 3:
                        return "集团子公司";
                    default:
                        return "未知(" + RelatedPartyType + ")";
                }

            }
            set
            {
                RelatedPartyTypeName = value;
            }
        }

    }
}