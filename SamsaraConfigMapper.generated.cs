using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArray.Core.Data;
using BlockArray.Core.Mapping;
using BlockArray.Core.Mongo;
using BlockArray.Core.Services;
using BlockArray.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
namespace FreightTrust.Modules.SamsaraConfig
{
    public static class SamsaraConfigExtensions {
        public static SamsaraConfigServiceModel MapSamsaraConfig(this IMapperService mapper, SamsaraConfig model, SamsaraConfigServiceModel serviceModel = null) {
            serviceModel = serviceModel ?? new SamsaraConfigServiceModel();
            mapper.MapTo<SamsaraConfig,SamsaraConfigServiceModel>(model,serviceModel);
            return serviceModel;
        }
    }
    public abstract class BaseSamsaraConfigMapper : IMapper<SamsaraConfig, SamsaraConfigServiceModel>, IFidelityMapper
    {
        public int Fidelity { get; set; }
        public IServiceProvider ServiceProvider { get;set; }

        public virtual SamsaraConfigServiceModel Map(SamsaraConfig source, SamsaraConfigServiceModel target)
        {
            if (source == null) return target;
            var mapper = ServiceProvider.GetService<IMapperService>();
            target.Id = source.Id;

            
            
            target.Enabled = source.Enabled;
            
            
            target.ApiKey = source.ApiKey;
            
            
            target.GroupId = source.GroupId;
            
            
            target.EmailDomain = source.EmailDomain;
            
            
            target.SyncDrivers = source.SyncDrivers;
            
            
            target.PayrollType = source.PayrollType;
            
            
            target.PayrollRate = source.PayrollRate;
            
            

            if (source.InvoiceItemCategoryId != null) {
                    target.InvoiceItemCategoryId = source.InvoiceItemCategoryId;
                     if (Fidelity > 0) {
                        target.InvoiceItemCategory = mapper.MapTo<FreightTrust.Modules.InvoiceItemCategory.InvoiceItemCategory, InvoiceItemCategoryServiceModel>(
                                ServiceProvider.GetService<BaseRepository<FreightTrust.Modules.InvoiceItemCategory.InvoiceItemCategory>>().Find(source.InvoiceItemCategoryId).Result, 0); // 0 So no cascading issues
                    }
            }
            
            
            MapMore( source, target );
            return target;
        }
        public abstract void MapMore(SamsaraConfig source, SamsaraConfigServiceModel target);

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }

}
