using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Project;
using Bucket.Admin.Model.Setting;
using Bucket.Core;
using Bucket.DbContext.SqlSugar;
using Bucket.Listener.Abstractions;
using Bucket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 项目管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class ProjectController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly IMapper _mapper;
        private readonly BucketSqlSugarClient _adminDbContext;
        private readonly IPublishCommand _networkCommand;
        private readonly IUser _user;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="mapper"></param>
        /// <param name="networkCommand"></param>
        /// <param name="user"></param>
        public ProjectController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper, IPublishCommand networkCommand, IUser user)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _mapper = mapper;
            _networkCommand = networkCommand;
            _user = user;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }

        /// <summary>
        /// 查看项目列表信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Project/QueryProject")]
        public BaseOutput<object> QueryProject()
        {
            var list = _adminDbContext.Queryable<ProjectModel>().ToList();
            return new BaseOutput<object> { Data = list };
        }
        /// <summary>
        /// 设置项目信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/SetProject")]
        public BaseOutput SetProject([FromBody] SetProjectInput input)
        {
            var model = _mapper.Map<SetProjectInput, ProjectModel>(input);
            if (model.Id > 0)
            {
                // 基础字段不容许更新
                model.LastTime = DateTime.Now;
                model.LastUid = _user.Id.ToLong();
                _adminDbContext.Updateable(model)
                               .IgnoreColumns(it => new { it.CreateUid, it.CreateTime })
                               .ExecuteCommand();
            }
            else
            {
                model.CreateTime = DateTime.Now;
                model.CreateUid = _user.Id.ToLong();
                model.LastTime = DateTime.Now;
                model.LastUid = model.CreateUid;
                _adminDbContext.Insertable(model)
                               .ExecuteCommand();
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 推送项目信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/PublishCommand")]
        public async Task<BaseOutput> PublishCommand([FromBody] PublishCommandInput input)
        {
            await _networkCommand.PublishCommandMessage(input.ProjectName, new Bucket.Values.NetworkCommand { CommandText = input.CommandText, NotifyComponent = input.CommandType });
            return new BaseOutput { };
        }
    }

}