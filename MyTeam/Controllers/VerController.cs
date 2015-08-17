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
#if Release
    [Authorize]
#endif
    // 年度版本下发计划Controller
    public class VerController : BaseController
    {
        //
        // GET: /Ver/
       

        public ActionResult Index(int pageNum = 1)
        {
            // 分页
            List<Ver> ls1 = dbContext.Vers.ToList();
            var ls = ls1.ToPagedList(pageNum, Constants.PAGE_SIZE);
            return View(ls);
        }

        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {

            List<RetailSystem> ls1 = this.GetSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone");
            }

            ViewBag.ReqPersonList = sl2;

            return View();
        }

        //
        // POST: /SysManage/Create

        [HttpPost]
        public ActionResult Create(Ver ver)
        {
         
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Vers.Add(ver);
                    dbContext.SaveChanges();
                    
                }
                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }


        public ActionResult Edit(int id)
        {
            List<RetailSystem> ls1 = this.GetSysList();

            SelectList sl1 = new SelectList(ls1, "SysID", "SysName");

            ViewBag.SysList = sl1;

            SelectList sl2 = null;

            User user = this.GetSessionCurrentUser();
            if (user != null)
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone", user.UID);
            }
            else
            {
                sl2 = new SelectList(this.GetUserList(), "UID", "NamePhone");
            }

            ViewBag.ReqPersonList = sl2;

            List<Ver> ls = dbContext.Vers.ToList();
            Ver ver = ls.Find(a => a.VerID == id);

            if (ver == null)
            {
                ModelState.AddModelError("", "不存在该年度版本计划！");
                ver = new Ver();
            }
            return View(ver);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public ActionResult Edit(Ver ver)
        {
            
            try
            {
                dbContext.Entry(ver).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception e1)
            {
                ModelState.AddModelError("", "出错了: " + e1.Message);
                return View();
            }
        }

        // AJAX调用
        // POST: /SysManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            
            try
            {
                List<Ver> ls = dbContext.Vers.ToList();
                Ver ver = ls.Find(a => a.VerID == id);
                
                dbContext.Entry(ver).State = System.Data.Entity.EntityState.Deleted;
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
