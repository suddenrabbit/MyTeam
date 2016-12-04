using System;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;
using System.Linq;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 系统管理Controller
    /// </summary>
    public class SysManageController : BaseController
    {
        //
        // GET: /SysManage/

        public ActionResult Index(SysQuery sys, bool isQuery = false, int pageNum = 1)
        {
            if (isQuery)
            {
                // 按照查询条件筛选
                var ls = from a in dbContext.RetailSystems
                         select a;

                if (!string.IsNullOrEmpty(sys.SysName))
                {
                    ls = ls.Where(p => p.SysName.Contains(sys.SysName));
                }

                if (sys.ReqPersonID != 0)
                {
                    ls = ls.Where(p => p.ReqPersonID == sys.ReqPersonID);
                }

                if (!string.IsNullOrEmpty(sys.SysStat))
                {
                    ls = ls.Where(p => p.SysStat.ToString() == sys.SysStat);
                }
                sys.ResultList = ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE); 
            }
            else
            {
                sys = new SysQuery();
            }
                       

            // 需求受理用户列表
            var ls1 = GetUserList().Where(p => p.UserType == 1).ToList();

            ls1.Insert(0, new User() { UID = 0, Realname = "全部" });

            SelectList sl = new SelectList(ls1, "UID", "Realname", sys.ReqPersonID); // 选中当前值

            ViewBag.ReqPersonList = sl;

            return View(sys);
        }

        //
        // GET: /SysManage/Create

        public ActionResult Create()
        {
            // 需求受理用户列表
            var ls1 = GetUserList().Where(p => p.UserType == 1).ToList();

            SelectList sl = new SelectList(ls1, "UID", "Realname"); // 选中当前值

            ViewBag.ReqPersonList = sl;

            // 需求编辑用户列表
            var ls2 = GetUserList().Where(p => p.UserType == 2).ToList();

            ls2.Insert(0, new User() { UID = 0, Realname = "暂无" });

            SelectList s2 = new SelectList(ls2, "UID", "Realname"); // 选中当前值

            ViewBag.ReqEditPersonList = s2;

            return View();
        }

        //
        // POST: /SysManage/Create

        [HttpPost]
        public string Create(RetailSystem sys)
        {
            // 判断是否有重复的系统名称，如有重复不允许新增
            RetailSystem rs = GetSysList().Find(a => a.SysName == sys.SysName);
            if (rs != null)
            {
                return "<p class='alert alert-danger'>出错了: " + sys.SysName + "已存在，不允许重复添加！" + "</p>";
            }

            try
            {
                dbContext.RetailSystems.Add(sys);
                dbContext.SaveChanges();
                // 更新内存
                Update(2);

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
            RetailSystem sys = GetSysList().Find(a => a.SysID == id);

            if (sys == null)
            {
                return View();
            }

            sys.OldSysName = sys.SysName;

            // 需求受理用户列表
            var ls1 = GetUserList().Where(p => p.UserType == 1).ToList();

            SelectList sl = new SelectList(ls1, "UID", "Realname", sys.ReqPersonID); // 选中当前值

            ViewBag.ReqPersonList = sl;

            // 需求编辑用户列表
            var ls2 = GetUserList().Where(p => p.UserType == 2).ToList();

            ls2.Insert(0, new User() { UID = 0, Realname = "暂无" });

            SelectList s2 = new SelectList(ls2, "UID", "Realname", sys.ReqEditPersonID); // 选中当前值

            ViewBag.ReqEditPersonList = s2;

            return View(sys);
        }

        //
        // POST: /SysManage/Edit/5

        [HttpPost]
        public string Edit(RetailSystem sys)
        {

            if (sys.SysName != sys.OldSysName)
            {
                // 若系统名称改变，则判断新改的系统名称是否有重复，如有重复不允许新增
                RetailSystem rs = GetSysList().Find(a => a.SysName == sys.SysName);
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
                Update(2);

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
                RetailSystem sys = GetSysList().Find(a => a.SysID == id);
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                // 更新内存
                Update(2);

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

    }
}
