using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Config;
using Bucket.Admin.Infrastructure;
using Bucket.Admin.Model.Config;
using Bucket.Core;
using Bucket.DbContext.SqlSugar;
using Bucket.Listener.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 配置中心管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class ConfigController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly IMapper _mapper;
        private readonly IUser _user;
        private readonly BucketSqlSugarClient _adminDbContext;
        private readonly IConfigService _configService;
        private readonly IPublishCommand _networkCommand;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="mapper"></param>
        /// <param name="user"></param>
        /// <param name="configService"></param>
        /// <param name="networkCommand"></param>
        public ConfigController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper, IUser user, IConfigService configService, IPublishCommand networkCommand)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _mapper = mapper;
            _user = user;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
            _configService = configService;
            _networkCommand = networkCommand;
        }
        /// <summary>
        /// 查看所有项目组
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Config/QueryAppList")]
        public BaseOutput<dynamic> QueryAppList()
        {
            var list = _adminDbContext.Queryable<AppModel>().ToList();
            return new BaseOutput<dynamic> { Data = list };
        }
        /// <summary>
        /// 设置项目组信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Config/SetAppInfo")]
        public BaseOutput SetAppInfo([FromBody] SetAppInfoInput input)
        {
            var model = _mapper.Map<SetAppInfoInput, AppModel>(input);
            if (model.Id > 0)
            {
                // 基础字段不容许更新
                _adminDbContext.Updateable(model).ExecuteCommand();
            }
            else
            {
                _adminDbContext.Insertable(model).ExecuteCommand();
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 查询配置项目
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Config/QueryAppProjectList")]
        public BasePageOutput<object> QueryAppProjectList([FromQuery] QueryAppProjectListInput input)
        {
            var totalNumber = 0;
            var list = _adminDbContext.Queryable<AppNamespaceModel>()
                                .WhereIF(!string.IsNullOrWhiteSpace(input.AppId), it => it.AppId == input.AppId)
                                .WhereIF(input.IsPublic == 1, it => it.IsPublic == true)
                                .WhereIF(input.IsPublic == 0, it => it.IsPublic == false)
                                .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            return new BasePageOutput<object> { Data = list, CurrentPage = input.PageIndex, Total = totalNumber };
        }
        /// <summary>
        /// 设置配置项目
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Config/SetAppProjectInfo")]
        public BaseOutput SetAppProjectInfo([FromBody] SetAppProjectInfoInput input)
        {
            var model = _mapper.Map<SetAppProjectInfoInput, AppNamespaceModel>(input);
            if (model.Id > 0)
            {
                // 基础字段不容许更新
                model.LastTime = DateTime.Now;
                model.LastUid = Convert.ToInt64(_user.Id);
                _adminDbContext.Updateable(model)
                               .IgnoreColumns(it => new { it.CreateUid, it.CreateTime })
                               .ExecuteCommand();
            }
            else
            {
                model.CreateTime = DateTime.Now;
                model.CreateUid = Convert.ToInt64(_user.Id);
                model.LastTime = DateTime.Now;
                model.LastUid = model.CreateUid;
                _adminDbContext.Insertable(model)
                               .ExecuteCommand();
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 查询配置信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Config/QueryAppConfigList")]
        public BasePageOutput<object> QueryAppConfigList([FromQuery] QueryAppConfigListInput input)
        {
            var totalNumber = 0;
            // 环境库
            var tableName = _configService.GetConfigTableName(input.Environment);
            // 执行
            var lst = _adminDbContext.Queryable<AppConfigModel>().AS(tableName)
                                     .WhereIF(!string.IsNullOrWhiteSpace(input.AppId), it => it.ConfigAppId == input.AppId)
                                     .WhereIF(!string.IsNullOrWhiteSpace(input.NameSpace), it => it.ConfigNamespaceName == input.NameSpace)
                                     .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            return new BasePageOutput<object> { Data = lst, CurrentPage = input.PageIndex, Total = totalNumber };
        }
        /// <summary>
        /// 设置配置信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Config/SetAppConfigInfo")]
        public BaseOutput SetAppConfigInfo([FromBody] SetAppConfigInfoInput input)
        {
            // 环境库
            var tableName = _configService.GetConfigTableName(input.Environment);
            // 编辑或新增
            var model = _mapper.Map<AppConfigModel>(input);
            if (model.Id > 0)
            {
                // 基础字段不容许更新
                model.LastTime = DateTime.Now;
                model.Version = _adminDbContext.Queryable<AppConfigModel>().AS(tableName).Max(it => it.Version) + 1;
                _adminDbContext.Updateable(model).AS(tableName)
                               .IgnoreColumns(it => new { it.CreateTime })
                               .ExecuteCommand();
            }
            else
            {
                model.CreateTime = DateTime.Now;
                model.LastTime = DateTime.Now;
                model.Version = _adminDbContext.Queryable<AppConfigModel>().AS(tableName).Max(it => it.Version) + 1;
                _adminDbContext.Insertable(model).AS(tableName).ExecuteCommand();
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 推送配置中心网络命令
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Config/PublishConfigNetworkCommand")]
        public async Task<BaseOutput> PublishConfigNetworkCommand([FromBody] PublishConfigNetworkCommandInput input)
        {
            await _networkCommand.PublishCommandMessage(input.ProjectName, new Values.NetworkCommand { CommandText = input.CommandText, NotifyComponent = input.CommandType });
            return new BaseOutput { };
        }
    }
}