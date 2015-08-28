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
    public class BusiReqProjController : BaseController
    {
        //
        // GET: /BusiReqProj/

        public ActionResult Index(int pageNum = 1 )
        {
            List<BusiReqProj> projLs = dbContext.BusiReqProjs.ToList();
           
            // 分页
            var ls1 = projLs.ToPagedList(pageNum, Constants.PAGE_SIZE);
            return View(ls1);
        }

        //
        // GET: /BusiReqProj/Details/5

        public ActionResult Details(int id)
        {
            return RedirectToAction("Index", "BusiReq", new { isQuery = true, BRProjID = id });
        }

        //
        // GET: /BusiReqProj/Create

        public ActionResult Create()
        {
            // 需求分析师下拉列表
            SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname");
            
            ViewBag.UserList = sl;
            return View();
        }

        //
        // POST: /BusiReqProj/Create

        [HttpPost]
        public string Create(BusiReqProj brProj)
        {
            try
            {
                var r = dbContext.BusiReqProjs.ToList().Find(a => a.BRProjName == brProj.BRProjName);
                if(r != null){
                    return "<p class='alert alert-danger'>出错了: " + brProj.BRProjName + "已存在，不允许重复添加！" + "</p>";
                }

                dbContext.BusiReqProjs.Add(brProj);
                dbContext.SaveChanges();

                // 更新内存
                this.Update(3);
                
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // GET: /BusiReqProj/Edit/5

        public ActionResult Edit(int id)
        {
            BusiReqProj brProj = dbContext.BusiReqProjs.ToList().Find(a => a.BRProjID == id);

            if (brProj == null)
            {
                return View();
            }

            // 需求分析师下拉列表
            SelectList sl = new SelectList(this.GetUserList(), "UID", "Realname");
            ViewBag.UserList = sl;

            brProj.OldBRProjName = brProj.BRProjName;

            return View(brProj);
        }

        //
        // POST: /BusiReqProj/Edit/5

        [HttpPost]
        public string Edit(BusiReqProj brProj)
        {
            if (brProj.BRProjName != brProj.OldBRProjName)
            {
                // 若业需项目名称改变，则判断新改的业需项目名称是否有重复，如有重复不允许新增
                BusiReqProj p = dbContext.BusiReqProjs.Where(a => a.BRProjName == brProj.BRProjName).FirstOrDefault();
                if (p != null)
                {
                    return "<p class='alert alert-danger'>出错了: 业需项目名称" + p.BRProjName + "已存在，不允许更新！" + "</p>";
                }
            }

            try
            {
                dbContext.Entry(brProj).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        //
        // POST: /BusiReqProj/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                BusiReqProj brProj = dbContext.BusiReqProjs.ToList().Find(a => a.BRProjID == id);

                dbContext.Entry(brProj).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                // 更新内存
                this.Update(3);

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }
    }
}
