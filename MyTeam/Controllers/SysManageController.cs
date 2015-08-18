using MyTeam.Models;
using MyTeam.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

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

        public ActionResult Index(int pageNum=1)
        {
            // 分页
            var ls = this.GetSysList().ToPagedList(pageNum, Constants.PAGE_SIZE);
            return View(ls);
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
        public ContentResult Create(RetailSystem sys)
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
                return Content("<p class='alert alert-success'>添加成功</p>");
                
            }
            catch (Exception e1)
            {
                return Content("<p class='alert alert-danger'>出错了: " + e1.Message + "</p>");
            }

        }

        //
        // GET: /SysManage/Edit/5

        public ActionResult Edit(int id)
        {
            RetailSystem sys = this.GetSysList().Find(a => a.SysID == id);

            if(sys==null)
            {
                return View();
            }

            // 用户列表
            SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname", sys.ReqPersonID); // 选中当前值

            ViewBag.ReqPersonList = sl;

            return View(sys);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public ContentResult Edit(RetailSystem sys)
        {            
            try
            {
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                // 更新内存
                this.Update();

                return Content("<p class='alert alert-success'>更新成功</p>");
            }
            catch (Exception e1)
            {
                return Content("<p class='alert alert-danger'>出错了: " + e1.Message + "</p>");
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
