using System;

namespace Bucket.Admin.Dto.Api
{
    public class SetApiInput
    {
        /// <summary>
        /// 资源唯一标示符
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        [NotEmpty("platform001", "项目名称不能为空")]
        public string ProjectName { get; set; }
        /// <summary>
        /// 资源Uri
        /// </summary>
        [NotEmpty("platform001", "接口Uri不能为空")]
        public string Url { get; set; }
        /// <summary>
        /// 请求方式(GET,POST)
        /// </summary>
        [NotEmpty("platform001", "请求方式不能为空")]
        public string Method { get; set; }
        /// <summary>
        /// 资源描述
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 控制器
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string ControllerName { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// 是否允许匿名访问
        /// </summary>
        public int AllowScope { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
