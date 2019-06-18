using SqlSugar;
using System;

namespace Bucket.Config.Model
{
    /// <summary>
    /// 配置信息
    /// </summary>
    [SugarTable("tb_appconfig")]
    public class AppConfigInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { set; get; }
        /// <summary>
        /// AppId
        /// </summary>
        public string ConfigAppId { set; get; }
        /// <summary>
        /// AppId下应用
        /// </summary>
        public string ConfigNamespaceName { set; get; }
        /// <summary>
        /// 配置Key
        /// </summary>
        public string ConfigKey { set; get; }
        /// <summary>
        /// 配置值
        /// </summary>
        public string ConfigValue { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { set; get; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { set; get; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastTime { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { set; get; }
        /// <summary>
        /// 版本Id,修改加一
        /// </summary>
        public long Version { get; set; }
    }
}
