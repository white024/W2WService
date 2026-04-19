//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Shared.Attributes;

//namespace AuthService.Filters;

//public class ApiKeyAuthFilter : IAsyncActionFilter
//{
//    private readonly IConfiguration _config;

//    public ApiKeyAuthFilter(IConfiguration config)
//    {
//        _config = config;
//    }

//    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//    {
//        var hasAttribute = context.ActionDescriptor.EndpointMetadata
//            .Any(x => x is ApiKeyAuthAttribute);

//        if (!hasAttribute)
//        {
//            await next();
//            return;
//        }

//        var key = context.HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
//        var valid = _config["Security:AdminApiKey"];

//        if (string.IsNullOrEmpty(key) || key != valid)
//        {
//            context.Result = new UnauthorizedObjectResult("Invalid API Key");
//            return;
//        }

//        await next();
//    }
//}