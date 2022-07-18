
using MAD.OData.Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using SqlKata;
using SqlKata.Execution;

namespace MAD.OData.Gateway.Controllers
{
    [Route("")]
    public class GatewayODataController : ODataController
    {
        private readonly IEdmModel edmModel;
        private readonly SqlKataFactory sqlKataFactory;

        public GatewayODataController(IEdmModel edmModel, SqlKataFactory sqlKataFactory)
        {
            this.edmModel = edmModel;
            this.sqlKataFactory = sqlKataFactory;
        }

        [HttpGet("{entityset}")]
        public async Task<IActionResult> Get(string entitySet)
        {
            var edmEntitySet = this.edmModel.FindDeclaredEntitySet(entitySet);

            if (edmEntitySet is null)
                return this.NotFound();

            using var db = this.sqlKataFactory.Create();
            var query = this.GetQuery(db, edmEntitySet);

            var results = (await query.GetAsync()).Cast<IDictionary<string, object>>().ToList();

            return this.Ok(this.GetEdmEntityObjects(results, edmEntitySet));
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
            var queryOptions = GetODataQueryOptions(entitySet);
            var query = db.Query(entitySet.Name);

            if (queryOptions.Top != null)
            {
                query = query.Take(queryOptions.Top.Value);
            }

            if (queryOptions.Skip != null)
            {
                query = query.Skip(queryOptions.Skip.Value);
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
