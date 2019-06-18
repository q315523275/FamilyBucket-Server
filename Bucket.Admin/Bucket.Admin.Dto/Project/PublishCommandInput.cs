namespace Bucket.Admin.Dto.Project
{
    public class PublishCommandInput
    {
        public string ProjectName { set; get; }
        public string CommandType { set; get; }
        public string CommandText { set; get; }
    }
}
