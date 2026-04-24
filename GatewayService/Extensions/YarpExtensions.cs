using Microsoft.Extensions.DependencyInjection;  
using Yarp.ReverseProxy.Configuration;           

namespace GatewayService.Extensions;

public static class YarpExtensions
{
    public static IReverseProxyBuilder AddSharedLoadBalancing(this IReverseProxyBuilder builder)
    {
      
        return builder;
    }
}