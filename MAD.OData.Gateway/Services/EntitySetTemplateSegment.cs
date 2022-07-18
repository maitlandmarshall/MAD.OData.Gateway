using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace MAD.OData.Gateway.Services
{
    public class EntitySetTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/{entityset}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("entityset", out var entitySetRouteValue))
            {
                return false;
            }

            var entitySetName = entitySetRouteValue as string;

            // if you want to support case-insensitive
            var edmEntitySet = context.Model.EntityContainer.EntitySets()
                .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

            if (edmEntitySet != null)
            {
                var segment = new EntitySetSegment(edmEntitySet);
                context.Segments.Add(segment);
                return true;
            }

            return false;
        }
    }
}
