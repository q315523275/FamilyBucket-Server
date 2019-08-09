using SqlSugar;

namespace Bucket.Identity.Model
{
    [SugarTable("wechat_apps")]
    public class WechatAppInfo
    {
        [SugarColumn(IsPrimaryKey = false, IsIdentity = false)]
        public string AppId { set; get; }
        public string AppSecret { set; get; }
    }
}
