using System.ComponentModel.DataAnnotations;
using PagedList;

namespace MyTeam.Models
{
    public class BusiReqQuery
    {
        [Display(Name = "业需项目名称")]
        public int BRProjID { get; set; }

        public IPagedList<BusiReq> ResultList { get; set; }

    }
}