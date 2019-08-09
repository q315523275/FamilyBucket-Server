namespace Bucket.Identity.IServices.Dto
{
    public class UdcBaseOutput<T>
    {
        public bool success { set; get; }
        public T data { set; get; }
    }
}
