using MAD.OData.Gateway.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace MAD.OData.Gateway.Services
{
    public class ODataApplicationModelProvider : IApplicationModelProvider
    {
        private readonly IEdmModel edmModel;

        public ODataApplicationModelProvider(IEdmModel edmModel)
        {
            this.edmModel = edmModel;
        }

        public int Order { get => 0; }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            foreach (var c in context.Result.Controllers)
            {
                if (c.ControllerType.UnderlyingSystemType != typeof(GatewayODataController))
                    continue;

                foreach (var a in c.Actions)
                {
                    ODataPathTemplate path;

                    if (a.ActionName == nameof(GatewayODataController.Get))
                    {
                        path = new ODataPathTemplate(new GenericODataSegmentTemplate("/{entityset}"));
                    }
                    else
                    {
                        continue;
                    }

                    a.Selectors.Clear();
                    a.AddSelector("get", string.Empty, this.edmModel, path);
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {

        }
    }
}
