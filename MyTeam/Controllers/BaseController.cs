using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTeam.Models;
using MyTeam.Utils;
using OfficeOpenXml;
using System.IO;
using System.Reflection;
using MyTeam.Enums;

namespace MyTeam.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected MyTeamContext dbContext = null;   

        public BaseController()
        {
            dbContext = new MyTeamContext();            
        }
       
        /*protected void SetSessionCurrentUser(int UID)
        {
            Session["UID"] = UID;
        }*/

        protected User GetSessionCurrentUser()
        {
            if(!User.Identity.IsAuthenticated || User.Identity.Name == "")
            {
                return null;
            }

            User user = null;
            try
            {
                user = this.GetUserList().Find(a => a.UID.ToString() == User.Identity.Name);
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

        /// <summary>
        /// 更新内存
        /// </summary>
        /// <param name="type">0-全部 1-用户 2-系统 3-项目 4-参数</param>
        protected void Update(int type = 0)
        {
            switch (type)
            {
                case 1:
                    Constants.UserList = dbContext.Users.ToList();
                    break;
                case 2:
                    Constants.SysList = dbContext.RetailSystems.ToList();
                    break;
                case 3:
                    Constants.ProjList = dbContext.Projs.ToList();
                    break;
                case 4:
                    Constants.ParamList = dbContext.Params.ToList();
                    break;
                default:
                    Constants.UserList = dbContext.Users.ToList();
                    Constants.SysList = dbContext.RetailSystems.ToList();
                    Constants.ProjList = dbContext.Projs.ToList();
                    Constants.ParamList = dbContext.Params.ToList();
                    break;
            }
        }

        /// <summary>
        /// 获取内存中的用户列表
        /// </summary>
        /// <returns></returns>
        protected List<User> GetUserList()
        {
            if (Constants.UserList == null)
            {
                Constants.UserList = dbContext.Users.ToList<User>();
            }
            return new List<User>(Constants.UserList.ToArray()); //生成一个新的list对象，防止改变内存中的静态值，下同
        }

        /// <summary>
        /// 获取内存中的系统列表
        /// </summary>
        /// <returns></returns>
        protected List<RetailSystem> GetSysList()
        {
            if (Constants.SysList == null)
            {
                Constants.SysList = dbContext.RetailSystems.ToList<RetailSystem>();
            }
            return new List<RetailSystem>(Constants.SysList.ToArray());
        }

        /// <summary>
        /// 获取内存中的项目列表
        /// </summary>
        /// <returns></returns>
        protected List<Proj> GetProjList()
        {
            if (Constants.ProjList == null)
            {
                Constants.ProjList = dbContext.Projs.ToList<Proj>();
            }
            return new List<Proj>(Constants.ProjList.ToArray());
        }

        // 生成Excel
        protected FileContentResult MakeExcel<T>(String templateFileName, String targetFileName,
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
            if (obj == null)
                return "";

            if (pi.PropertyType == typeof(DateTime))
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

        // 2016.7.16新增：获取状态为1的系统列表（用于维护需求相关）
        /// <summary>
        /// 获取正常系统列表
        /// </summary>
        /// <returns></returns>
        protected List<RetailSystem> GetNormalSysList()
        {
            return GetSysList().Where(p => p.SysStat == 1).ToList();
        }

        // 2016.12.15新增：获取状态为1的用户列表
        /// <summary>
        /// 获取行员列表
        /// </summary>
        /// <returns></returns>
        protected List<User> GetFormalUserList()
        {
            return GetUserList().Where(p => p.UserType == (int)UserTypeEnums.行员).ToList();
        }

        /// <summary>
        /// 获取行员+外协列表
        /// </summary>
        /// <returns></returns>
        protected List<User> GetStaffList()
        {
            return GetUserList().Where(p => p.UserType == (int)UserTypeEnums.行员 || p.UserType == (int)UserTypeEnums.外协).ToList();
        }

        /// 2017.2.27新增：获取参数列表

        /// <summary>
        /// 获取内存中的参数列表
        /// </summary>
        /// <returns></returns>
        protected List<Param> GetParamList()
        {
            if (Constants.ParamList == null)
            {
                Constants.ParamList = dbContext.Params.ToList();
            }
            return new List<Param>(Constants.ParamList.ToArray());
        }

        /// <summary>
        /// 获取需求发起部门参数列表
        /// </summary>
        /// <returns></returns>
        protected List<Param> GetReqFromDeptList()
        {            
            return GetParamList().Where(p => p.ParamType == Constants.PARAM_TYPE_REQ_FROM_DEPT).ToList();
        }

        /// <summary>
        /// 获取周报工作类型参数列表
        /// </summary>
        /// <returns></returns>
        protected List<Param> GetWorkTypeList()
        {
            return GetParamList().Where(p => p.ParamType == Constants.PARAM_TYPE_WORK_TYPE).ToList();
        }
    }
}