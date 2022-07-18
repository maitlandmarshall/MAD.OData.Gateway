
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;

namespace MAD.OData.Gateway.Controllers
{
    [Route("")]
    public class GatewayODataController : ODataController
    {
        private readonly IEdmModel edmModel;

        public GatewayODataController(IEdmModel edmModel)
        {
            this.edmModel = edmModel;
        }

        [EnableQuery]
        [HttpGet("{entityset}")]
        public IActionResult Get(string entitySet)
        {
            var edmEntitySet = this.edmModel.FindDeclaredEntitySet(entitySet);

            if (edmEntitySet is null)
                return this.NotFound();


            
            


            return Ok();
        }
    }
}
