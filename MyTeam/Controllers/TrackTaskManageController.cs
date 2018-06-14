﻿using MyTeam.Models;
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

            MultiSelectList sl = new MultiSelectList(ls1, "UID", "Realname", new string[] { GetSessionCurrentUser().UID.ToString() }); // 选中当前值

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

            //model.TaskPersons = task.TrackTaskPersons.Select(m => m.PersonID.ToString()).ToArray();

            var persons = task.TrackTaskPersons.Select(m => m.PersonID.ToString()).ToArray();

            model.OldTaskPersons = string.Join(",", persons);

            // 人员下拉
            var ls1 = GetStaffList();

            MultiSelectList sl = new MultiSelectList(ls1, "UID", "Realname", persons); // 选中当前值

            ViewBag.PersonList = sl;

            return View(model);
        }

        [HttpPost]
        public string Edit(TrackTaskCreateEdit model)
        {
            try
            {
                dbContext.Entry(model.TrackTask).State = System.Data.Entity.EntityState.Modified;

                // 调整项目干系人
                // 比较新旧值，找出差异，增加或删除
                var oldPersons = model.OldTaskPersons.Split(',');
                // 增加：新的有，旧的没有
                foreach(var n in model.TaskPersons)
                {
                    if(!oldPersons.Contains(n))
                    {
                        var newModel = new TrackTaskPerson { TrackTaskID = model.TrackTask.TrackTaskID, PersonID = int.Parse(n) };
                        dbContext.TrackTaskPersons.Add(newModel);
                    }
                }
                // 删除：旧的有，新的没有
                foreach(var o in oldPersons)
                {
                    if(!model.TaskPersons.Contains(o))
                    {
                        var toDelPersonID = int.Parse(o);
                        var todel = dbContext.TrackTaskPersons.Where(p => p.TrackTaskID == model.TrackTask.TrackTaskID && p.PersonID == toDelPersonID).FirstOrDefault();
                        dbContext.Entry(todel).State = System.Data.Entity.EntityState.Deleted;
                    }
                }

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

        // ajax 接口，在Edit页面直接调整干系人
        [HttpPost]
        public string ModifyPerson(int trackTaskID, int personID, int id = 0) // id = 0 means add
        {
            if(id != 0)
            {
                var todel = dbContext.TrackTaskPersons.Find(id);
                dbContext.Entry(todel).State = System.Data.Entity.EntityState.Deleted;
                
            }
            else
            {
                var find = dbContext.TrackTaskPersons.Where(p => p.TrackTaskID == trackTaskID && p.PersonID == personID).FirstOrDefault();
                if (find != null)
                {
                    return "不能重复添加";
                }

                var model = new TrackTaskPerson { TrackTaskID = trackTaskID, PersonID = personID };
                dbContext.TrackTaskPersons.Add(model);
            }
            
            dbContext.SaveChanges();
            return "success";
        }
    }
}