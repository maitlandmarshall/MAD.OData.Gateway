
using MAD.OData.Gateway.DynamicDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
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

        [EnableQuery(MaxTop = 1000, PageSize = 1000, EnsureStableOrdering = false)]
        public IActionResult Get(string entitySet)
        {
            var edmEntitySet = this.edmModel.FindDeclaredEntitySet(entitySet);

            if (edmEntitySet is null)
                return this.NotFound();

            var entityQueryable = this.dynamicDbContext.Query(entitySet).AsQueryable();

            return this.Ok(entityQueryable);
        }

        private ODataQueryOptions GetODataQueryOptions(IEdmEntitySet edmEntitySet)
        {
            var feature = this.Request.ODataFeature();
            var edmEntityType = edmEntitySet.EntityType();
            var queryContext = new ODataQueryContext(this.edmModel, edmEntityType, feature.Path);
            var queryOptions = new ODataQueryOptions(queryContext, this.Request);

            return queryOptions;
        }
    }
}
