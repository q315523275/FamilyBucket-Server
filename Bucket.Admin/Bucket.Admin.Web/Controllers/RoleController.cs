using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Role;
using Bucket.Admin.Model.Setting;
using Bucket.DbContext.SqlSugar;
using Bucket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class RoleController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly IMapper _mapper;
        private readonly BucketSqlSugarClient _adminDbContext;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="mapper"></param>
        public RoleController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _mapper = mapper;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }

        /// <summary>
        /// 查询所有角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Role/QueryAllRoles")]
        public BaseOutput<object> QueryAllRoles([FromQuery] QueryRolesInput input)
        {
            var list = _adminDbContext.Queryable<RoleModel>()
                                 .WhereIF(!input.PlatformKey.IsEmpty(), it => it.PlatformKey == input.PlatformKey)
                                 .ToList();
            return new BaseOutput<object> { Data = list };
        }
        /// <summary>
        /// 查询可用角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Role/QueryRoles")]
        public BaseOutput<object> QueryRoles([FromQuery] QueryRolesInput input)
        {
            var list = _adminDbContext.Queryable<RoleModel>()
                               .Where(it => it.IsDel == false)
                               .WhereIF(!input.PlatformKey.IsEmpty(), it => it.PlatformKey == input.PlatformKey)
                               .ToList();
            return new BaseOutput<object> { Data = list };
        }
        /// <summary>
        /// 查询角色权限信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Role/QueryRoleInfo")]
        public BaseOutput<object> QueryRoleInfo([FromQuery] QueryRoleInfoInput input)
        {
            var model = _adminDbContext.Queryable<RoleModel>().Where(it => it.Id == input.RoleId).First();
            var apiList = _adminDbContext.Queryable<RoleApiModel>().Where(it => it.RoleId == input.RoleId).ToList();
            var menuList = _adminDbContext.Queryable<RoleMenuModel>().Where(it => it.RoleId == input.RoleId).ToList();
            return new BaseOutput<object> { Data = new { RoleInfo = model, MenuList = menuList, ApiList = apiList } };
        }
        /// <summary>
        /// 设置角色信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Role/SetRole")]
        public BaseOutput SetRole([FromBody] SetRoleInput input)
        {
            try
            {
                _adminDbContext.Ado.BeginTran();

                #region 基础信息更新
                var model = _mapper.Map<RoleModel>(input);
                if (model.Id > 0)
                {
                    model.UpdateTime = DateTime.Now;
                    _adminDbContext.Updateable(model)
                                   .IgnoreColumns(it => new { it.PlatformKey, it.CreateTime, it.IsSys })
                                   .ExecuteCommand();
                }
                else
                {
                    model.CreateTime = DateTime.Now;
                    model.IsDel = false;
                    model.Id = _adminDbContext.Insertable(model).ExecuteReturnIdentity();
                }
                #endregion

                #region 菜单权限
                // 用户角色操作
                List<RoleMenuModel> roleMenuList = new List<RoleMenuModel>();
                foreach (var id in input.MenuIdList)
                {
                    // 防止重复数据
                    if (!roleMenuList.Exists(it => it.MenuId == id))
                    {
                        roleMenuList.Add(new RoleMenuModel
                        {
                            MenuId = id,
                            RoleId = model.Id
                        });
                    }
                }
                // 删除用户当前角色
                _adminDbContext.Deleteable<RoleMenuModel>().Where(f => f.RoleId == model.Id).ExecuteCommand();
                // 添加用户角色
                if (roleMenuList.Count > 0)
                    _adminDbContext.Insertable(roleMenuList).ExecuteCommand();
                #endregion

                #region 接口权限
                // 用户角色操作
                List<RoleApiModel> roleApiList = new List<RoleApiModel>();
                foreach (var id in input.ApiIdList)
                {
                    // 防止重复数据
                    if (!roleApiList.Exists(it => it.ApiId == id))
                    {
                        roleApiList.Add(new RoleApiModel
                        {
                            ApiId = id,
                            RoleId = model.Id
                        });
                    }
                }
                // 删除用户当前角色
                _adminDbContext.Deleteable<RoleApiModel>().Where(f => f.RoleId == model.Id).ExecuteCommand();
                // 添加用户角色
                if (roleApiList.Count > 0)
                    _adminDbContext.Insertable(roleApiList).ExecuteCommand();
                #endregion

                _adminDbContext.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _adminDbContext.Ado.RollbackTran();
                throw new Exception("事务执行失败", ex);
            }
            return new BaseOutput { };
        }
    }
}