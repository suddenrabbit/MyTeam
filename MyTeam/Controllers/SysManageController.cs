using MyTeam.Models;
using MyTeam.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 系统管理Controller
    /// </summary>
#if Release
    [Authorize]
#endif
    public class SysManageController : BaseController
    {
        //
        // GET: /SysManage/

        public ActionResult Index()
        {            
            ViewBag.IsAdmin = this.IsAdminNow();

            return View(this.GetSysList());
        }

        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {
            SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname");

            ViewBag.ReqPersonList = sl;

            return View();
        }

        //
        // POST: /SysManage/Create

        [HttpPost]
        public ActionResult Create(RetailSystem sys)
        {            
            try
            {
                if (ModelState.IsValid)
                {

                    dbContext.RetailSystems.Add(sys);
                    dbContext.SaveChanges();
                    // 更新内存
                    this.Update();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        //
        // GET: /SysManage/Edit/5

        public ActionResult Edit(int id)
        {
            ViewBag.IsAdmin = this.IsAdminNow();

            RetailSystem sys = this.GetSysList().Find(a => a.SysID == id);

            if(sys==null)
            {
                ModelState.AddModelError("", "不存在该系统！");
                sys = new RetailSystem();
            }


            // 用户列表
            SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname", sys.ReqPersonID); // 选中当前值

            ViewBag.ReqPersonList = sl;

            return View(sys);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public ActionResult Edit(RetailSystem sys)
        {            
            try
            {
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                // 更新内存
                this.Update();

                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                // 为了正常显示页面，重新生成select list
                // 用户列表
                SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname", sys.ReqPersonID);
                return View(sys);
            }
        }

        // AJAX调用
        // POST: /SysManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {           
            try
            {
                RetailSystem sys = this.GetSysList().Find(a => a.SysID == id); 
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                // 更新内存
                this.Update();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

    }
}
