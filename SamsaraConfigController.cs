using System.Threading.Tasks;
using BlockArray.Core.Services;
using BlockArray.ServiceModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using MediatR;

namespace FreightTrust.Modules.SamsaraConfig
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class SamsaraConfigController : BaseSamsaraConfigController
    {
        public SamsaraConfigController(IMediator mediator) : base(mediator)
        {

        }
    }
}
