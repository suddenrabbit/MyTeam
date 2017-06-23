using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class BusiReq
    {
        [Key]
        public int BRID { get; set; }

        [Required]
        [Display(Name = "业需项目名称")]
        public int BRProjID { get; set; }

        [ForeignKey("BRProjID")]
        public virtual BusiReqProj BusiReqProj { get; set; }

        [Required]
        [Display(Name = "业需编号")]
        [StringLength(32, ErrorMessage = "业需编号不能超过32位")]
        public string BusiReqNo { get; set; }

        [Required]
        [Display(Name = "业需名称")]
        [StringLength(32, ErrorMessage = "业需名称不能超过32位")]
        public string BusiReqName { get; set; }

        [Required]
        [Display(Name = "功能描述")]
        public string Desc { get; set; }

        [Display(Name = "创建日期")]
        [DataType(DataType.Date)]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "需求来源及状态")]
        [StringLength(16)]
        public string Stat { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [NotMapped]
        public string ProjName
        {
            get
            {
                // ProjID转name
                using (MyTeamContext dbContext = new MyTeamContext())
                {
                    var p = dbContext.BusiReqProjs.ToList().Find(a => a.BRProjID == this.BRProjID);
                    return p == null ? "未知" : p.BRProjName;
                }
            }

            set { this.ProjName = value; }
        }
    
    }
}