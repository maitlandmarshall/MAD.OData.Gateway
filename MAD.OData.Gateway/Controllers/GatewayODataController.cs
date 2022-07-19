
using MAD.OData.Gateway.DynamicDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;

namespace MAD.OData.Gateway.Controllers
{
    [ODataRouteComponent]
    public class GatewayODataController : ODataController
    {
        private readonly IEdmModel edmModel;
        private readonly DbContext dynamicDbContext;

        public GatewayODataController(IEdmModel edmModel, DbContext dynamicDbContext)
        {
            this.edmModel = edmModel;
            this.dynamicDbContext = dynamicDbContext;
        }

        [EnableQuery(MaxTop = 1000, PageSize = 1000)]
        public IActionResult Get(string entitySet)
        {
            var edmEntitySet = this.edmModel.FindDeclaredEntitySet(entitySet);

            if (edmEntitySet is null)
                return this.NotFound();

            var entityQueryable = this.dynamicDbContext.Query(entitySet).AsQueryable();

            return this.Ok(entityQueryable);
        }
    }
}
