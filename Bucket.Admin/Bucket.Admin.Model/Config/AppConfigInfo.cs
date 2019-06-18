using SqlSugar;
using System;

namespace Bucket.Admin.Model.Config
{
    public class AppConfigModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { set; get; }
        public string ConfigAppId { set; get; }
        public string ConfigNamespaceName { set; get; }
        public string ConfigKey { set; get; }
        public string ConfigValue { set; get; }
        public string Remark { set; get; }
        public DateTime LastTime { set; get; }
        public DateTime CreateTime { set; get; }
        public long Version { set; get; }
        public bool IsDeleted { set; get; }
    }
}
