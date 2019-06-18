namespace Bucket.Admin.Dto.Microservice
{
    public class QueryApiGatewayReRouteListInput : BasePageInput
    {
        public int GatewayId { set; get; }
        public int State { set; get; }
    }
}
