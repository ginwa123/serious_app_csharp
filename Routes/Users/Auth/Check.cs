
using App.Constants;
using App.Models;

namespace App.Routes.Users.Auth;

public static class AuthCheckRoute
{

    public static object Do(
        HttpContext context
   )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();

        return new Response<object>()
        {
            Message = BasicConstants.SUCCCESS,
            Data = null,
            Log = logId!
        };
    }
}