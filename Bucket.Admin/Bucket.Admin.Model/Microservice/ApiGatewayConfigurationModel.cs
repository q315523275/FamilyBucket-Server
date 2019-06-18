using SqlSugar;

namespace Bucket.Admin.Model.Microservice
{
    [SugarTable("tb_apigateway_config")]
    public class ApiGatewayConfigurationModel
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int GatewayId { set; get; }
        public string GatewayKey { set; get; }
        public string BaseUrl { set; get; }
        public string DownstreamScheme { set; get; }
        public string RequestIdKey { set; get; }
        public string HttpHandlerOptions { set; get; }
        public string LoadBalancerOptions { set; get; }
        public string QoSOptions { set; get; }
        public string ServiceDiscoveryProvider { set; get; }
        public string RateLimitOptions { set; get; }
    }
}
