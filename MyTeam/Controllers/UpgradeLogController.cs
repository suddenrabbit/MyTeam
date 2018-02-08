using MyTeam.Models;
using MyTeam.Utils;
using PagedList;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class UpgradeLogController : BaseController
    {
       
        public ActionResult Index(int pageNum = 1)
        {
            var ls = dbContext.UpgradeLogs.OrderByDescending(p => p.ReleaseDate);

            var result = ls.ToPagedList(pageNumber: pageNum, pageSize: Constants.PAGE_SIZE);

            ViewBag.IsAdminNow = IsAdminNow();

            return View(result);
        }

        public ActionResult Create()
        {
            UpgradeLog upgradeLog = new UpgradeLog
            {
                ReleaseDate = DateTime.Now
            };

            return View(upgradeLog);
        }

        [HttpPost]
        public string Create(UpgradeLog upgradeLog)
        {
            try
            {
                // 下发说明中的换行替换<br />
                var description = upgradeLog.Description;                
                upgradeLog.Description = description.Replace(Environment.NewLine, "<br />"); 

                dbContext.UpgradeLogs.Add(upgradeLog);
                dbContext.SaveChanges();
                return Constants.AJAX_CREATE_SUCCESS_RETURN;
            }
            catch (DbEntityValidationException dbEx)
            {
                var msg = string.Empty;
                var errors = (from u in dbEx.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                return "<p class='alert alert-danger'>校验出错: " + msg + "</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }
 
    }
       
}
