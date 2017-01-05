using MyTeam.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class UserNewsController : BaseController
    {
        //
        // GET: /UserNews/

        public ActionResult Index()
        {
            return View();
        }

        public string ReadNews(int newsID)
        {
            try
            {
                dbContext.Database.ExecuteSqlCommand("update UserNews set NotifyStat = 2 where NewsID=@p0", newsID);
                return Constants.AJAX_RESULT_SUCCESS;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

    }
}
