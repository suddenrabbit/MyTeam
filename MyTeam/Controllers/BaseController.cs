using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Models;
using MyTeam.Utils;

namespace MyTeam.Controllers
{
    public class BaseController : Controller
    {
        protected MyTeamContext dbContext = new MyTeamContext();

        
        protected void SetSessionCurrentUser(int UID)
        {
            Session["UID"] = UID;
        }

        protected User GetSessionCurrentUser()
        {
            User user = null;
            try
            {
                user = this.GetUserList().Find(a => a.UID == (int)Session["UID"]);
            }
            catch
            {
                // do nothing
            }
            return user;
        }

        // 判断当前用户是否为管理员
        protected bool IsAdminNow()
        {
            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                return user.IsAdmin;
            }
            else
            {
                return false;
            }
        }

        // 更新内存
        protected void Update()
        {
            Constants.UserList = dbContext.Users.ToList<User>();
            Constants.SysList = dbContext.RetailSystems.ToList<RetailSystem>();
        }

        protected List<User> GetUserList()
        {
            if (Constants.UserList == null)
            {
                Constants.UserList = dbContext.Users.ToList<User>();
            }
            return new List<User>(Constants.UserList.ToArray()); //生成一个新的list对象，防止改变内存中的静态值，下同
        }

        protected List<RetailSystem> GetSysList()
        {
            if (Constants.SysList == null)
            {
                Constants.SysList = dbContext.RetailSystems.ToList<RetailSystem>();
            }
            return new List<RetailSystem>(Constants.SysList.ToArray());
        }
    }   
}