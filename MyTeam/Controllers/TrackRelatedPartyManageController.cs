using MyTeam.Models;
using MyTeam.Utils;
using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class TrackRelatedPartyManageController : BaseController
    {
        // GET: TrackRelatedPartyManage
        public ActionResult Index(TrackRelatedPartyQuery query, bool isQuery = false, int pageNum = 1)
        {
            if (isQuery)
            {
                var ls = from a in dbContext.TrackRelatedParties
                         select a;

                if (!string.IsNullOrEmpty(query.RelatedPartyName))
                {
                    ls = ls.Where(p => p.RelatedPartyName.Contains(query.RelatedPartyName));
                }

                if (query.RelatedPartyType != 0)
                {
                    ls = ls.Where(p => p.RelatedPartyType == query.RelatedPartyType);
                }

                query.ResultList = ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE);
            }
            else
            {
                query = new TrackRelatedPartyQuery { RelatedPartyType = 0 };
            }
            return View(query);
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public string Create(TrackRelatedParty model)
        {
            // 判断有无名称重复的 （暂时不做）
            /*var check = dbContext.TrackRelatedParties.Where(p => p.RelatedPartyName == model.RelatedPartyName).FirstOrDefault();
            if(check != null)
            {
                return "<p class='alert alert-danger'>" + model.RelatedPartyName + "已存在，请勿重复添加！" + "</p>";
            }
            */
            try
            {
                dbContext.TrackRelatedParties.Add(model);
                dbContext.SaveChanges();
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

            return Constants.AJAX_CREATE_SUCCESS_RETURN;
        }

        public ActionResult Edit(int id)
        {
            var model = dbContext.TrackRelatedParties.Find(id);

            return View(model);
        }

        [HttpPost]
        public string Edit(TrackRelatedParty model)
        {
            try
            {
                dbContext.Entry(model).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges(); 
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

            return Constants.AJAX_CREATE_SUCCESS_RETURN;
        }

        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                var model = dbContext.TrackRelatedParties.Find(id);
                dbContext.Entry(model).State = System.Data.Entity.EntityState.Deleted;
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