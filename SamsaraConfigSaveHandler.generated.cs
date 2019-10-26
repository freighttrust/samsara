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
    public partial class SaveSamsaraConfigRequest : IRequest<SamsaraConfigServiceModel>
    {
        public SamsaraConfigServiceModel ServiceModel { get; set; }
    }
    public class SamsaraConfigSaveHandlerBase : IRequestHandler<SaveSamsaraConfigRequest,SamsaraConfigServiceModel>
    {
        public BaseRepository<SamsaraConfig> Repo { get; }
        public IMapperService Mapper { get; }

        public SamsaraConfigSaveHandlerBase(
            BaseRepository<SamsaraConfig> repo,
            IMapperService mapper
            )
        {
            Repo = repo;
            Mapper = mapper;
        }

        public virtual async Task<SamsaraConfigServiceModel> Handle(SaveSamsaraConfigRequest request, CancellationToken cancellationToken)
        {
            SamsaraConfig target = null;
            if (string.IsNullOrEmpty(request.ServiceModel.Id))
            {
                // new
                target = new SamsaraConfig();
            }
            else
            {
                target = await Repo.Find(request.ServiceModel.Id);
            }

            if (target == null)
            {
                throw new Exception("Could not save the shipment with that id.  It was not found.");
            }
            var source = request.ServiceModel;
            target.Id = source.Id;
            
            
            target.Enabled = source.Enabled;
            
            
            
            target.ApiKey = source.ApiKey;
            
            
            
            target.GroupId = source.GroupId;
            
            
            
            target.EmailDomain = source.EmailDomain;
            
            
            
            target.SyncDrivers = source.SyncDrivers;
            
            
            
            target.PayrollType = source.PayrollType;
            
            
            
            target.PayrollRate = source.PayrollRate;
            
            
            
            target.InvoiceItemCategoryId = source.InvoiceItemCategoryId;
            
            
            await Apply(request.ServiceModel, target);
            // Save it
            await Repo.Save(target);
            return this.Mapper.MapTo<SamsaraConfig, SamsaraConfigServiceModel>(target);
        }
        protected virtual async Task Apply(SamsaraConfigServiceModel source , SamsaraConfig target )
        {

        }
        protected virtual Expression<Func<SamsaraConfig, bool>> GetFilter()
        {
            return null;
        }
    }
}
