using MyTeam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyTeam.Utils
{
    public class Constants
    {
        /// <summary>
        /// AJAX返回成功标志
        /// </summary>
        public const string AJAX_RESULT_SUCCESS = "success";

        /// <summary>
        /// 获取内存中的用户列表
        /// </summary>
        public static List<User> UserList;        

        /// <summary>
        /// 获取内存中的系统列表
        /// </summary>
        public static List<RetailSystem> SysList;

        /// <summary>
        /// 需求发起部门列表
        /// </summary>
        public static List<string> ReqFromDeptList = new List<string>() { "零售银行总部", "信息科技部", "电子银行部", "信用卡中心", "支付结算部", "私人银行部", "审计部" };

        /// <summary>
        /// 需求类型列表
        /// </summary>
        public static List<string> ReqTypeList = new List<string>() { "功能改进", "新增功能", "性能优化", "适应性需求", "软件缺陷", "项目配套", "监管要求" };

        /// <summary>
        /// 需求状态列表
        /// </summary>
        public static List<string> ReqStatList = new List<string>() { "未办结", "入池", "出池", "办结", "作废", "拒绝", "整合", "纳入升级" };

        /// <summary>
        /// 任务阶段列表
        /// </summary>
        public static List<string> WorkStageList = new List<string>() { "/", "调研小组成立", "需求调研", "需求大纲开发", "需求大纲审核", "业务可行性分析", "技术可行性分析", "技术方案编写", "技术方案审核", "实施方案汇报", "已立项", "业务需求开发", "业务需求评审", "招标准备", "软件需求开发", "系统设计", "系统编码", "系统测试", "上线配合", "暂缓", "完成" };
 
        /// <summary>
        /// 工作完成情况列表
        /// </summary>
        public static List<string> WorkStatList = new List<string>() { "计划", "部分完成", "暂停", "完成" };

    }
}