using MyTeam.Models;
using MyTeam.Utils;
using PagedList;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

                var fileName = Path.GetFileNameWithoutExtension(filePath);

                // 判断文件名是否重复
                var checkFile = dbContext.MyFiles.Where(p => p.FileName == fileName).FirstOrDefault();
                if (checkFile != null)
                {
                    throw new Exception("此文件与其他现有文件重名，不能上传！");
                }

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
                    FileName = fileName, // 获取不带扩展的文件名
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
        [HttpGet]
        public ActionResult Edit(int id)
        {
            // 只能编辑自己上传的文件
            if (!IsAdminNow() && GetSessionCurrentUser().UID != id)
            {
                ViewBag.ErrMsg = "非管理员只能更新自己上传的文件！";
            }

            ViewBag.FileID = id;

            return View();
        }

        //
        // POST: /FileManage/Edit/5

        [HttpPost]
        public string Edit(int id, bool keepFileName, HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "<p class='alert alert-danger'>未获取到文件</p>";
            }

            FileInfo finalFile = null;

            try
            {
                // 获取文件记录
                var fileRecord = dbContext.MyFiles.Find(id);
                var oldHashName = fileRecord.FileHashName;

                var dir = _filePathPrefix + fileRecord.CreateTime.ToString("yyyyMMdd");

                // 保存新文件
                if (Directory.Exists(dir) == false)//如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(dir);
                }

                string filePath = Path.Combine(dir, Path.GetFileName(file.FileName));

                file.SaveAs(filePath);

                // 读取文件
                finalFile = new FileInfo(filePath);

                // 获取新数据
                var newFileHashName = HashHelper.ComputeMD5(finalFile.FullName).ToUpper();
                var newFileName = Path.GetFileNameWithoutExtension(filePath);
                var newFileSize = finalFile.Length;

                // 首先判断文件类型
                if (fileRecord.FileType != finalFile.Extension)
                {
                    throw new Exception("不同类型的文件不能更新！");
                }

                // 判断是否重名
                /*if (!keepFileName)
                {
                    var checkFile = dbContext.MyFiles.Where(p => p.FileID != id && p.FileName == newFileName).FirstOrDefault();
                    if (checkFile != null)
                    {
                        throw new Exception("此文件与其他现有文件重名，不能更新！");
                    }
                }*/

                // 将文件重命名为hash形式
                finalFile.MoveTo(finalFile.DirectoryName + "/" + newFileHashName + finalFile.Extension); //此处自动判重                   

                // 更新数据库记录
                if (!keepFileName)
                {
                    fileRecord.FileName = newFileName;
                }
                fileRecord.FileHashName = newFileHashName;
                fileRecord.FileSize = newFileSize;
                fileRecord.UpdateTime = DateTime.Now;

                dbContext.Entry(fileRecord).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                // 最后删除旧文件
                string oldFilePath = Path.Combine(dir, oldHashName + fileRecord.FileType);

                var oldFileInfo = new FileInfo(oldFilePath);

                if (oldFileInfo.Exists)
                {
                    oldFileInfo.Delete();
                }

                return Constants.AJAX_EDIT_SUCCESS_RETURN;
            }
            catch (Exception e1)
            {
                if (finalFile != null) finalFile.Delete(); // 出错的时候删除已有文件
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // AJAX调用
        // POST: /FileManage/Delete/5
        [HttpPost]
        public string Delete(int id)
        {
            if (!IsAdminNow() && GetSessionCurrentUser().UID != id)
            {
                return "<p class='alert alert-danger'>非管理员只能更新自己上传的文件！</p>";
            }

            try
            {
                MyFile myFile = dbContext.MyFiles.ToList().Find(a => a.FileID == id);

                // 删除文件
                var dir = _filePathPrefix + myFile.CreateTime.ToString("yyyyMMdd");
                var filePath = Path.Combine(dir, myFile.FileHashName + myFile.FileType);

                // 读取文件
                var file = new FileInfo(filePath);

                var warning = "";

                if (file.Exists)
                {
                    file.Delete();
                }
                else
                {
                    warning = "<p class='alert alert-warning'>未找到相应文件，仅删除数据库记录</p>";
                }

                dbContext.Entry(myFile).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();

                return warning.Length > 0 ? warning : "<p class='alert alert-success'>删除成功</p>";
            }
            catch (Exception e1)
            {
                return "<p class='alert alert-danger'>出错了: " + e1.Message + "</p>";
            }
        }

        // 下载
        public ActionResult Download(int id)
        {
            try
            {
                // 获取文件
                MyFile myFile = dbContext.MyFiles.ToList().Find(a => a.FileID == id);

                var dir = _filePathPrefix + myFile.CreateTime.ToString("yyyyMMdd");
                var filePath = Path.Combine(dir, myFile.FileHashName + myFile.FileType);
                if(!new FileInfo(filePath).Exists)
                {
                    throw new Exception("未能找到相应文件，请联系管理员！");
                }

                var result = File(filePath, "text/plain", Url.Encode(myFile.FileName + myFile.FileType));

                // 更新下载次数
                myFile.DownloadTimes += 1;
                dbContext.Entry(myFile).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();

                return result;

            }
            catch (Exception e1)
            {
                return Content("出错了: " + e1.Message);
            }
        }

        // 重命名
        public string Rename(int id, string fileName)
        {
            try
            {
                /*var file = dbContext.MyFiles.Where(p => p.FileName == fileName).FirstOrDefault();
                if (file != null)
                {
                    return "此文件名已存在！";
                }*/

                int num = dbContext.Database.ExecuteSqlCommand("update MyFiles set FileName=@p0, UpdateTime=@p1 where FileID=@p2", fileName, DateTime.Now, id);
                if (num == 0)
                {
                    return "更新失败！";
                }
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
            return "success";
        }
    }


}
