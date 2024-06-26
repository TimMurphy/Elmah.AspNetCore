﻿#if USE_GLOBAL_ERROR_HANDLING
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Elmah.AspNetCore;

internal class ElmahExceptionHandler : IExceptionHandler
{
    private readonly IElmahExceptionLogger _elmahLogger;
    private readonly IOptions<ElmahOptions> _elmahOptions;

    public ElmahExceptionHandler(IElmahExceptionLogger elmahLogger, IOptions<ElmahOptions> elmahOptions)
    {
        _elmahLogger = elmahLogger;
        _elmahOptions = elmahOptions;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var entry = await _elmahLogger.LogExceptionAsync(httpContext, exception);

        string? location = null;
        if (entry is not null)
        {
            location = $"{httpContext.GetElmahRelativeRoot()}/detail/{entry.Id}";
            httpContext.Features.Set<IElmahFeature>(new ElmahFeature(entry.Id, location));
        }

        if (!string.IsNullOrEmpty(location) && _elmahOptions.Value.ShowElmahErrorPage)
        {
            httpContext.Response.StatusCode = ErrorFactory.GetStatusCodeFromExceptionOr500(exception);
            if (httpContext.RequestAcceptsHtml())
            {
                httpContext.Response.Redirect(location);
            }

            return true;
        }

        return false;
    }
}
#endif