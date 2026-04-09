using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Celmah;

/// <summary>
///     Elmah Options
/// </summary>
public class CelmahOptions
{
    /// <summary>
    ///     Filters file path, example: options.LogPath = "~/celmah.xml"; // OR options.LogPath = "с:\celmah.xml"
    /// </summary>
    public string? FiltersConfig { get; set; }

    /// <summary>
    ///     Custom filters
    /// </summary>
    public ICollection<IErrorFilter> Filters { get; set; } = new List<IErrorFilter>();

    /// <summary>
    ///     Custom notifiers
    /// </summary>
    public ICollection<IErrorNotifier> Notifiers { get; set; } = new List<IErrorNotifier>();

    /// <summary>
    ///     Custom error handler
    /// </summary>
    public Func<HttpContext, Error, Task> OnError { get; set; } = (context, error) => Task.CompletedTask;

    /// <summary>
    ///     Application Name
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    ///     List of paths to sources
    /// </summary>
    public string[]? SourcePaths { get; set; }

    /// <summary>
    ///     Enable/Disable request body logging
    /// </summary>
    public bool LogRequestBody { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to log cookies sent with the request. (Default is <c>true</c>)
    /// </summary>
    public bool LogRequestCookies { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to log form values sent with the request. (Default is <c>true</c>)
    /// </summary>
    public bool LogRequestForm { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to show the Elmah error page when an error is raised.
    /// </summary>
    public bool ShowCelmahErrorPage { get; set; }

    /// <summary>
    ///     HTTP status codes to ignore when logging synthetic errors from response status codes.
    ///     For example, set to [404] to skip logging 404 Not Found errors.
    ///     Default is empty (log all status codes).
    /// </summary>
    public int[] IgnoredStatusCodes { get; set; } = [];

    /// <summary>
    ///     Gets or sets a value indicating whether to log SQL queries made during the request. (Default is <c>true</c>)
    /// </summary>
    public bool LogSqlQueries { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to log SQL parameter values for queries made during the request. (Default is <c>true</c>)
    /// </summary>
    public bool LogSqlQueryParameters { get; set; } = true;

    public virtual Task Error(HttpContext context, Error error) => OnError(context, error);
}