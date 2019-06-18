namespace Bucket.Admin.Dto.Config
{
    public class QueryAppProjectListInput : BasePageInput
    {
        public string AppId { set; get; }
        public int IsPublic { set; get; } = -1;
    }
}
