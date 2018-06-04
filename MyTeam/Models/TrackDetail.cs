using MyTeam.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    /// <summary>
    /// 跟踪任务详情
    /// </summary>
    public class TrackDetail
    {
        [Key]
        public int TrackDetailID { get; set; }
                
        [Display(Name = "任务名称")]        
        public int TrackTaskID { get; set; }

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

        [Display(Name = "改造情况")] 
        [StringLength(24,ErrorMessage = "最长24位")]// 需要改造、仅配合测试、无关联
        public string ChangeType { get; set; }

        [Display(Name = "关联情况")]
        [StringLength(24, ErrorMessage = "最长24位")]// 交易关联、数据关联、交易/数据关联
        public string RelatedType { get; set; }

        [Display(Name = "改造内容")]
        public string ChangeDesc { get; set; }

        [Display(Name = "可测试日期")]
        public DateTime? TestStartDate { get; set; }

        [Display(Name = "是否提交测试案例")]
        public bool HasSubmitedTestCase { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Display(Name = "记录创建日期")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "记录更新日期")]
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        [Display(Name = "任务名称")]
        public string TrackTaskName { get; set; }

        public virtual ICollection<TrackReq> TrackReqs { get; set; } //与TrackReq 1对多 的外键关系

    }
}