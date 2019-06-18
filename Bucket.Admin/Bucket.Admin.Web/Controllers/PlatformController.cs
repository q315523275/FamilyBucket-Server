using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Platform;
using Bucket.Admin.Model.Setting;
using Bucket.DbContext.SqlSugar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 平台管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class PlatformController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly BucketSqlSugarClient _adminDbContext;
        private readonly IMapper _mapper;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="mapper"></param>
        public PlatformController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _mapper = mapper;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }

        /// <summary>
        /// 查询平台列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Platform/QueryPlatforms")]
        public BaseOutput<object> QueryPlatforms()
        {
            var list = _adminDbContext.Queryable<PlatformModel>().OrderBy(it => it.SortId, OrderByType.Asc).ToList();
            return new BaseOutput<object> { Data = list };
        }
        /// <summary>
        /// 设置平台信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Platform/SetPlatform")]
        public BaseOutput SetPlatform([FromBody] SetPlatformInput input)
        {
            var model = _mapper.Map<PlatformModel>(input);
            if (model.Id > 0)
            {
                _adminDbContext.Updateable(model).ExecuteCommand();
            }
            else
            {
                model.AddTime = DateTime.Now;
                model.IsDel = false;
                _adminDbContext.Insertable(model).ExecuteCommand();
            }
            return new BaseOutput { };
        }
    }
}