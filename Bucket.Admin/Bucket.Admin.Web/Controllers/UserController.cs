using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.User;
using Bucket.Admin.Model.Setting;
using Bucket.Admin.Model.User;
using Bucket.DbContext.SqlSugar;
using Bucket.Utility;
using Bucket.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class UserController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly IMapper _mapper;
        private readonly BucketSqlSugarClient _adminDbContext;

        public UserController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _mapper = mapper;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/User/QueryUsers")]
        public BasePageOutput<List<QueryUserDto>> QueryUsers([FromQuery] QueryUsersInput input)
        {
            var userList = new List<QueryUserDto>();
            var totalNumber = 0;
            if (input.RoleId > 0)
            {
                userList = _adminDbContext.Queryable<UserModel, UserRoleModel>((u, urole) => new object[] { JoinType.Inner, u.Id == urole.Uid })
                                          .Where((u, urole) => urole.RoleId == input.RoleId)
                                          .WhereIF(input.State > -1, (u, urole) => u.State == input.State)
                                          .WhereIF(!input.RealName.IsEmpty(), (u, urole) => u.RealName == input.RealName)
                                          .WhereIF(!input.UserName.IsEmpty(), (u, urole) => u.UserName == input.UserName)
                                          .WhereIF(!input.Mobile.IsEmpty(), (u, urole) => u.Mobile == input.Mobile)
                                          .Select((u, urole) => new QueryUserDto { Id = u.Id, Mobile = u.Mobile, RealName = u.RealName, State = u.State, UpdateTime = u.UpdateTime, UserName = u.UserName, Email = u.Email })
                                          .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            }
            else if (!input.PlatformKey.IsEmpty())
            {
                // 项目角色Id数组
                var roleIdList = _adminDbContext.Queryable<RoleModel>().Where(it => it.PlatformKey == input.PlatformKey && it.IsDel == false).Select(it => it.Id).ToList();
                // 查询
                userList = _adminDbContext.Queryable<UserModel, UserRoleModel>((u, urole) => new object[] { JoinType.Inner, u.Id == urole.Uid })
                                          .Where((u, urole) => roleIdList.Contains(urole.RoleId))
                                          .WhereIF(input.State > -1, (u, urole) => u.State == input.State)
                                          .WhereIF(!input.RealName.IsEmpty(), (u, urole) => u.RealName == input.RealName)
                                          .WhereIF(!input.UserName.IsEmpty(), (u, urole) => u.UserName == input.UserName)
                                          .WhereIF(!input.Mobile.IsEmpty(), (u, urole) => u.Mobile == input.Mobile)
                                          .GroupBy((u, urole) => u.Id)
                                          .Select((u, urole) => new QueryUserDto { Id = u.Id, Mobile = u.Mobile, RealName = u.RealName, State = u.State, UpdateTime = u.UpdateTime, UserName = u.UserName, Email = u.Email })
                                          .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            }
            else
            {
                userList = _adminDbContext.Queryable<UserModel>()
                                          .WhereIF(input.State > -1, f => f.State == input.State)
                                          .WhereIF(!input.RealName.IsEmpty(), f => f.RealName == input.RealName)
                                          .WhereIF(!input.UserName.IsEmpty(), f => f.UserName == input.UserName)
                                          .WhereIF(!input.Mobile.IsEmpty(), f => f.Mobile == input.Mobile)
                                          .Select(u => new QueryUserDto { Id = u.Id, Mobile = u.Mobile, RealName = u.RealName, State = u.State, UpdateTime = u.UpdateTime, UserName = u.UserName, Email = u.Email })
                                          .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            }
            // 当前所有用户id
            var userIdList = userList.Select(it => it.Id);
            // 查询用户及对应角色列表
            var userRoleList = _adminDbContext.Queryable<RoleModel, UserRoleModel>((role, urole) => new object[] { JoinType.Inner, role.Id == urole.RoleId })
                                              .Where((role, urole) => userIdList.Contains(urole.Uid))
                                              .Select((role, urole) => new { role.Id, role.Name, ProjectName = role.PlatformKey, urole.Uid })
                                              .ToList();
            // 组合用户对应角色
            userList.ForEach(m =>
            {
                m.RoleList = userRoleList.Where(it => it.Uid == m.Id);
            });
            return new BasePageOutput<List<QueryUserDto>> { Data = userList, CurrentPage = input.PageIndex, Total = totalNumber };
        }

        /// <summary>
        /// 设置用户信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/User/SetUser")]
        public BaseOutput SetUser([FromBody] SetUserInput input)
        {
            try
            {
                _adminDbContext.Ado.BeginTran();

                var model = _mapper.Map<SetUserInput, UserModel>(input);
                if (model.Id > 0)
                {
                    model.UpdateTime = DateTime.Now;
                    if (!model.Password.IsEmpty())
                    {
                        model.Salt = Randoms.CreateRandomValue(8, false);
                        model.Password = Encrypt.SHA256(model.Password + model.Salt);
                        // 基础字段不容许更新
                        _adminDbContext.Updateable(model)
                                       .IgnoreColumns(it => new { it.UserName, it.Mobile, it.CreateTime })
                                       .ExecuteCommand();
                    }
                    else
                    {
                        // 基础字段不容许更新
                        _adminDbContext.Updateable(model)
                                       .IgnoreColumns(it => new { it.UserName, it.Password, it.Salt, it.Mobile, it.CreateTime })
                                       .ExecuteCommand();
                    }
                }
                else
                {
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    model.Salt = Randoms.CreateRandomValue(8, false);
                    model.Password = Encrypt.SHA256(model.Password + model.Salt);
                    model.Id = Convert.ToInt64($"{Time.GetUnixTimestamp()}{ Randoms.CreateRandomValue(3, true) }");
                    _adminDbContext.Insertable(model).ExecuteCommand();
                }
                // 用户角色操作
                var userRoleList = new List<UserRoleModel>();
                foreach (var id in input.RoleIdList)
                {
                    // 防止重复数据
                    if (!userRoleList.Exists(it => it.RoleId == id))
                    {
                        userRoleList.Add(new UserRoleModel
                        {
                            Uid = model.Id,
                            RoleId = id
                        });
                    }
                }
                // 删除用户当前角色
                _adminDbContext.Deleteable<UserRoleModel>().Where(f => f.Uid == model.Id).ExecuteCommand();
                // 添加用户角色
                if (userRoleList.Count > 0)
                    _adminDbContext.Insertable(userRoleList).ExecuteCommand();

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