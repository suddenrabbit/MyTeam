using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MyTeam.Models
{
    // 统计
    public class OTSumUp
    {      
        [Display(Name = "加班人员")]
        public int PersonID { get; set; }

        [Display(Name = "年度总计加班小时数")]
        public double OTHours { get; set; }
        
        public string PersonName
        {
            get
            {
                return Utils.MyTools.GetUserName(PersonID);
            }
            set
            {
                this.PersonName = value;
            }
        } //用于显示UID对应的名字

    }
}