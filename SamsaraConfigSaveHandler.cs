using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BlockArray.Core.Data;
using BlockArray.Core.Mapping;
using BlockArray.Core.Services;
using BlockArray.Model.Mongo;
using BlockArray.ServiceModel;
using MediatR;

namespace FreightTrust.Modules.SamsaraConfig
{
    public class SamsaraConfigSaveHandler : SamsaraConfigSaveHandlerBase  {
        public SamsaraConfigSaveHandler(
                BaseRepository<SamsaraConfig> repo,
                IMapperService mapper
                ) : base(repo, mapper)
        {

        }
        protected override async Task Apply( SamsaraConfigServiceModel source , SamsaraConfig target )
        {
            if (source == null) return;
            // Do Additional saving things here
        }
    }
}
