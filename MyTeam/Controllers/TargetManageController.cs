using System;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 量化指标管理Controller
    /// </summary>
    public class TargetManageController : BaseController
    {
        //
        // GET: /TargetManage/

        public ActionResult Index(int pageNum = 1, int year = 0)
        {            
            // 按照年份显示
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            // 分页
            var ls = from t in dbContext.Targets 
                     select t;
            ls = ls.Where(t => t.TargetYear == year).OrderByDescending(t => t.TargetYear);
            var result = ls.ToPagedList(pageNum, Constants.PAGE_SIZE);

            ViewBag.year = year;

            return View(result);
        }

        //
        // GET: /TargetManage/Create

        public ActionResult Create()
        {

            return View();
        }

        //
        // POST: /TargetManage/Create

        [HttpPost]
        public string Create(Target tar)
        {
            try
            {
                // 量化目标、评分规则中的换行符
                string targetDesc = tar.TargetDesc.Replace(System.Environment.NewLine, "<br />");
                string targetRule = tar.TargetRule.Replace(System.Environment.NewLine, "<br />");

                tar.TargetDesc = targetDesc;
                tar.TargetRule = targetRule;

                dbContext.Targets.Add(tar);
                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;

            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        //
        // GET: /TargetManage/Edit/5

        public ActionResult Edit(int id)
        {
            Target tar = this.dbContext.Targets.FirstOrDefault(a => a.TID == id);

            if (tar == null)
            {
                return View();
            }

            return View(tar);
        }

        //
        // POST: /TargetManage/Edit/5

        [HttpPost]
        public string Edit(Target tar)
        {
            try
            {
                // 量化目标、评分规则中的换行符
                string targetDesc = tar.TargetDesc.Replace(System.Environment.NewLine, "<br />");
                string targetRule = tar.TargetRule.Replace(System.Environment.NewLine, "<br />");

                tar.TargetDesc = targetDesc;
                tar.TargetRule = targetRule;

                dbContext.Entry(tar).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /TargetManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                Target tar = this.dbContext.Targets.FirstOrDefault(a => a.TID == id);
                dbContext.Entry(tar).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

    }
}
