using MyTeam.Models;
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
                UserType = 0,
                IsAdmin = true
            };
            dbContext.Users.Add(u);
            dbContext.SaveChanges();
            this.Update(1);
            

            return "Init Completed. User 'Admin/123456' has been created.";
        }

        [AllowAnonymous]
        public string Upgarde(string version)
        {
            if (version != "1223")
                return "not this version";

            // 12.23 升级：Reqs表ReqType字段值域化
            var ls = dbContext.Reqs.ToList();
            var i = 0;
            var s = "";
            foreach(var req in ls)
            {
                if(string.IsNullOrEmpty(req.ReqDetailNo))
                {
                    continue;
                }
                try
                {
                    var reqTypeNum = req.ReqDetailNo.Split('-')[2];
                    var sql = "update Reqs set ReqType = '" + reqTypeNum + "' where RID=" + req.RID;
                    dbContext.Database.ExecuteSqlCommand(sql);
                    i++;
                }
                catch
                {
                    s += req.RID + ", ";
                }
                
            }

            return "updated: " + i + " records. failed records: " + s;
        }        
    }
}
