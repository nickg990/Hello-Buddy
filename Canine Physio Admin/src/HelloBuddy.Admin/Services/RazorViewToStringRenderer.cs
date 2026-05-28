using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace HelloBuddy.Admin.Services;

/// <summary>
/// Renders a Razor view (with model) to an HTML string. Used by the publish
/// flow so the same view feeds the in-page preview and the PuppeteerSharp PDF.
/// </summary>
public interface IRazorViewToStringRenderer
{
    Task<string> RenderAsync<TModel>(string viewName, TModel model, CancellationToken ct = default);
}

public sealed class RazorViewToStringRenderer : IRazorViewToStringRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _services;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RazorViewToStringRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider services,
        IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _services = services;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> RenderAsync<TModel>(string viewName, TModel model, CancellationToken ct = default)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? new DefaultHttpContext { RequestServices = _services };
        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor());

        var viewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: false);
        if (!viewResult.Success)
        {
            var searched = string.Join(", ", viewResult.SearchedLocations);
            throw new InvalidOperationException($"Razor view '{viewName}' not found. Searched: {searched}");
        }

        using var sw = new StringWriter();
        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

        var viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, sw, new HtmlHelperOptions());
        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
