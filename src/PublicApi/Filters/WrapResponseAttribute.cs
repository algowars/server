using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PublicApi.Filters;

public class WrapResponseAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            if (objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
            {
                if (!IsAlreadyWrapped(objectResult.Value))
                {
                    context.Result = new JsonResult(new { data = objectResult.Value })
                    {
                        StatusCode = objectResult.StatusCode,
                    };
                }
            }
        }

        base.OnResultExecuting(context);
    }

    private static bool IsAlreadyWrapped(object value)
    {
        var type = value.GetType();
        return type.GetProperty("data") != null;
    }
}
