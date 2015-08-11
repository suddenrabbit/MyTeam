using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyTeam.Models;

namespace MyTeam.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            User user = this.GetSessionCurrentUser();
            if(user == null)
            {
                return RedirectToAction("Login", "User");
            }
            ViewBag.Message = user.Realname;
            return View();
        }

    }
}
