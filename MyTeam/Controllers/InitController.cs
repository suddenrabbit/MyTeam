using MyTeam.Models;
using System.Collections;
using System.Linq;
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
            if(ls.Count > 0)
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
            if(dbContext.ReqMains.Count() > 0)
            {
                return "ReqMains is not empty! ";
            }
            var ls = dbContext.Reqs.Where(p=>p.RID > 1341).ToList();                    

            int num = 0;

            // init ReqMains
            foreach (var r in ls)
            {
                var exist = (from a in dbContext.ReqMains where a.ReqNo == r.ReqNo select a).FirstOrDefault();
                if(exist == null)
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
                    dbContext.SaveChanges();
                    num++;
                }             

            }            

            return "Init " + num + "ReqMains!";
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
