using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Controllers;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class HomeResult
    {
        public List<HomeReq> ReqLs { get; set; }

        public List<HomeProj> ProjLs { get; set; }
    }

    public class HomeReq
    {
        public int SysId { get; set; }

        public int ReqNum { get; set; }

        public string SysName
        {
            get
            {
                var r = (from a in Constants.SysList
                         where a.SysID == this.SysId
                         select a.SysName).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.SysName = value; }
        }
    }

    public class HomeProj
    {
        public int ProjID { get; set; }

        public string ProjName { get; set; }

        public string Progress { get; set; }        
    }
}