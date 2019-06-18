namespace Bucket.Admin.Dto
{
    /// <summary>
    /// 返回基类
    /// </summary>
    public class BaseOutput
    {
        public BaseOutput()
        {
            this.ErrorCode = "000000";
            this.Message = "操作成功";
        }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }
    /// <summary>
    /// 返回基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseOutput<T> : BaseOutput
    {
        public T Data { set; get; }
    }
}
