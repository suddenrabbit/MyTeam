using System;
using System.Collections.Generic;
using System.Linq;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class HomeResult
    {

        public List<HomeReq> ReqLS { get; set; }

        public List<HomeReq> ReqDelayLS { get; set; }

        public List<HomeReq> ReqInpoolDelayLS { get; set; }


        public List<HomeProjDelay> ProjDetails { get; set; }

        public List<ReqRelease> RlsDelayLS { get; set; }

        public List<HomeReq> ReqInpoolLS { get; set; }

        public int ReqLsSum { get; internal set; }
        public int ReqDelayLsSum { get; internal set; }
        public int ReqInpoolLsSum { get; internal set; }
        public int ReqInpoolDelayLsSum { get; internal set; }

        public List<HomeNoRlsNo> NoRlsNoLS { get; internal set; }

        public UpgradeLog NewsLog { get; internal set; }

        public int UID { get; set; }
    }

    public class HomeReq
    {
        public int SysID { get; set; }

        public int ReqNum { get; set; }

        public int ReqAcptPerson { get; set; }

        public string SysName
        {
            get
            {
                var r = (from a in Constants.SysList
                         where a.SysID == this.SysID
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


    public class HomeReleaseDelay
    {
        public string ReleaseNo { get; set; }
        public DateTime PlanReleaseDate { get; set; }
        //public bool IsSideRelease { get; set; }
        public string ReleaseDesc { get; set; }
    }

    public class HomeNoRlsNo
    {
        public int SysID { get; set; }

        public string Version { get; set; }

        public string SysName
        {
            get
            {
                var r = (from a in Constants.SysList
                         where a.SysID == this.SysID
                         select a.SysShortName).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.SysName = value; }
        }
    }
}