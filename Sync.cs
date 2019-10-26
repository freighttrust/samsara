using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlockArray.Core;
using BlockArray.Core.Data;
using BlockArray.Core.Mongo;
using BlockArray.Model.Mongo;
using BlockArray.Seal;
using BlockArray.Seal.Identity.Models;
using BlockArray.ServiceModel;
using FreightTrust.Modules.Driver;
using FreightTrust.Modules.SamsaraConfig.Api;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FreightTrust.Modules.SamsaraConfig
{
    public interface ITrackingProvider
    {
        TrackingProvider Type { get; }
        Task UpdateLocation(Shipment.Shipment item, Driver.Driver driver);
    }

 
    
    public class DisabledLocationProvider : ITrackingProvider
    {
        public TrackingProvider Type => TrackingProvider.None;
        
        public async Task UpdateLocation(Shipment.Shipment item, Driver.Driver driver)
        {
            
        }
    }
    public class SamsaraLocationProvider : ITrackingProvider
    {
        public TrackingProvider Type => TrackingProvider.Samsara;
        
        public async Task UpdateLocation(Shipment.Shipment item, Driver.Driver driver)
        {
            
        }
    }

    public class MobileLocationProvider : ITrackingProvider
    {
        private readonly IPushNotifier _pushNotifier;
        public TrackingProvider Type => TrackingProvider.Mobile;

        public MobileLocationProvider(IPushNotifier pushNotifier)
        {
            _pushNotifier = pushNotifier;
        }

        public async Task UpdateLocation(Shipment.Shipment item, Driver.Driver driver)
        {
            var result = await _pushNotifier.SendMessage($"user-{driver.UserId}", 
                "Shipment Tracking Needed", 
                "Open to send location.",new NotificationData()
                {
                    action = "SendTrackingInfo",
                    shipmentId = item.Id,
                    click_action = "FLUTTER_NOTIFICATION_CLICK"
                });
        }
    }
    
    public abstract class Sync
    {
        private readonly BaseRepository<Company.Company> _company;
        private readonly ITenancyContextProvider _tenancyContext;
        private readonly IServiceProvider _serviceProvider;

        public Sync(BaseRepository<Company.Company> company, ITenancyContextProvider tenancyContext,
            IServiceProvider serviceProvider)
        {
            _company = company;
            _tenancyContext = tenancyContext;
            _serviceProvider = serviceProvider;
        }

        public async Task Process()
        {
            var companies = _company.Get().Where(p => p.IsEnabled).ToArray();
            foreach (var c in companies)
            {
                using (var sp = _tenancyContext.SetTenant(c.Id))
                {
                    await ProcessCompany(c, sp.ServiceProvider);
                }
            }
        }

        public abstract Task ProcessCompany(Company.Company company, IServiceProvider sp);
    }

    public class SyncSamsara : Sync
    {
        private readonly ICompanyAccountService _companyAccountService;

        public SyncSamsara(BaseRepository<Company.Company> company,
            ITenancyContextProvider tenancyContext,
            ICompanyAccountService companyAccountService,
            IServiceProvider serviceProvider) : base(company, tenancyContext, serviceProvider)
        {
            _companyAccountService = companyAccountService;
        }

        public override async Task ProcessCompany(Company.Company company, IServiceProvider sp)
        {
            var driverRepo = sp.GetService<BaseRepository<Driver.Driver>>();
            var vehicleRepo = sp.GetService<BaseRepository<Vehicle.Vehicle>>();
            var userRepo = sp.GetService<BaseRepository<ApplicationUser>>();
            var safetyRepo = sp.GetService<BaseRepository<DriverSafety.DriverSafety>>();
            var config = sp.GetService<Config<SamsaraConfig>>();
            var mediator = sp.GetService<IMediator>();
            var samsaraApi = new SamsaraApi(config.Cached.ApiKey, config.Cached.GroupId);
            if (config.Cached.Enabled == false || config.Cached.ApiKey == null || config.Cached.GroupId == null) return;

            if (config.Cached.SyncDrivers)
            {
                var vehicles = samsaraApi.GetFleetList();
                foreach (var item in vehicles)
                {
                    var v = vehicleRepo.Get().FirstOrDefault(p => p.ExternalIds.SamsaraId == item.id) ??
                            new Vehicle.Vehicle();

                    v.Vin = item.vin;
                    v.Name = item.name;
                    v.EngineHours = item.engineHours;
                    v.OdometerMeters = item.odometerMeters;
                    v.Note = item.note;
                    v.CompanyId = company.Id;
                    if (v.ExternalIds == null)
                        v.ExternalIds = new BlockArray.ServiceModel.ExternalIds();

                    v.ExternalIds.SamsaraId = item.id;

                    await vehicleRepo.Save(v);
                }

                var locations = samsaraApi.GetFleetLocations();
                foreach (var item in locations)
                {
                    var v = vehicleRepo.Get().FirstOrDefault(p => p.Vin == item.vin);
                    if (v == null) continue;
                    v.CurrentLocation = new TrackingLocationServiceModel()
                    {
                        Lat = item.latitude,
                        Lng = item.longitude,
                        Timestamp = new DateTime(1970, 1, 1).AddMilliseconds(item.time)
                    };
                    await vehicleRepo.Save(v);
                }

                var drivers = samsaraApi.GetFleetDrivers();
                foreach (var item in drivers)
                {
                    var driver = driverRepo.Get().FirstOrDefault(p => p.ExternalIds.SamsaraId == item.id) ??
                                 new Driver.Driver();
                    var vehicle = vehicleRepo.Get()
                        .FirstOrDefault(p => p.ExternalIds.SamsaraId == item.currentVehicleId);

                    driver.CurrentVehicleId = vehicle?.Id;
                    if (vehicle?.CurrentLocation?.Lat != 0)
                    {
                        driver.CurrentLocation = vehicle?.CurrentLocation;
                    }

                    if (!string.IsNullOrEmpty(config.Cached.EmailDomain))
                        driver.Email = $"{item.username}@{config.Cached.EmailDomain}";

                    driver.LicenseNumber = item.licenseNumber;
                    driver.LicenseState = item.licenseState;
                    driver.Notes = item.notes;
                    driver.Name = item.name;
                    driver.Phone = item.phone;
                    driver.CompanyId = company.Id;
                    if (string.IsNullOrEmpty(driver.InvoiceItemCategoryId))
                    {
                        driver.InvoiceItemCategoryId = config.Cached.InvoiceItemCategoryId;
                        driver.PayrollRate = config.Cached.PayrollRate;
                        driver.PayrollType = config.Cached.PayrollType;
                    }
                    if (driver.ExternalIds == null)
                        driver.ExternalIds = new BlockArray.ServiceModel.ExternalIds();

                    driver.ExternalIds.SamsaraId = item.id;

                    ApplicationUser usr = null;
                    if (userRepo.Get().FirstOrDefault(p => p.NormalizedEmail == driver.Email.ToUpper()) != null)
                        usr = userRepo.Get().FirstOrDefault(p => p.NormalizedEmail == driver.Email.ToUpper());
                    else if (!string.IsNullOrEmpty(driver.LicenseNumber))
                    {
                        usr = _companyAccountService.CreateCompanyUser(driver.Email, driver.LicenseNumber.ToLower(),
                            company.Id,
                            new[] {"Driver"}, true).Result;
                    }

                    if (usr != null)
                    driver.UserId = usr.Id;
                    await driverRepo.Save(driver);


                    var safetyRecord = samsaraApi.GetSafetyRecord(item.id);
                    if (safetyRecord != null)
                    {
                        var record = safetyRepo.Get().FirstOrDefault(p => p.DriverId == driver.Id) ??
                                     new DriverSafety.DriverSafety();
                        record.CrashCount = safetyRecord.crashCount;
                        record.SafetyScore = safetyRecord.safetyScore;
                        record.HarshAccelCount = safetyRecord.harshAccelCount;
                        record.HarshBrakingCount = safetyRecord.harshBrakingCount;
                        record.HarshTurningCount = safetyRecord.harshTurningCount;
                        record.TotalHarshEventCount = safetyRecord.totalHarshEventCount;
                        record.SafetyScoreRank = Convert.ToInt32(safetyRecord.safetyScoreRank);
                        record.TotalDistanceDrivenMeters = Convert.ToInt64(safetyRecord.totalDistanceDrivenMeters);
                        record.TotalTimeDrivenMs = Convert.ToInt64(safetyRecord.totalTimeDrivenMs);
                        record.TimeOverSpeedLimitMs = Convert.ToInt64(safetyRecord.timeOverSpeedLimitMs);
                        record.DriverId = driver.Id;
                        await safetyRepo.Save(record);
                    }
                }
            }
        }
    }
}