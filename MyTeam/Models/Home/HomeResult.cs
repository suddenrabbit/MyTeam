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
        public List<HomeReqDelay> ReqDelayLS {get; set;}

        public List<HomeReq> ReqLs { get; set; }

        public List<WeekReportDetail> Works { get; set; }

        public List<HomeProjDelay> ProjDetails { get; set; }

        public List<HomeInpoolReqDelay> ReqInpoolDelayLS { get; set; }
    }

    public class HomeReqDelay
    {
        public int SysId { get; set; }

        public int ReqDelayNum { get; set; }

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

    public class HomeProjDelay
    {
        public int ProjId { get; set; }

        public string DelayDetail { get; set; }

        public string ProjName
        {
            get
            {
                var r = (from a in Constants.ProjList
                         where a.ProjID == this.ProjId
                         select a.ProjName).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.ProjName = value; }
        }
    }

    public class HomeInpoolReqDelay
    {
        public int SysId { get; set; }

        public int ReqDelayNum { get; set; }

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
}