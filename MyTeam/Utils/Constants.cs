using MyTeam.Models;
using System.Collections.Generic;

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
        /// 获取内存中的项目列表
        /// </summary>
        public static List<Proj> ProjList;

        /// <summary>
        /// 需求发起部门列表/主办部门列表
        /// </summary>
        public static List<string> ReqFromDeptList = new List<string>() { "零售银行总部", "信息科技部", "网络金融部", "信用卡中心", "运营管理部", "私人银行部", "审计部", "企业金融总部" };

        /// <summary>
        /// 需求类型列表
        /// </summary>
        //public static List<string> ReqTypeList = new List<string>() { "功能改进", "新增功能", "性能优化", "适应性需求", "软件缺陷", "项目配套", "监管要求", "专项工作" };

        /// <summary>
        /// 需求状态列表
        /// </summary>
        //public static List<string> ReqStatList = new List<string>() { "未办结", "待评估", "入池", "出池", "办结", "作废", "拒绝", "整合", "纳入升级", "取消" };

        /// <summary>
        /// 任务阶段列表
        /// </summary>
        public static List<string> WorkStageList = new List<string>() { "/", "调研小组成立", "需求调研", "需求大纲开发", "需求大纲审核", "业务可行性分析", "技术可行性分析", "技术方案编写", "技术方案审核", "实施方案汇报", "已立项", "业务需求开发", "业务需求评审", "招标准备", "软件需求开发", "系统设计", "系统编码", "系统测试", "上线配合", "暂缓", "完成" };

        /// <summary>
        /// 工作完成情况列表
        /// </summary>
        public static List<string> WorkStatList = new List<string>() { "计划", "部分完成", "暂停", "完成" };

        /// <summary>
        /// 需求调研方式列表
        /// </summary>
        public static List<string> SurveyWayList = new List<string>() { "理论研究", "专题讨论", "供应商交流", "同业调研", "用户访谈", "现场观摩", "调查问卷" };

        /// <summary>
        /// 需求会议类型列表
        /// </summary>
        public static List<string> MeetingTypeList = new List<string>() { "需求研讨会", "业务部门交流会", "上线评审会", "业务需求评审会", "业务需求内部评审会", "软件需求评审会", "其他会议" };

        /// <summary>
        /// 需求会议评审结果列表
        /// </summary>
        public static List<string> ReviewConclusionList = new List<string>() { "通过", "有条件通过", "不通过", "不适用" };

        /// <summary>
        /// 需求会议当前状态列表
        /// </summary>
        public static List<string> StatList = new List<string>() { "计划", "进行中", "办结" };

        /// <summary>
        /// 业需软需状态跟踪优先级列表
        /// </summary>
        public static List<string> PriorityList = new List<string>() { "高", "中", "低" };

        /// <summary>
        /// 业需软需状态跟踪变更标识列表
        /// </summary>
        public static List<string> ChangeCharList = new List<string>() { "原始", "修改", "增加", "删除" };

        /// <summary>
        /// 业需软需状态跟踪需求状态列表
        /// </summary>
        public static List<string> ReqSoftStatList = new List<string>() { "设计", "编码", "测试", "完成", "作废" };

        /// <summary>
        /// 分页的每页记录数
        /// </summary>
        public const int PAGE_SIZE = 20;

        public static List<string> BusiReqStat = new List<string>() { "一期原始需求", "二期原始需求", "三期原始需求", "新增维护需求", "需求已作废", "文档更新增补" };

        /// <summary>
        /// 年度版本下发计划发布频率列表
        /// </summary>
        public static List<string> ReleaseFreqList = new List<string>() { "1", "2", "3", "4", "6", "12", "0" };

        /// <summary>
        /// AJAX新增成功后统一返回HTML值
        /// </summary>
        public static string AJAX_CREATE_SUCCESS_RETURN = "<p class='alert alert-success'>添加成功！<a href='#' data-dismiss='modal' onclick='javascript:window.location.reload()'>关闭</a></p>";

        /// <summary>
        /// AJAX编辑成功后统一返回HTML值
        /// </summary>
        public static string AJAX_EDIT_SUCCESS_RETURN = "<p class='alert alert-success'>修改成功！<a href='#' data-dismiss='modal' onclick='javascript:window.location.reload()'>关闭</a></p>";

        /// <summary>
        /// 工作类型下拉列表
        /// </summary>
        public static List<string> WorkTypeList = new List<string>() { "创新项目", "科技引领", "专项工作", "解决方案", "维护需求", "创新研究", "培训学习", "规模度量", "制度建设", "原型工具应用", "人力资源管理", "物料资源管理", "信息资源管理", "单位内务", "其他", "流程银行" };

        /// <summary>
        /// 项目状态下拉列表
        /// </summary>
        public static List<string> ProjStatList = new List<string>() { "进行中", "完成", "暂停", "取消" };

    }
}