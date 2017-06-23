using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{

    public class UserNews
    {
        [Key]
        public int NewsID { get; set; }

        public int UID { get; set; }

        public int LogID { get; set; }

        public int NotifyStat { get; set; } // 1-待提示 2-已提示

    }

    
}