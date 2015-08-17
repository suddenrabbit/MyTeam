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
#if RELEASE
    [Authorize]
#endif
    public class ProjController : BaseController
    {
        //
        // GET: /Proj/

        public ActionResult Index(int pageNum = 1)
        {
            // 分页
            List<Proj> ls = dbContext.Projs.ToList();
            var ls1 = ls.ToPagedList(pageNum, Constants.PAGE_SIZE);
            return View(ls1);
        } 

        // GET: /Proj/Details/5

        public ActionResult Details(int id)
        {
            
            List<Proj> ls = dbContext.Projs.ToList();
            Proj proj = ls.Find(a => a.ProjID == id);

            if (proj == null)
            {
                ModelState.AddModelError("", "不存在该年度版本计划！");
                proj = new Proj();
            }
            return View(proj);
        }

        //
        // GET: /Proj/Create

        public ActionResult Create()
        {
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

            ViewBag.ReqAnalysisList = sl2;

            // 主办部门
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            return View();
        }

        //
        // POST: /Proj/Create

        [HttpPost]
        public ActionResult Create(Proj proj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    dbContext.Projs.Add(proj);
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

        //
        // GET: /Proj/Edit/5

        public ActionResult Edit(int id)
        {
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

            ViewBag.ReqAnalysisList = sl2;

            // 主办部门
            ViewBag.ReqFromDeptList = MyTools.GetSelectList(Constants.ReqFromDeptList);

            List<Proj> ls = dbContext.Projs.ToList();
            Proj proj = ls.Find(a => a.ProjID == id);

            if (proj == null)
            {
                ModelState.AddModelError("", "不存在该年度版本计划！");
            }
            return View(proj);
        }

        //
        // POST: /Proj/Edit/5

        [HttpPost]
        public ActionResult Edit(Proj proj)
        {
            
            try
            {
                dbContext.Entry(proj).State = System.Data.Entity.EntityState.Modified;
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
                List<Proj> ls = dbContext.Projs.ToList();
                Proj proj = ls.Find(a => a.ProjID == id);

                dbContext.Entry(proj).State = System.Data.Entity.EntityState.Deleted;
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
