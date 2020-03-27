using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Logging;
using Bucket.Admin.Model.Logging;
using Bucket.DbContext.SqlSugar;
using Bucket.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 日志管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class LoggingController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly BucketSqlSugarClient _adminDbContext;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        public LoggingController(ISqlSugarDbContextFactory sqlSugarDbContextFactory)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }
        /// <summary>
        /// 查询日志列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Api/QueryLogList")]
        public BasePageOutput<object> QueryLogList([FromQuery] QueryLogListInput input)
        {
            var totalNumber = 0;
            var list = _adminDbContext.Queryable<LogModel>()
                                 .WhereIF(!input.ServiceName.IsEmpty(), it => it.ProjectName == input.ServiceName)
                                 .WhereIF(!input.Level.IsEmpty(), it => it.LogType == input.Level)
                                 .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            return new BasePageOutput<object> { Data = list, CurrentPage = input.PageIndex, Total = totalNumber };
        }
    }
}