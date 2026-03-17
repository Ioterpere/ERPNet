using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace ERPNet.Infrastructure.Mailing;

public class RazorViewToStringRenderer(
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider)
{
    public async Task<string> RenderAsync(string viewName, object model)
    {
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var viewResult = viewEngine.GetView(
            executingFilePath: null,
            viewPath: $"/{viewName}.cshtml",
            isMainPage: true);

        if (!viewResult.Success)
        {
            var searchedLocations = string.Join("\n", viewResult.SearchedLocations);
            throw new InvalidOperationException(
                $"No se encontró la vista '{viewName}'. Ubicaciones buscadas:\n{searchedLocations}");
        }

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(httpContext, tempDataProvider);

        await using var writer = new StringWriter();
        var viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, writer, new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
