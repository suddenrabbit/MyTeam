using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyTeam.Utils;
using MyTeam.Models;
using PagedList;
using MyTeam.Enums;
using System.Web;
using System.IO;

namespace MyTeam.Controllers
{
    /// <summary>
    /// 文档库管理Controller
    /// </summary>
    public class FileManageController : BaseController
    {
#if Release
                private string _filePathPrefix = @"E:\MyTeamFileLab\";
#else
        private string _filePathPrefix = @"D:\MyTeamFileLab\";
#endif


        //
        // GET: /FileManage/

        public ActionResult Index(int pageNum = 1)
        {
            // 分页
            var ls = dbContext.MyFiles.OrderByDescending(a => a.FileID).ToPagedList(pageNum, Constants.PAGE_SIZE);

            // 提供是否管理员、当前登录ID信息，以供前台判断是否可以编辑
            ViewBag.IsAdmin = IsAdminNow();
            ViewBag.CurrentID = GetSessionCurrentUser().UID;

            return View(ls);
        }

        //
        // GET: /FileManage/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /FileManage/Create

        [HttpPost]
        public string Create(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "<p class='alert alert-danger'>未获取到文件</p>";
            }

            FileInfo finalFile = null;

            try
            {
                // 保存文件
                // 判断文件夹是否存在，不存在则创建文件夹
                var dir = _filePathPrefix + DateTime.Now.ToString("yyyyMMdd");
                if (Directory.Exists(dir) == false)//如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(dir);
                }

                string filePath = Path.Combine(dir, Path.GetFileName(file.FileName));

                file.SaveAs(filePath);
                // 读取文件
                finalFile = new FileInfo(filePath);

                var personID = GetSessionCurrentUser().UID;
                var hashName = HashHelper.ComputeMD5(finalFile.FullName).ToUpper();
                var fileType = finalFile.Extension;

                // 将文件重命名为hash形式
                finalFile.MoveTo(finalFile.DirectoryName + "/" + hashName + fileType); // 此处系统会自动判断有无重复文件，若有重复就不会新增一条数据库记录了

                // 根据文件生成记录
                var myFile = new MyFile
                {
                    FileName = Path.GetFileNameWithoutExtension(filePath), // 获取不带扩展的文件名
                    FileSize = finalFile.Length,
                    FileType = fileType,
                    FileHashName = hashName,
                    PersonID = personID,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                dbContext.MyFiles.Add(myFile);
                dbContext.SaveChanges();                

                return Constants.AJAX_CREATE_SUCCESS_RETURN;

            }
            catch (Exception e1)
            {
                if (finalFile != null) finalFile.Delete(); // 如果出错了，要删除已经上传的文件
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }

        }

        //
        // GET: /FileManage/Edit/5

        public ActionResult Edit(int id)
        {
            MyFile myFile = dbContext.MyFiles.ToList().Find(a => a.FileID == id);

            if (myFile == null)
            {
                return View();
            }

            // 只能编辑自己上传的文件
            if(!IsAdminNow() && GetSessionCurrentUser().UID != myFile.PersonID)
            {
                ViewBag.ErrMsg = "非管理员只能编辑自己上传的文件！";
                return View();
            }

            return View(myFile);
        }

        //
        // POST: /FileManage/Edit/5

        [HttpPost]
        public string Edit(MyFile myFile)
        {
            try
            {
                dbContext.Entry(myFile).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /FileManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                MyFile myFile = dbContext.MyFiles.ToList().Find(a => a.FileID == id);

                // 删除文件
                var dir = _filePathPrefix + myFile.CreateTime.ToString("yyyyMMdd");
                var filePath = Path.Combine(dir, myFile.FileHashName + myFile.FileType);

                // 读取文件
                var file = new FileInfo(filePath);

                if(!file.Exists)
                {
                    throw new Exception("文件不存在！");
                }

                file.Delete();

                dbContext.Entry(myFile).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return "<p class='alert alert-success'>删除成功</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 下载
        public ActionResult Download(int id)
        {
            return null;
        }
    }


}
