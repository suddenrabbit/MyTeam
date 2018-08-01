using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using MyTeam.Models;
using MyTeam.Enums;
using MyTeam.Utils;

namespace MyTeam.Controllers
{
    public class UserController : BaseController
    {

        // 显示登录页面
        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl)
        {          
            ViewBag.ReturnUrl = ReturnUrl;

            return View();
        }

        // 登录
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserLogin userLogin, string ReturnUrl)
        {
            string username = userLogin.Username;
            string password = userLogin.Password;
            try
            {
                // Password要MD5加密
                //password = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");
                password = FormsAuthenticationHelper.HashPasswordForStoringInConfigFile(password, "MD5");
                // 检测登录信息
                // 根据用户名、密码获取User信息
                // 2018年7月31日新增：支持NotesID或用户名登陆
                User user = this.GetUserList().Find(a => a.Username == username && a.Password == password);
                if (user != null)
                {
                    if(user.UserType == (int)UserTypeEnums.离职)
                    {
                        throw new Exception("已经离职的员工不能登录系统");
                    }
                    // 先用Session记录UID
                    //this.SetSessionCurrentUser(user.UID);

                    // 记录姓名，右上角显示
                    Session["Realname"] = user.Realname;

                    // 控制部分菜单显示，session记录是否为管理员
                    Session["IsAdmin"] = user.IsAdmin;

                    FormsAuthentication.RedirectFromLoginPage(user.UID.ToString(), userLogin.RememberMe);
                }
                else
                {
                    throw new Exception("用户名或密码错误");
                }
            }
            catch (Exception e1)
            {
                ViewBag.ErrMsg = "登录失败：" + e1.Message;
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

            //user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(changePsw.NewPsw, "MD5");
            user.Password = FormsAuthenticationHelper.HashPasswordForStoringInConfigFile(changePsw.NewPsw, "MD5");
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
            // 根据UserTypeEnums生成下拉框
            ViewBag.UserTypeList = MyTools.GetSelectListByEnum(enumType: typeof(UserTypeEnums));
            // 归属人员下拉
            ViewBag.BelongToList = new SelectList(this.GetFormalUserList(), "UID", "Realname");
            return View();
        }

        // 新增用户:ajax调用
        [HttpPost]
        public string Create(User user)
        {
            try
            {
                // Password要MD5加密
                // user.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(user.Password, "MD5");
                user.Password = FormsAuthenticationHelper.HashPasswordForStoringInConfigFile(user.Password, "MD5");
                // 检验是否已经存在该用户
                var ls = this.GetUserList().Where(a => a.Username == user.Username);
                if (ls.Count() > 0)
                {
                    return "<p class='alert alert-danger'>该用户名已存在，无法添加！</p>";
                }

                // 2018年7月31日新增：如果有NotesID，则不能重复
                if(!string.IsNullOrEmpty(user.NotesID))
                {
                    ls = this.GetUserList().Where(a => a.NotesID == user.NotesID);
                    if (ls.Count() > 0)
                    {
                        return "<p class='alert alert-danger'>该NotesID已存在，无法添加！</p>";
                    }
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
            
            // 为避免直接访问/Edit或传入的id不正确，默认id为当前登录用户
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

            // 根据UserTypeEnums生成下拉框
            ViewBag.UserTypeList = MyTools.GetSelectListByEnum(enumType: typeof(UserTypeEnums), forEdit: true, toEditValue: user.UserType.ToString());

            // 归属人员下拉
            ViewBag.BelongToList = new SelectList(this.GetFormalUserList(), "UID", "Realname", user.BelongTo);

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
            return "<p class='alert alert-success'>修改成功&nbsp;<a href='/User/Index'>返回</a></p>";
        }

    }
}
