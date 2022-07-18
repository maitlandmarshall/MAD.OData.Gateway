
using MAD.OData.Gateway.DynamicDbContext;
using MAD.OData.Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using SqlKata;
using SqlKata.Execution;

namespace MAD.OData.Gateway.Controllers
{
    [Route("")]
    [ODataRouteComponent]
    public class GatewayODataController : ODataController
    {
        private readonly IEdmModel edmModel;
        private readonly SqlKataFactory sqlKataFactory;
        private readonly DbContext dynamicDbContext;

        public GatewayODataController(IEdmModel edmModel, SqlKataFactory sqlKataFactory, DbContext dynamicDbContext)
        {
            this.edmModel = edmModel;
            this.sqlKataFactory = sqlKataFactory;
            this.dynamicDbContext = dynamicDbContext;
        }

        [HttpGet("{entityset}")]
        [EnableQuery(MaxTop = 1000, PageSize = 1000)]
        public IActionResult Get(string entitySet)
        {
            var edmEntitySet = this.edmModel.FindDeclaredEntitySet(entitySet);

            if (edmEntitySet is null)
                return this.NotFound();

            var entityQueryable = this.dynamicDbContext.Query(entitySet).AsQueryable();

            return this.Ok(entityQueryable);
        }

        [HttpGet("{entityset}/$count")]
        public async Task<IActionResult> Count(string entitySet)
        {
            var edmEntitySet = this.edmModel.FindDeclaredEntitySet(entitySet);

            if (edmEntitySet is null)
                return this.NotFound();

            using var db = this.sqlKataFactory.Create();
            var query = this.GetQuery(db, edmEntitySet);

            return this.Ok(await query.CountAsync<int>());
        }

        private Query GetQuery(QueryFactory db, IEdmEntitySet entitySet)
        {
            var queryOptions = this.GetODataQueryOptions(entitySet);
            var query = db.Query(entitySet.Name);
            var top = Math.Max(Math.Min(queryOptions.Top?.Value ?? 1000, 1000), 0);

            query = query.Take(top);

            if (queryOptions.Skip != null)
            {
                query = query.Skip(Math.Max(queryOptions.Skip.Value, 0));
            }

            //if (queryOptions.Filter != null)
            //{
            //    var filter = queryOptions.Filter;


            //    ODataQuery
            //}

            return query;
        }

        private EdmEntityObjectCollection GetEdmEntityObjects(IEnumerable<IDictionary<string, object>> entities, IEdmEntitySet edmEntitySet)
        {
            var edmEntityType = edmEntitySet.EntityType();
            var collectionType = new EdmCollectionType(new EdmEntityTypeReference(edmEntityType, false));
            var result = new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType));

            foreach (var e in entities.ToList())
            {
                var edmEntityObject = new EdmEntityObject(edmEntityType);

                foreach (var kv in e)
                {
                    var prop = edmEntitySet.EntityType().FindProperty(kv.Key);

                    if (prop is null)
                        continue;

                    var value = kv.Value;

                    var finalValue = this.GetConvertedValueToEdmType(value, prop);
                    edmEntityObject.TrySetPropertyValue(kv.Key, finalValue);
                }

                result.Add(edmEntityObject);
            }

            return result;
        }

        private object? GetConvertedValueToEdmType(object? value, IEdmProperty prop)
        {
            if (value is null)
                return null;

            object finalValue;

            if (prop.Type.IsInt16())
            {
                finalValue = Convert.ChangeType(value, typeof(Int16));
            }
            else if (prop.Type.IsInt32())
            {
                finalValue = Convert.ChangeType(value, typeof(Int32));
            }
            else if (prop.Type.IsDateTimeOffset())
            {
                if (value is DateTime dt)
                {
                    // If the value is a DateTime (without a timezone) and the kind (utc, or local) is unspecified
                    // convert the value to a DateTimeOffset for UTC, so the OData layer doesn't translate the date time to the server's local timezone
                    if (dt.Kind == DateTimeKind.Unspecified)
                    {
                        value = new DateTimeOffset(dt, TimeSpan.Zero);
                    }
                }

                finalValue = value;
            }
            else
            {
                finalValue = value;
            }

            return finalValue;
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
