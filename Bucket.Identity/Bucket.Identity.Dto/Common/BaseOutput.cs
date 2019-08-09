namespace Bucket.Identity.Dto
{
    /// <summary>
    /// 返回值
    /// </summary>
    public class BaseOutput
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public BaseOutput()
        {
            this.ErrorCode = "000000";
            this.Message = "操作成功";
        }
        /// <summary>
        /// 错误码
        /// </summary>
        public string ErrorCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Message { get; set; }
    }
}
