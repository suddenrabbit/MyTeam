using MyTeam.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    public class ReqRelease
    {
        // 需求下发信息

        [Key]
        public int ReqReleaseID { get; set; }

        [Required]
        [Display(Name = "下发通知编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ReleaseNo { get; set; } // 唯一索引

        [Display(Name = "计划下发日期")]
        [DataType(DataType.Date)]
        public DateTime PlanReleaseDate { get; set; }

        [Display(Name = "实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "是否副下发")]
        [System.ComponentModel.DefaultValue(false)]
        public bool IsSideRelease { get; set; }

        [Display(Name = "起草人")]
        public int DraftPersonID { get; set; }

        /*[Display(Name = "对应主下发编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RelatedMainReleaseNo { get; set; }*/

        [Display(Name = "下发说明")]
        [StringLength(100, ErrorMessage = "不能超过100个字")]
        public string ReleaseDesc { get; set; }

        [NotMapped]
        public string OldReleaseNo { get; set; }

        [NotMapped]
        public string DraftPersonName
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.DraftPersonID
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.DraftPersonName = value; }
        }

    }
}