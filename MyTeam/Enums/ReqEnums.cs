namespace MyTeam.Enums
{
    public enum ReqTypeEnums
    {
        //未知 = 0,
        监管要求 = 1,
        软件缺陷 = 2,
        适应性要求 = 3,
        新增功能 = 4,
        功能改进 = 5,
        性能优化 = 6,
        项目配套 = 7,
        专项工作 = 8
    }

    public enum ReqStatEnums
    {
        待评估 = 1,
        入池 = 2,
        拒绝 = 3,
        纳入升级 = 4,
        作废 = 5,
        出池 = 6,
        已下发 = 7,
        取消 = 8
    }

    public enum ReqFromDeptEnums
    {
        零售风险管理部, 普惠金融事业部, 零售资产负债部, 银行卡与渠道部, 私人银行部, 信息科技部, 网络金融部, 运营管理部, 计划财务部, 审计部, 企业金融总部, 分行
    }
}