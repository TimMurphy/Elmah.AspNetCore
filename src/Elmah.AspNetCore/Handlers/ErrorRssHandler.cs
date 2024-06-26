using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Elmah.AspNetCore.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elmah.AspNetCore.Handlers;

/// <summary>
///     Renders a XML using the RSS 0.91 vocabulary that displays, at most,
///     the 15 most recent errors recorded in the error log.
/// </summary>
internal static partial class Endpoints
{
    public static IEndpointConventionBuilder MapRss(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(async ([FromServices] ErrorLog errorLog, HttpContext context) =>
        {
            const int pageSize = 15;
            var entries = new List<ErrorLogEntry>(pageSize);
            var log = errorLog;
            await log.GetErrorsAsync(ErrorLogFilterCollection.Empty, 0, pageSize, entries);

            var title = $@"Error log of {log.ApplicationName} on {Environment.MachineName}";

            var link = context.GetElmahAbsoluteRoot();
            var baseUrl = new Uri(link.TrimEnd('/') + "/");

            var items =
                from entry in entries
                let error = entry.Error
                select RssXml.Item(
                    error.Message,
                    "An error of type " + error.Type + " occurred. " + error.Message,
                    error.Time,
                    baseUrl + "detail?id=" + Uri.EscapeDataString(entry.Id.ToString()));

            var rss = RssXml.Rss(title, link, "AddMessage of recent errors", items);
            return Results.Content(XmlText.StripIllegalXmlCharacters(rss.ToString()), MediaTypeNames.Application.Xml);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapGet($"{prefix}/rss", pipeline.Build());
    }
}