using SqlSugar;

namespace Bucket.Admin.Model.User
{
    /// <summary>
    /// 用户角色实体类
    /// </summary>
    [SugarTable("tb_user_roles")]
    public class UserRoleModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户唯一UdcId
        /// </summary>
        public long Uid { get; set; }
        /// <summary>
        /// 角色唯一ID
        /// </summary>
        public int RoleId { get; set; }
    }
}
