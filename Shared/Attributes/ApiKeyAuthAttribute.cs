using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute
{
}