using System.Threading.Tasks;
using BlockArray.Core.Services;
using BlockArray.ServiceModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;
using MediatR;
namespace FreightTrust.Modules.SamsaraConfig
{
    public class BaseSamsaraConfigController : Controller
    {
        public IMediator Mediator { get; }

        public BaseSamsaraConfigController( IMediator mediator )
        {
            Mediator = mediator;
        }
        

        

        

        
        [HttpGet("getSamsaraConfig")]
        [Produces(typeof(SamsaraConfigServiceModel))]
        [SwaggerOperation("getSamsaraConfig")]
        public async Task<IActionResult> GetSamsaraConfig(string id)
        {
            return Ok(await Mediator.Send(new GetSamsaraConfigRequest() { Id = id }));
        }
        

        
        [HttpPost("saveSamsaraConfig")]
        [Produces(typeof(SamsaraConfigServiceModel))]
        [SwaggerOperation("saveSamsaraConfig")]
        public async Task<IActionResult> SaveSamsaraConfig([FromBody] SamsaraConfigServiceModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await Mediator.Send(new SaveSamsaraConfigRequest() {
                ServiceModel = model
            }));
        }
        
        
    }
}
