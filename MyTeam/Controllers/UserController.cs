using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using MyTeam.Models;

namespace MyTeam.Controllers
{
    public class UserController : BaseController
    {

        // 显示会员登陆页面
        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // 会员登陆
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserLogin userLogin, string ReturnUrl)
        {
            string username = userLogin.Username;
            string password = userLogin.Password;
            try
            {
                // Password要MD5加密
                password = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");
                // 检测登陆信息
                // 根据用户名、密码获取User信息
                User user = this.GetUserList().Find(a => a.Username == username && a.Password == password);
                if (user != null)
                {
                    // 先用Session记录UID
                    this.SetSessionCurrentUser(user.UID);
                    // 控制部分菜单显示，session记录是否为管理员
                    Session["IsAdmin"] = user.IsAdmin;
                    FormsAuthentication.RedirectFromLoginPage(user.Realname, false);
                    // return RedirectToAction("Index", "Home");
                }
                else
                {
                    throw new Exception("用户名或密码错误");
                }
            }
            catch (Exception e1)
            {
                ViewBag.ErrMsg = "登陆失败：" + e1.Message;
            }            

            return View();

        }

        // 注销
        public ActionResult Logout()
        {
            // 清除Cookies
            FormsAuthentication.SignOut();

            // 清除Session
            Session.Clear();

            return RedirectToAction("Login", "User");
        }

        // 普通用户修改自己的密码页面       
        public ActionResult ChangePsw()
        {           
            return View();
        }

        // 修改密码:ajax调用
        [HttpPost]
        public string ChangePsw(ChangePsw changePsw)
        {
            User user = this.GetSessionCurrentUser();

            user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(changePsw.NewPsw, "MD5");
            dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
            dbContext.SaveChanges();

            // 更新内存
            this.Update(1);

            return "<p class='alert alert-success'>您已成功修改密码</p>";
        }

        // 管理页面
        public ActionResult Index()
        {
            // 只允许管理员           
            if (!this.IsAdminNow())
            {

                ViewBag.ErrMsg = "您没有权限管理用户";

                return View();
            }

            return View(this.GetUserList());
        }

        // 新增用户页面
        public ActionResult Create()
        {
            return View();
        }

        // 新增用户:ajax调用
        [HttpPost]
        public string Create(User user)
        {
            try
            {
                // Password要MD5加密
                user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(user.Password, "MD5");
                // 检验是否已经存在该用户
                var ls = this.GetUserList().Where(a => a.Username == user.Username);
                if (ls.Count() > 0)
                {
                    return "<p class='alert alert-danger'>该用户名已存在，无法添加！</p>";
                }
                
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                // 更新内存
                this.Update(1);

                return "<p class='alert alert-success'>新增用户成功&nbsp;<a href='/User/Index'>返回</a></p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        // 编辑用户页面：普通用户只允许修改真实姓名与电话号码，管理员可以修改用户的属性（是否管理员）
        public ActionResult Edit(string id)
        {
            User user = this.GetSessionCurrentUser();
            
            // 为避免直接访问/Edit或传入的id不正确，默认id为当前登陆用户
            int uid;
            try
            {
                uid = int.Parse(id);
            }
            catch
            {
                uid = user.UID;
            }

            // 若是管理员，则允许id与当前用户不一致，否则只能改自己
            if (!user.IsAdmin && uid != user.UID)
            {
                ViewBag.ErrMsg = "非管理员只能修改自己的信息！";
            }
            else
            {
                user = this.GetUserList().Find(a => a.UID == uid);
            }

            if (user == null)
            {
                ViewBag.ErrMsg = "无此用户！";
            }

            return View(user);
        }

        // 更新用户信息
        [HttpPost]
        public string Edit(User user)
        {
            try
            {
                dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                // 更新内存
                this.Update(1);

            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
            return "<p class='alert alert-success'>修改成功</p>";
        }

    }
}
