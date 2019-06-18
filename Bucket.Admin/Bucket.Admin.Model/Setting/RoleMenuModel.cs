using SqlSugar;

namespace Bucket.Admin.Model.Setting
{
    /// <summary>
    /// 角色菜单实体类
    /// </summary>
    [SugarTable("tb_role_menus")]
    public class RoleMenuModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public int MenuId { get; set; }
        /// <summary>
        /// 角色唯一ID
        /// </summary>
        public int RoleId { get; set; }
    }
}
