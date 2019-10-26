using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArray.Core.Data;
using BlockArray.Core.Mapping;
using BlockArray.Core.Mongo;
using BlockArray.Core.Services;
using BlockArray.ServiceModel;
using MongoDB.Bson.Serialization.Attributes;
namespace FreightTrust.Modules.SamsaraConfig
{
    [BsonIgnoreExtraElements]
    public partial class SamsaraConfig : BaseMongoDocument
    {
        
        public System.Boolean Enabled {get;set;}
        
        public System.String ApiKey {get;set;}
        
        public System.String GroupId {get;set;}
        
        public System.String EmailDomain {get;set;}
        
        public System.Boolean SyncDrivers {get;set;}
        
        public BlockArray.ServiceModel.DriverPayrollType PayrollType {get;set;}
        
        public System.Decimal PayrollRate {get;set;}
        
        public string InvoiceItemCategoryId {get;set;}
        
        
    }

}
