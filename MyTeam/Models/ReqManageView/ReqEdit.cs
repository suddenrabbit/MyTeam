using System.Collections.Generic;

namespace MyTeam.Models
{
    public class ReqEdit
    {
        public ReqMain reqMain { get; set; }
        public ReqDetail reqDetail { get; set; }
        public bool isUpdateMain { get; set; }
    }
}