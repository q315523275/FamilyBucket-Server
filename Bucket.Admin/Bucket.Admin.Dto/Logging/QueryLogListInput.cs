namespace Bucket.Admin.Dto.Logging
{
    public class QueryLogListInput: BasePageInput
    {
        public string ServiceName { set; get; }
        public string Level { set; get; }
    }
}
