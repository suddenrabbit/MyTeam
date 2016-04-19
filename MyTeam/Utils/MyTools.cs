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
        public static SelectList GetSelectList(List<string> sourceList, bool forQuery = false, bool forEdit = false, string toEditValue = null, bool staticList = true)
        {
            List<string> ls = null;
            if(staticList)
            {
                ls = new List<string>(sourceList.ToArray());
            }
            else
            {
                ls = sourceList;
            }
            if (forQuery)
            {
                ls.Insert(0, "全部");
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
    }
}