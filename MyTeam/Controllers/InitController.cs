using MyTeam.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class InitController : BaseController
    {
        //
        // GET: /User/
        /// <summary>
        /// 初始化一个管理员用户
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public string InitUser()
        {
            var ls = this.GetUserList();
            if (ls.Count > 0)
            {
                return "System has Users, you cannot init it again.";
            }
            User u = new User()
            {
                Username = "Admin",
                Password = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile("123456", "MD5"),
                UserType = 9,
                Realname = "管理员",
                Phone = "000000",
                IsAdmin = true
            };
            dbContext.Users.Add(u);
            dbContext.SaveChanges();
            this.Update(1);


            return "Init Completed. User 'Admin/123456' has been created.";
        }


        [AllowAnonymous]
        public string InitReqs()
        {
            //Req拆表初始化
            if (dbContext.ReqMains.Count() > 0)
            {
                return "ReqMains is not empty! ";
            }

            var reqList = dbContext.Reqs.Where(p=>p.AcptDate.Value.Year > 2014).ToList();

            List<string> reqNoList = dbContext.Database.SqlQuery<string>("select distinct ReqNo from Reqs where ReqNo <> '暂无' and AcptDate BETWEEN '2014/12/31 23:59:59' AND '2017/12/31 23:59:59'").ToList();

            List<string> errList = new List<string>();

            foreach(var reqNo in reqNoList)
            {
                try
                {
                    var realList = reqList.Where(p => p.ReqNo == reqNo).ToList();
                    var first = realList.First();
                    var main = new ReqMain
                    {
                        ReqNo = reqNo,
                        SysID = first.SysID,
                        AcptDate = first.AcptDate,
                        ReqReason = first.ReqReason,
                        ReqFromDept = first.ReqFromDept,
                        ReqFromPerson = first.ReqFromPerson,
                        ReqAcptPerson = first.ReqAcptPerson,
                        ReqDevPerson = first.ReqDevPerson,
                        ReqBusiTestPerson = first.ReqBusiTestPerson,
                        DevAcptDate = first.DevAcptDate,
                        DevEvalDate = first.DevEvalDate
                    };

                    dbContext.ReqMains.Add(main);

                    foreach (var d in realList)
                    {
                        DateTime createTime;
                        DateTime updateTime;
                        try
                        {
                            createTime = d.CreateTime == null ? d.AcptDate.Value : DateTime.ParseExact(d.CreateTime, "yyyyMMddhhmmsss", System.Globalization.CultureInfo.CurrentCulture);
                            updateTime = d.UpdateTime == null ? d.AcptDate.Value : DateTime.ParseExact(d.UpdateTime, "yyyyMMddhhmmsss", System.Globalization.CultureInfo.CurrentCulture);
                        }
                        catch
                        {
                            createTime = d.AcptDate.Value;
                            updateTime = d.AcptDate.Value;
                        }
                            var detail = new ReqDetail
                        {
                            ReqDetailNo = d.ReqDetailNo,
                            Version = d.Version,
                            ReqDesc = d.ReqDesc,
                            ReqType = d.ReqType,
                            DevWorkload = d.DevWorkload,
                            ReqStat = d.ReqStat,
                            OutDate = d.OutDate,
                            IsSysAsso = d.IsSysAsso,
                            AssoSysName = d.AssoSysName,
                            AssoReleaseDesc = d.AssoRlsDesc,
                            AssoReqNo = d.AssoReqNo,
                            Remark = d.Remark,
                            ReqMain = main,
                            CreateTime = createTime,
                            UpdateTime = updateTime
                        };

                        dbContext.ReqDetails.Add(detail);
                    }
                }
                catch(Exception ee)
                {
                    errList.Add(reqNo + " " + ee.Message + "<br />");
                }
            }

            dbContext.SaveChanges();

            StringBuilder errMsg = new StringBuilder();

            for(int i=0; i< errList.Count; i++)
            {
                errMsg.Append(errList[i]);
            }

            return "done<br />" + errMsg.ToString();
            /*
            // init ReqRlses
            foreach (var r in ls)
            {
                var exist = (from a in dbContext.ReqRlses where a.RlsNo == r.RlsNo || a.RlsNo == r.SecondRlsNo select a).FirstOrDefault();
                if (exist == null)
                {
                    var reqMain = new ReqMain
                    {
                        SysID = r.SysID,
                        ReqNo = r.ReqNo,
                        AcptDate = r.AcptDate,
                        ReqReason = r.ReqReason,
                        ReqFromDept = r.ReqFromDept,
                        ReqFromPerson = r.ReqFromPerson,
                        ReqAcptPerson = r.ReqAcptPerson,
                        ReqDevPerson = r.ReqDevPerson,
                        ReqBusiTestPerson = r.ReqBusiTestPerson,
                        DevAcptDate = r.DevAcptDate,
                        DevEvalDate = r.DevEvalDate
                    };
                    dbContext.ReqMains.Add(reqMain);
                }

            }
            dbContext.SaveChanges(); */


        }

    }
}
