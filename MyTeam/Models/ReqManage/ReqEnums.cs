using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyTeam.Models
{
    public enum ReqTypeEnum
    {
        未知 = 0,
        监管要求 = 1,
        软件缺陷 = 2,
        适应性要求 = 3,
        新增功能 = 4,
        功能改进 = 5,
        性能优化 = 6,
        项目配套 = 7,
        专项工作 = 8
    }
}