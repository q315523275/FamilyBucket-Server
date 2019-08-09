using SqlSugar;

namespace Bucket.Identity.Model
{
    [SugarTable("tb_thirdoauths")]
    public class ThirdOAuthInfo
    {
        /// <summary>
        /// udcid 
        /// </summary>
        [SugarColumn(IsPrimaryKey = false, IsIdentity = false)]
        public long Uid { set; get; }
        public string OpenId { set; get; }
        public string UnionId { set; get; }
        public string AuthServer { set; get; }
        public string AppId { set; get; }
    }
}
