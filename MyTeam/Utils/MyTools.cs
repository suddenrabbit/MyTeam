using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MyTeam.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class MyTools
    {
        /// <summary>
        /// 下拉列表通用处理
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="forQuery"></param>
        /// <param name="forEdit"></param>
        /// <param name="toEditValue"></param>
        /// <param name="staticList"></param>
        /// <returns></returns>
        public static SelectList GetSelectList(List<string> sourceList, bool forQuery = false, bool forEdit = false, string toEditValue = null, bool staticList = true, string emptyText = "全部")
        {
            List<string> ls = null;
            if (staticList)
            {
                ls = new List<string>(sourceList.ToArray());
            }
            else
            {
                ls = sourceList;
            }
            if (forQuery)
            {
                ls.Insert(0, emptyText);
            }
            SelectList sl = null;
            if (forEdit)
            {
                sl = new SelectList(ls, toEditValue);
            }
            else
            {
                sl = new SelectList(ls);
            }

            return sl;
        }

        /// <summary>
        /// 通过枚举生成下拉列表
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="forQuery"></param>
        /// <param name="forEdit"></param>
        /// <param name="toEditValue"></param>
        /// <param name="emptyText"></param>
        /// <returns></returns>
        public static SelectList GetSelectListByEnum(Type enumType, bool forQuery = false, bool forEdit = false, string toEditValue = null, string emptyText = "全部")
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            foreach (int e in Enum.GetValues(enumType))
            {
                ls.Add(new SelectListItem { Value = e.ToString(), Text = Enum.GetName(enumType, e) });
                            
            }

            if (forQuery)
            {
                ls.Insert(0, new SelectListItem { Value = "", Text = emptyText });
            }
            SelectList sl = null;
            if (forEdit)
            {
                sl = new SelectList(ls, "Value", "Text", toEditValue);
            }
            else
            {
                sl = new SelectList(ls, "Value", "Text");
            }

            return sl;
        }

        /// <summary>
        /// 通过简单枚举生成下拉列表
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static SelectList GetSelectListBySimpleEnum(Type enumType)
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            foreach (var e in Enum.GetValues(enumType))
            {
                ls.Add(new SelectListItem { Value = e.ToString(), Text = e.ToString() });

            }
              
            SelectList sl = new SelectList(ls, "Value", "Text");           

            return sl;
        }
    }
}