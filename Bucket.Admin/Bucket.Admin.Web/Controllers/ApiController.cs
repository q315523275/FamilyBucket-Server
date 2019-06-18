using AutoMapper;
using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Api;
using Bucket.Admin.Model.Setting;
using Bucket.DbContext.SqlSugar;
using Bucket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// WebApi接口资源管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class ApiController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly BucketSqlSugarClient _adminDbContext;
        private readonly IMapper _mapper;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="mapper"></param>
        public ApiController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IMapper mapper)
        {
            _mapper = mapper;
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }
        /// <summary>
        /// 查询Api资源
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Api/QueryApiList")]
        public BasePageOutput<object> QueryApiList([FromQuery] QueryApiListInput input)
        {
            var totalNumber = 0;
            var list = _adminDbContext.Queryable<ApiModel>()
                                 .WhereIF(!input.ProjectKey.IsEmpty(), it => it.ProjectName == input.ProjectKey)
                                 .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            return new BasePageOutput<object> { Data = list, CurrentPage = input.PageIndex, Total = totalNumber };
        }
        /// <summary>
        /// 设置Api资源
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Api/SetApi")]
        public BaseOutput SetApi([FromBody] SetApiInput input)
        {
            var model = _mapper.Map<ApiModel>(input);
            if (model.Id > 0)
            {
                model.UpdateTime = DateTime.Now;
                _adminDbContext.Updateable(model).IgnoreColumns(it => new { it.CreateTime }).ExecuteCommand();
            }
            else
            {
                model.UpdateTime = DateTime.Now;
                model.CreateTime = DateTime.Now;
                _adminDbContext.Insertable(model).ExecuteCommand();
            }
            return new BaseOutput { };
        }
    }
}