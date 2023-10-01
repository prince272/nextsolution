using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using NextSolution.Core.Extensions.ViewRenderer;

namespace NextSolution.Infrastructure.ViewRenderer.Razor
{
    public class RazorViewRenderer : IViewRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<string> RenderAsync(string name, object? model, CancellationToken cancellationToken = default)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, name, isMainPage: true);
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };
            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            await using var output = new StringWriter();
            var viewContext = new ViewContext(actionContext, view, viewData, tempData, output, new HtmlHelperOptions());
            await view.RenderAsync(viewContext);
            return output.ToString();
        }

        private IView FindView(ActionContext actionContext, string viewName, bool isMainPage)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(viewName)))
                viewName += ".cshtml";

            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new string[] {
                    $"Unable to find view '{viewName}'. The following locations were searched:"
                }.Concat(searchedLocations));

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext()
            {
                RequestServices = _serviceProvider
            };
            //var baseUri = new Uri(baseUrl);
            //httpContext.Request.Scheme = baseUri.Scheme;
            //httpContext.Request.Host = HostString.FromUriComponent(baseUri);

            var app = new ApplicationBuilder(_serviceProvider);
            var routeBuilder = new RouteBuilder(app)
            {
                DefaultHandler = new ViewRouter()
            };

            routeBuilder.MapRoute(
                string.Empty,
                "{controller}/{action}/{id}",
                new RouteValueDictionary(new { id = "defaultid" }));

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            actionContext.RouteData.Routers.Add(routeBuilder.Build());
            return actionContext;
        }

        private class ViewRouter : IRouter
        {
            public VirtualPathData? GetVirtualPath(VirtualPathContext context)
            {
                return null;
            }

            public Task RouteAsync(RouteContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
