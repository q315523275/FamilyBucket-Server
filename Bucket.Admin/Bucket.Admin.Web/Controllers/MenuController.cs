using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Menu;
using Bucket.Admin.Model.Setting;
using Bucket.Admin.Model.User;
using Bucket.Core;
using Bucket.DbContext.SqlSugar;
using Bucket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Linq;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class MenuController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly IMapper _mapper;
        private readonly IUser _user;
        private readonly BucketSqlSugarClient _adminDbContext;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="mapper"></param>
        /// <param name="user"></param>
        public MenuController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper, IUser user)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _mapper = mapper;
            _user = user;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }
        /// <summary>
        /// 查询平台菜单
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Menu/QueryAllMenus")]
        public BaseOutput<object> QueryAllMenus([FromQuery] QueryAllMenusInput input)
        {
            var list = _adminDbContext.Queryable<MenuModel>()
                                 .WhereIF(input.PlatformId > 0, it => it.PlatformId == input.PlatformId)
                                 .ToList();
            return new BaseOutput<object> { Data = list };
        }
        /// <summary>
        /// 设置平台菜单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Menu/SetPlatform")]
        public BaseOutput SetPlatform([FromBody] SetMenuInput input)
        {
            var model = _mapper.Map<MenuModel>(input);
            if (model.Id > 0)
            {
                _adminDbContext.Updateable(model).ExecuteCommand();
            }
            else
            {
                _adminDbContext.Insertable(model).ExecuteCommand();
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 查询用户菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Menu/QueryUserMenus")]
        public BaseOutput<object> QueryUserMenus([FromQuery] QueryUserMenuInput input)
        {
            var uid = _user.Id.ToLong();
            var platformId = 0;
            if (!input.PlatformKey.IsEmpty())
            {
                var platformInfo = _adminDbContext.Queryable<PlatformModel>().First(it => it.Key == input.PlatformKey && !it.IsDel);
                if (platformInfo != null)
                    platformId = platformInfo.Id;
            }
            var list = _adminDbContext.Queryable<MenuModel, RoleMenuModel, UserRoleModel>((t1, t2, t3) => new object[] { JoinType.Inner, t1.Id == t2.MenuId, JoinType.Inner, t2.RoleId == t3.RoleId })
                                      .WhereIF(platformId > 0, (t1, t2, t3) => t1.PlatformId == platformId)
                                      .Where((t1, t2, t3) => t1.State == 1 && t3.Uid == uid)
                                      .OrderBy((t1, t2, t3) => t1.SortId, OrderByType.Asc)
                                      .GroupBy((t1, t2, t3) => t1.Id)
                                      .Select<MenuModel>()
                                      .ToList();
            var platformIdList = list.GroupBy(p => p.PlatformId).Select(it => it.First().PlatformId).ToList();
            var pidArr = platformIdList.ToArray();
            var platformList = _adminDbContext.Queryable<PlatformModel>().Where(it => it.IsDel == false && pidArr.Contains(it.Id)).ToList();
            return new BaseOutput<object> { Data = new { Menu = list, Platform = platformList } };
        }
    }
}
