using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        办结 = 7,
        取消 = 8
    }
}