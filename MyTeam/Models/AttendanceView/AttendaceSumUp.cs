using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MyTeam.Models
{
    // 统计
    public class AttendanceSumUp
    {      
        [Display(Name = "请假人员")]
        public int PersonID { get; set; }

        [Display(Name = "年度总计请假天数")]
        public double LeaveDays { get; set; }
        
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