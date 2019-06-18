namespace Bucket.Config.IService
{
    public interface IConfigService
    {
        /// <summary>
        /// 查询配置库表名
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        string GetConfigTableName(string environment);
    }
}
