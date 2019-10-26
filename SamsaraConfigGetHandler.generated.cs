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
    public partial class GetSamsaraConfigRequest : IRequest<SamsaraConfigServiceModel>
    {
        public string Id { get; set; }
    }
    public class SamsaraConfigGetHandlerBase : IRequestHandler<GetSamsaraConfigRequest,SamsaraConfigServiceModel>
    {
        public BaseRepository<SamsaraConfig> Repo { get; }
        public IMapperService Mapper { get; }

        public SamsaraConfigGetHandlerBase(
            BaseRepository<SamsaraConfig> repo,
            IMapperService mapper
            )
        {
            Repo = repo;
            Mapper = mapper;
        }

        public virtual async Task<SamsaraConfigServiceModel> Handle(GetSamsaraConfigRequest request, CancellationToken cancellationToken)
        {
            return Mapper.MapTo<SamsaraConfig,SamsaraConfigServiceModel>(await Repo.Find(request.Id),2);
        }

        protected virtual Expression<Func<SamsaraConfig, bool>> GetFilter()
        {
            return null;
        }
    }
}
