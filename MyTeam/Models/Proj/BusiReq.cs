using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class BusiReq
    {

        [Key]
        public int BRID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Required]
        [Display(Name = "业需编号")]
        public string BusiReqNo { get; set; }

        [Required]
        [Display(Name = "业需名称")]
        public string BusiReqName { get; set; }

        [Required]
        [Display(Name = "功能描述")]
        public string Desc { get; set; }

        [Display(Name = "创建日期")]
        [DataType(DataType.Date)]
        public DateTime CreateDate { get; set; }

        [Display(Name = "需求来源及状态")]
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
                    var p = dbContext.Projs.ToList().Find(a => a.ProjID == this.ProjID);
                    return p == null ? "未知" : p.ProjName;
                }
            }

            set { this.ProjName = value; }
        }
    
    }
}