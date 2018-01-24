namespace MyTeam.Models
{
    using System.Data.Entity;

    public class MyTeamContext : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“MyTeam”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“MyTeam.Models.MyTeam”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“MyTeam”
        //连接字符串。
        public MyTeamContext()
            : base("name=MyTeam")
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }

        public DbSet<User> Users {get; set;}

        public DbSet<Req> Reqs { get; set; }

        public DbSet<RetailSystem> RetailSystems { get; set; }

        public DbSet<WeekReportMain> WeekReportMains { get; set; }
        public DbSet<WeekReportDetail> WeekReportDetails { get; set; }
        public DbSet<WeekReportRisk> WeekReportRisks { get; set; }

        public DbSet<Ver> Vers { get; set; }
        public DbSet<Proj> Projs { get; set; }

        public DbSet<ProjSurv> ProjSurvs { get; set; }

        public DbSet<ProjMeeting> ProjMeetings { get; set; }

        public DbSet<ReqTrack> ReqTracks { get; set; }

        public DbSet<BusiReq> BusiReqs { get; set; }

        public DbSet<BusiReqProj> BusiReqProjs { get; set; }

        public DbSet<ProjPlan> ProjPlans { get; set; }

        public DbSet<Target> Targets { get; set; }

        public DbSet<TargetMission> TargetMissions { get; set; }

        public DbSet<YearMission> YearMissions { get; set; }

        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<UpgradeLog> UpgradeLogs { get; set; }

        public DbSet<UserNews> UserNews { get; set; }

        public DbSet<Param> Params { get; set; }

        public DbSet<ReqMain> ReqMains { get; set; }
        public DbSet<ReqDetail> ReqDetails { get; set; }
        public DbSet<ReqRelease> ReqReleases { get; set; }

        public DbSet<Leave> Leaves { get; set; }
        public DbSet<OT> OTs { get; set; }

        public DbSet<MyFile> MyFiles { get; set; }
    }
   
}