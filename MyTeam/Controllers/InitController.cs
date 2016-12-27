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
                UserType = 9,
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

        /*
        [AllowAnonymous]
        
        public string TestMail()
        {
            try
            {
                Domino.NotesSession nSession = new Domino.NotesSession();
                //string pwd = System.Configuration.ConfigurationManager.AppSettings["LotusMailPassword"];//lotus邮箱密码
                //string server = System.Configuration.ConfigurationManager.AppSettings["LotusMailServer"];//lotus邮箱服务器地址
                //string serverPath = System.Configuration.ConfigurationManager.AppSettings["LotusMailServerPath"];//存储nsf文件的路径
                //string saveMessageOnSend = System.Configuration.ConfigurationManager.AppSettings["SaveMessageOnSend"];//发送前是否保存

                string pwd = "wenshan1103";//lotus邮箱密码
                string server = "oasvr-fib";//lotus邮箱服务器地址
                string serverPath = "C:\\Users\\zm\\AppData\\Local\\IBM\\Notes\\Data\\names.nsf";//存储nsf文件的路径
                //string saveMessageOnSend = System.Configuration.ConfigurationManager.AppSettings["SaveMessageOnSend"];//发送前是否保存

                nSession.Initialize(pwd);//初始化邮件
                Domino.NotesDatabase nDatabase =
                nSession.GetDatabase(server, serverPath, false);
                Domino.NotesDocument nDocument = nDatabase.CreateDocument();
                nDocument.ReplaceItemValue("SentTo", "周梦 014690/FIB@FIB");//收件人，数据：数组
                nDocument.ReplaceItemValue("Subject", "TEST");//主题
               
                nDocument.SaveMessageOnSend = true;
               
                NotesStream HtmlBody = nSession.CreateStream();
                HtmlBody.WriteText("this is a test");//构建HTML邮件，可以在头和尾添加公司的logo和系统提醒语
                NotesMIMEEntity mine = nDocument.CreateMIMEEntity("Body");//构建邮件正文
                mine.SetContentFromText(HtmlBody, "text/html;charset=UTF-8", Domino.MIME_ENCODING.ENC_IDENTITY_BINARY);
                nDocument.AppendItemValue("Principal", "XXX管理员");//设置邮件的发件人昵称
                nDocument.Send(false, "周梦 014690/FIB@FIB"); //发送邮件
                nDocument.CloseMIMEEntities();//关闭
                return "success";//已经提交到lotus，返回true
            }
            catch(System.Exception e1)
            {
                return "failed: " + e1.Message;//提交失败
            }
        }
        */
    }
}
