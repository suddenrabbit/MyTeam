using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class BusiReqQuery
    {
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        public IPagedList<BusiReq> ResultList { get; set; }

    }
}