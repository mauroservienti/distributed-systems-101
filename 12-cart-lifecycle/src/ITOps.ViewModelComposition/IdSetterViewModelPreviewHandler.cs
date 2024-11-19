using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using System.Threading.Tasks;

namespace ITOps.ViewModelComposition
{
    class IdSetterViewModelPreviewHandler : IViewModelPreviewHandler
    {
        public Task Preview(HttpRequest request)
        {
            var viewModel = request.GetComposedResponseModel();
            var routeData = request.HttpContext.GetRouteData();
            if (routeData.Values.TryGetValue("id", out var value))
            {
                var id = (string)value;
                viewModel.Id = id;
            }

            return Task.CompletedTask;
        }
    }
}