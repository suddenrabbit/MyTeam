using MyTeam.Models;
using MyTeam.Utils;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MyTeam.Controllers
{
    public class TrackTaskManageController : BaseController
    {
        public ActionResult Index(TrackTaskQuery query, bool isQuery = false, int pageNum = 1)
        {
            if (isQuery)
            {
                var ls = from a in dbContext.TrackTasks
                         select a;

                if (!string.IsNullOrEmpty(query.TrackTaskName))
                {
                    ls = ls.Where(p => p.TrackTaskName.Contains(query.TrackTaskName));
                }

                if (query.TaskPersonID != 0)
                {
                    var persons = dbContext.TrackTaskPersons.Where(q => q.PersonID == query.TaskPersonID).Select(p => p.TrackTaskID);
                    ls = ls.Where(p => persons.Contains(p.TrackTaskID));
                }

                if (query.TrackTaskStat != 0)
                {
                    ls = ls.Where(p => p.TrackTaskStat == query.TrackTaskStat);
                }

                query.ResultList = ls.ToList().ToPagedList(pageNum, Constants.PAGE_SIZE);
            }
            else
            {
                query = new TrackTaskQuery { TaskPersonID = 0, TrackTaskStat = 0 };
            }

            // 人员下拉
            var ls1 = GetStaffList();

            ls1.Insert(0, new User() { UID = 0, Realname = "全部" });

            SelectList sl = new SelectList(ls1, "UID", "Realname", query.TaskPersonID); // 选中当前值

            ViewBag.PersonList = sl;

            return View(query);
        }

        public ActionResult Create()
        {
            // 人员下拉
            var ls1 = GetStaffList();

            SelectList sl = new SelectList(ls1, "UID", "Realname", GetSessionCurrentUser().UID); // 选中当前值

            ViewBag.PersonList = sl;

            return View();
        }

        [HttpPost]
        public string Create(TrackTaskCreateEdit model)
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
                dbContext.TrackTasks.Add(model.TrackTask); // Task部分

                // 添加项目干系人
                foreach(var p in model.TaskPersons)
                {
                    var pid = int.Parse(p);
                    var person = new TrackTaskPerson { TrackTaskID = model.TrackTask.TrackTaskID, PersonID = pid };
                    dbContext.TrackTaskPersons.Add(person);
                }

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
            var task = dbContext.TrackTasks.Find(id);

            var model = new TrackTaskCreateEdit { TrackTask = task };

            List<string> pids = new List<string>();
            foreach(var p in task.TrackTaskPersons)
            {
                pids.Add(p.PersonID.ToString());
            }

            model.TaskPersons = pids.ToArray();

            // 人员下拉
            var ls1 = GetStaffList();

            SelectList sl = new SelectList(ls1, "UID", "Realname", model.TaskPersons);

            ViewBag.PersonList = sl;

            return View(model);
        }

        [HttpPost]
        public string Edit(TrackTask model)
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
                var model = dbContext.TrackTasks.Find(id);
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