using System;
using System.Collections.Generic;
using System.Linq;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class HomeResult
    {

        public List<HomeReq> ReqLs { get; set; }

        public List<HomeReq> ReqDelayLS { get; set; }

        public List<HomeReq> ReqInpoolDelayLS { get; set; }


        public List<HomeProjDelay> ProjDetails { get; set; }

        public List<HomeRlsDelay> RlsDelayLS { get; set; }

        public List<HomeReq> ReqInpoolLS { get; set; }
    }

    public class HomeReq
    {
        public int SysId { get; set; }

        public int ReqNum { get; set; }

        public int ReqAcptPerson { get; set; }

        public string SysName
        {
            get
            {
                var r = (from a in Constants.SysList
                         where a.SysID == this.SysId
                         select a.SysShortName).FirstOrDefault();

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


    public class HomeRlsDelay
    {
        public string RlsNo { get; set; }
        public string SecondRlsNo { get; set; }
        public DateTime? PlanRlsDate { get; set; }
    }
}