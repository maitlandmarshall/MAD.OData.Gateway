using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.OData.Edm;

namespace MAD.OData.Gateway.Services
{
    public class ODataMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IEdmModel edmModel;
        private readonly IODataTemplateTranslator templateTranslator;

        public ODataMatcherPolicy(IEdmModel edmModel, IODataTemplateTranslator templateTranslator)
        {
            this.edmModel = edmModel;
            this.templateTranslator = templateTranslator;
        }

        public override int Order { get => 0; }

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return endpoints.Any(e => e.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() != null);
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            var feature = httpContext.ODataFeature();

            if (feature.Path != null)
                return Task.CompletedTask;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];

                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                var metadata = candidate.Endpoint.Metadata.OfType<IODataRoutingMetadata>().FirstOrDefault();

                if (metadata == null)
                {
                    continue;
                }

                var translatorContext = new ODataTemplateTranslateContext(httpContext, candidate.Endpoint, candidate.Values, this.edmModel);

                try
                {
                    var odataPath = this.templateTranslator.Translate(metadata.Template, translatorContext);

                    if (odataPath != null)
                    {
                        feature.RoutePrefix = metadata.Prefix;
                        feature.Model = this.edmModel;
                        feature.Path = odataPath;

                        this.MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                    }
                    else
                    {
                        candidates.SetValidity(i, false);
                    }
                }
                catch
                {
                }
            }

            return Task.CompletedTask;
        }

        private void MergeRouteValues(RouteValueDictionary updates, RouteValueDictionary? source)
        {
            if (source is null)
                return;

            foreach (var data in updates)
            {
                source[data.Key] = data.Value;
            }
        }
    }
}
