namespace MyTeam.Models
{
    public class OutPoolTableResult : OutPoolTableResultExcel
    {
        // Excel里不能有short字段，其他保持一致，故继承子类   

        public string ShortReqReason //需求申请事由，只显示25个字
        {
            get
            {
                if (this.ReqReason.Length > 25)
                    return this.ReqReason.Substring(0, 24) + "...";
                else
                    return this.ReqReason;
            }
            set
            {
                this.ShortReqReason = value;
            }
        }

        public string ShortReqDesc //需求描述，只显示45个字
        {
            get
            {
                if (this.ReqDesc.Length > 45)
                    return this.ReqDesc.Substring(0, 44) + "...";
                else
                    return this.ReqDesc;
            }
            set
            {
                this.ShortReqDesc = value;
            }
        }
    }
}