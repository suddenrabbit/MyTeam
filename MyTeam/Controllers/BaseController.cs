using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Models;
using MyTeam.Utils;
using OfficeOpenXml;
using System.IO;
using System.Collections;
using System.Reflection;

namespace MyTeam.Controllers
{
    public class BaseController : Controller
    {
        protected MyTeamContext dbContext = new MyTeamContext();


        protected void SetSessionCurrentUser(int UID)
        {
            Session["UID"] = UID;
        }

        protected User GetSessionCurrentUser()
        {
            User user = null;
            try
            {
                user = this.GetUserList().Find(a => a.UID == (int)Session["UID"]);
            }
            catch
            {
                // do nothing
            }
            return user;
        }

        // 判断当前用户是否为管理员
        protected bool IsAdminNow()
        {
            return Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
        }

        // 更新内存
        protected void Update()
        {
            Constants.UserList = dbContext.Users.ToList<User>();
            Constants.SysList = dbContext.RetailSystems.ToList<RetailSystem>();
        }

        protected List<User> GetUserList()
        {
            if (Constants.UserList == null)
            {
                Constants.UserList = dbContext.Users.ToList<User>();
            }
            return new List<User>(Constants.UserList.ToArray()); //生成一个新的list对象，防止改变内存中的静态值，下同
        }

        protected List<RetailSystem> GetSysList()
        {
            if (Constants.SysList == null)
            {
                Constants.SysList = dbContext.RetailSystems.ToList<RetailSystem>();
            }
            return new List<RetailSystem>(Constants.SysList.ToArray());
        }

        // 生成Excel
        protected FileContentResult makeExcel<T>(String templateFileName, String targetFileName,
            List<T> ls, int headSize = 1)
        {
            // 读取模板
            string tmpFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Content/templates/" + templateFileName + ".xlsx");
            // POIFSFileSystem fs = new POIFSFileSystem(new FileStream(tmpFilePath, FileMode.OpenOrCreate));
            using (ExcelPackage ep = new ExcelPackage(new FileInfo(tmpFilePath)))
            {
                ExcelWorkbook wb = ep.Workbook;
                ExcelWorksheet sheet = wb.Worksheets[1]; // 获取第一个sheet

                // 开始循环写入数据，起始行：headSize + 1
                int rowNum = headSize + 1;

                foreach (T m in ls)
                {
                    sheet.Cells[rowNum, 1].Value = rowNum - headSize; //序号

                    int colNum = 0;

                    foreach (PropertyInfo pi in m.GetType().GetProperties())
                    {
                        sheet.Cells[rowNum, colNum + 2].Value = this.GetPropertyInfoValue<T>(pi, m);
                        colNum++;
                    }

                    rowNum++;
                }

                // 下载
                // 文件名中文处理
                targetFileName = HttpUtility.UrlEncode(targetFileName);

                return File(ep.GetAsByteArray(), "application/excel", targetFileName + ".xlsx");
            }
        }

        private string GetPropertyInfoValue<T>(PropertyInfo pi, T m)
        {
            object obj = pi.GetValue(m, null);
            if(obj == null)
                return "";

            if(pi.PropertyType == typeof(DateTime))
            {
                return ((DateTime)obj).ToShortDateString();
            }
            else if (pi.PropertyType == typeof(DateTime?))
            {
                return ((DateTime?)obj).Value.ToShortDateString();
            }
            else
            {
                return obj.ToString();
            }
        }
    }
}