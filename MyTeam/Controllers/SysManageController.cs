using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using System.Text;
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
        public string Create(RetailSystem sys)
        {
            // 判断是否有重复的系统名称，如有重复不允许新增
            RetailSystem rs = this.GetSysList().Find(a => a.SysName == sys.SysName);
            if (rs != null)
            {
                return "<p class='alert alert-danger'>出错了: " + sys.SysName + "已存在，不允许重复添加！" + "</p>";
            }

            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.RetailSystems.Add(sys);
                    dbContext.SaveChanges();
                    // 更新内存
                    this.Update(2);
                }
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
                
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
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

            sys.OldSysName = sys.SysName;

            // 用户列表
            SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname", sys.ReqPersonID); // 选中当前值

            ViewBag.ReqPersonList = sl;

            return View(sys);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public string Edit(RetailSystem sys)
        {

            if(sys.SysName != sys.OldSysName)
            {
                // 若系统名称改变，则判断新改的系统名称是否有重复，如有重复不允许新增
                RetailSystem rs = this.GetSysList().Find(a => a.SysName == sys.SysName);
                if (rs != null)
                {
                    return "<p class='alert alert-danger'>出错了: " + sys.SysName + "已存在，不允许更新！" + "</p>";
                }
            }            

            try
            {
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                // 更新内存
                this.Update(2);

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
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
                this.Update(2);

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

    }
}
