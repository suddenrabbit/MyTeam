using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;
using MyTeam.Enums;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 外协人员请假管理Controller
    /// </summary>
    public class OTManageController : BaseController
    {
        //
        // GET: /OTManage/

        public ActionResult Index(int pageNum = 1)
        {           
            // 分页
            var ls = dbContext.OTs.OrderByDescending(a => a.OTID).ToPagedList(pageNum, Constants.PAGE_SIZE);

            // 权限控制：外协人员不可以编辑
            var user = GetSessionCurrentUser();
            ViewBag.CanEdit = user.UserType != (int)UserTypeEnums.外协;

            return View(ls);
        }

        //
        // GET: /OTManage/Create

        public ActionResult Create()
        {
            SelectList sl = new SelectList(this.GetUserList().Where(a => a.UserType == (int)UserTypeEnums.外协), "UID", "Realname");

            ViewBag.PersonList = sl;

            return View();
        }

        //
        // POST: /OTManage/Create

        [HttpPost]
        public string Create(OT ot)
        {
            try
            {
                // 若请假天数大于1，则批量处理
                var hours = ot.OTHours;
                
                dbContext.OTs.Add(ot);                    
                

                dbContext.SaveChanges();

                return Constants.AJAX_CREATE_SUCCESS_RETURN;

            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        //
        // GET: /OTManage/Edit/5

        public ActionResult Edit(int id)
        {
            OT ot = dbContext.OTs.ToList().Find(a => a.OTID == id);

            if (ot == null)
            {
                return View();
            }

            // 用户列表
            SelectList sl = new SelectList(this.GetUserList().Where(a => a.UserType == (int)UserTypeEnums.外协), "UID", "Realname", ot.PersonID); // 选中当前值

            ViewBag.PersonList = sl;

            return View(ot);
        }

        //
        // POST: /OTManage/Edit/5

        [HttpPost]
        public string Edit(OT ot)
        {
            try
            {
                dbContext.Entry(ot).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /OTManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                OT sys = dbContext.OTs.ToList().Find(a => a.OTID == id);
                dbContext.Entry(sys).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "删除成功";
            }
            catch (Exception e1)
            {
                return "出错了: " + e1.Message;
            }
        }

        // 导出excel
        [HttpGet]
        public ActionResult Export()
        {
            // 转换格式
            List<OTResult> r = new List<OTResult>();
            foreach(var a in dbContext.OTs.ToList<OT>() )
            {
                r.Add(new OTResult { OTDate = a.OTDate.ToString("yyyy/M/d"), PersonName = a.PersonName, OTHours = a.OTHours });
            }
            return this.MakeExcel<OTResult>("OTReportT", "外协人员加班统计表＿" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                r);
        }

        // 统计年度请假情况
        [HttpGet]
        public ActionResult SumUp()
        {
            var ls = dbContext.Database.SqlQuery<OTSumUp>("SELECT PersonID, sum(OTHours) as OTHours FROM OTs WHERE year(OTDate) = @p0  GROUP BY PersonID", DateTime.Now.Year).ToList();

            return View(ls);
        }
    }


}
