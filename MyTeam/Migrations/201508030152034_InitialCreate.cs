namespace MyTeam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reqs",
                c => new
                    {
                        RID = c.Int(nullable: false, identity: true),
                        SysId = c.Int(nullable: false),
                        AcptDate = c.DateTime(nullable: false),
                        ReqNo = c.String(nullable: false),
                        ReqReason = c.String(nullable: false),
                        ReqFromDept = c.String(nullable: false),
                        ReqFromPerson = c.String(nullable: false),
                        ReqAcptPerson = c.String(nullable: false),
                        ReqDevPerson = c.String(nullable: false),
                        ReqBusiTestPerson = c.String(nullable: false),
                        DevAcptDate = c.DateTime(),
                        DevEvalDate = c.DateTime(),
                        ReqDetailNo = c.String(),
                        Version = c.String(),
                        BusiReqNo = c.String(),
                        ReqDesc = c.String(),
                        ReqType = c.String(),
                        DevWorkload = c.Int(),
                        ReqStat = c.String(),
                        OutDate = c.DateTime(),
                        PlanRlsDate = c.DateTime(),
                        RlsDate = c.DateTime(),
                        RlsNo = c.String(),
                        IsSysAsso = c.Boolean(nullable: false),
                        AssoSysName = c.String(),
                        AssoReqNo = c.String(),
                        AssoRlsDesc = c.String(),
                        Remark = c.String(),
                    })
                .PrimaryKey(t => t.RID);
            
            CreateTable(
                "dbo.RetailSystems",
                c => new
                    {
                        SysID = c.Int(nullable: false, identity: true),
                        SysNO = c.String(nullable: false),
                        SysName = c.String(nullable: false),
                        SysShortName = c.String(nullable: false),
                        HostDept = c.String(nullable: false),
                        SecondDept = c.String(),
                        BusiPerson = c.String(nullable: false),
                        DevCenter = c.String(nullable: false),
                        DevPerson = c.String(nullable: false),
                        ReqPerson = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.SysID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UID = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 20),
                        Password = c.String(nullable: false),
                        Realname = c.String(nullable: false, maxLength: 10),
                        Phone = c.String(nullable: false, maxLength: 6),
                        IsAdmin = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.RetailSystems");
            DropTable("dbo.Reqs");
        }
    }
}
