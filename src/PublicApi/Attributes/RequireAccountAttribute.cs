using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class RequiresAccountAttribute : Attribute { }
