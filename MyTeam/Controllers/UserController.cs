using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyTeam.Models;

namespace MyTeam.Controllers
{
#if Release
    [Authorize]
#endif
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
            // Password要MD5加密
            password = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");
            // 检测登陆信息
            // 根据用户名、密码获取User信息
            User user = this.GetUserList().Find(a => a.Username == username && a.Password == password);
            if (user != null)
            {
                // 先用Session记录UID
                this.SetSessionCurrentUser(user.UID);
                FormsAuthentication.RedirectFromLoginPage(username, false);
                // return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "您输入的用户名或密码错误");
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

        // 修改密码
        [HttpPost]
        public ActionResult ChangePsw(ChangePsw changePsw)
        {

            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/User/ChangePsw" });
            }

            user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(changePsw.NewPsw, "MD5");
            dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
            dbContext.SaveChanges();
            // 提示更新成功
            ViewBag.Message = "您已成功修改密码";
            // 更新内存
            this.Update();

            return View();
        }

        // 管理页面
        public ActionResult Index()
        {
            // 只允许管理员
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/User/Index" });
            }

            if (!user.IsAdmin)
            {

                ModelState.AddModelError("", "您没有权限管理用户");

                return View();
            }

            return View(this.GetUserList());
        }

        // 新增用户页面
        public ActionResult Create()
        {

            return View();
        }

        // 新增用户
        [HttpPost]
        public ActionResult Create(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Password要MD5加密
                    user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(user.Password, "MD5");
                    // 检验是否已经存在该客户
                    var ls = this.GetUserList().Where(a => a.Username == user.Username);
                    if (ls.Count() > 0)
                    {
                        ModelState.AddModelError("", "该用户名已存在，无法添加");
                        return View();
                    }
                    else
                    {
                        dbContext.Users.Add(user);
                        dbContext.SaveChanges();
                        // 更新内存
                        this.Update();
                    }

                }
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View(user);
            }

        }

        // 编辑用户页面：普通用户只允许修改真实姓名与电话号码，管理员可以修改用户的属性（是否管理员）
        public ActionResult Edit(string id)
        {
            User user = this.GetSessionCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User", new { ReturnUrl = "/User/Edit/id" });
            }

            // 为避免直接访问/Edit或传入的id不正确，默认id为当前登陆用户

            // 传递是否为管理员到页面，控制是否显示“是否管理员”这个选项
            ViewBag.IsAdmin = user.IsAdmin;

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

            return View(user);
        }

        // 更新用户信息
        [HttpPost]
        public ActionResult Edit(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                    // 更新内存
                    this.Update();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View(user);
            }
        }

    }
}
