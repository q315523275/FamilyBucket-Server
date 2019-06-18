namespace Bucket.Config.Dto
{
    /// <summary>
    /// 通用返回基类
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
    /// 通用返回基类
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class BaseOutput<TData> : BaseOutput
    {
        public TData Data { set; get; }
    }
}
