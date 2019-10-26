using System;
using System.Collections.Generic;
using RestSharp;

namespace FreightTrust.Modules.SamsaraConfig.Api
{
    public class SamsaraApi
    {
        private readonly string _accessToken;
        public string GroupId { get; }
        public RestClient RestClient { get; }
        
        public SamsaraApi(string accessToken, string groupId, string endpoint = "https://api.samsara.com/v1/")
        {
            _accessToken = accessToken;
            GroupId = groupId;
            RestClient = new RestClient(endpoint);
            RestClient.AddDefaultUrlSegment("access_token", accessToken);
        }
        public List<SamsaraUser> GetUsers()
        {    
            var req = new RestRequest($"users?access_token={_accessToken}");
            var result = RestClient.Execute<List<SamsaraUser>>(req);
            return result.Data;
        }          
        public List<SamsaraDriver> GetFleetDrivers()
        {
            var req = new RestRequest($"fleet/drivers?access_token={_accessToken}",Method.POST);
            
            req.AddJsonBody(
            new {
                groupId = GroupId
            });
            var result = RestClient.Execute<FleetDriversResult>(req);
            Console.WriteLine(result.Content);
            return result.Data.drivers;
        }        
        
        public List<SamsaraVehicle> GetFleetList()
        {
            var req = new RestRequest($"fleet/list?access_token={_accessToken}",Method.POST);
            
            req.AddJsonBody(
            new {
                groupId = GroupId,
                limit = Int32.MaxValue
            });
            var result = RestClient.Execute<FleetListResult>(req);
            Console.WriteLine(result.Content);
            return result.Data.vehicles;
        }  
        public List<SamsaraAsset> GetFleetAssets()
        {
            var req = new RestRequest($"fleet/assets?access_token={_accessToken}&groupId={GroupId}",Method.GET);
            var result = RestClient.Execute<AssetsRequest>(req);
            Console.WriteLine(result.Content);
            return result.Data.assets;
        }       
        public List<SamsaraVehicleLocation> GetFleetLocations()
        {
            var req = new RestRequest($"fleet/locations?access_token={_accessToken}",Method.POST);
            req.AddJsonBody(
                new {
                groupId = GroupId
            });
            var result = RestClient.Execute<FleetLocationsResponse>(req);
            Console.WriteLine(result.Content);
            return result.Data.vehicles;
        }        
        
        public List<Log> GetHosLogs(object driverId, object startMs, object endMs)
        {
            var req = new RestRequest($"fleet/hos_logs?access_token={_accessToken}",Method.POST);
            
            req.AddJsonBody(
                new {
                    groupId = GroupId,
                    driverId = driverId,
                    startMs = startMs,
                    endMs = endMs
                });
            var result = RestClient.Execute<HosLogsResponse>(req);
            Console.WriteLine(result.Content);
            return result.Data.logs;
        }    
        
        public DriverSafteyRecordResult GetSafetyRecord(string driverId)
        {
            var req = new RestRequest(
                $"fleet/drivers/{driverId}/safety/score?startMs={DateTimeOffset.Now.Subtract(new TimeSpan(365,0,0,0)).ToUnixTimeMilliseconds()}&endMs={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&access_token={_accessToken}",Method.GET);
       
            var result = RestClient.Execute<DriverSafteyRecordResult>(req);
            return result.Data;
        }
    }
    
    public class HarshEvent
    {
        public string harshEventType { get; set; }
        public string timestampMs { get; set; }
        public string vehicleId { get; set; }
    }

    public class DriverSafteyRecordResult
    {
        public int crashCount { get; set; }
        public string driverId { get; set; }
        public int harshAccelCount { get; set; }
        public int harshBrakingCount { get; set; }
        public List<HarshEvent> harshEvents { get; set; }
        public int harshTurningCount { get; set; }
        public int safetyScore { get; set; }
        public string safetyScoreRank { get; set; }
        public string timeOverSpeedLimitMs { get; set; }
        public string totalDistanceDrivenMeters { get; set; }
        public int totalHarshEventCount { get; set; }
        public string totalTimeDrivenMs { get; set; }
    }
    
    public class SamsaraVehicleLocation
    {
        public double heading { get; set; }
        public string id { get; set; }
        public double latitude { get; set; }
        public string location { get; set; }
        public double longitude { get; set; }
        public string name { get; set; }
        public string odometerMeters { get; set; }
        public bool onTrip { get; set; }
        public string speed { get; set; }
        public long time { get; set; }
        public string vin { get; set; }
    }

    public class FleetLocationsResponse
    {
        public string groupId { get; set; }
        public List<SamsaraVehicleLocation> vehicles { get; set; }
    }
    
    public class ExternalIds
    {
        public string payrollId { get; set; }
        public string maintenanceId { get; set; }
    }
    public class FleetDriversResult
    {
        public List<SamsaraDriver> drivers { get; set; }
    }
    public class SamsaraVehicle
    {
        public string id { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        public string vin { get; set; }
        public string odometerMeters { get; set; }
        public string engineHours { get; set; }
        public double fuelLevelPercent { get; set; }
    }
    public class Cable
    {
        public string assetType { get; set; }
    }

    public class SamsaraAsset
    {
        public string id { get; set; }
        public string name { get; set; }
        public string engineHours { get; set; }
        public string assetSerialNumber { get; set; }
        public List<Cable> cable { get; set; }
    }

    public class AssetsRequest
    {
        public List<SamsaraAsset> assets { get; set; }
    }
    public class HosRequest
    {
        public string driverId { get; set; }
        public string endMs { get; set; }
        public string groupId { get; set; }
        public string startMs { get; set; }
    }
    
    public class Log
    {
        public List<int> codriverIds { get; set; }
        public string driverId { get; set; }
        public string groupId { get; set; }
        public string locCity { get; set; }
        public double locLat { get; set; }
        public double locLng { get; set; }
        public string locName { get; set; }
        public string locState { get; set; }
        public string logStartMs { get; set; }
        public string remark { get; set; }
        public string statusType { get; set; }
        public string vehicleId { get; set; }
    }

    public class HosLogsResponse
    {
        public List<Log> logs { get; set; }
    }
    public class Pagination
    {
        public bool hasNextPage { get; set; }
        public bool hasPrevPage { get; set; }
        public string startCursor { get; set; }
        public string endCursor { get; set; }
    }

    public class FleetListResult
    {
        public string groupId { get; set; }
        public List<SamsaraVehicle> vehicles { get; set; }
        public Pagination pagination { get; set; }
    }
    public class SamsaraDriver
    {
        public string id { get; set; }
        public bool isDeactivated { get; set; }
        public List<Tag> tags { get; set; }
        public string currentVehicleId { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string phone { get; set; }
        public string notes { get; set; }
        public string licenseNumber { get; set; }
        public string licenseState { get; set; }
        public bool eldExempt { get; set; }
        public string eldExemptReason { get; set; }
        public bool eldBigDayExemptionEnabled { get; set; }
        public bool eldAdverseWeatherExemptionEnabled { get; set; }
        public bool eldPcEnabled { get; set; }
        public bool eldYmEnabled { get; set; }
        public string eldDayStartHour { get; set; }
        public string vehicleId { get; set; }
        public string groupId { get; set; }
        public ExternalIds externalIds { get; set; }
    }
    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }
        public string parentTagId { get; set; }
    }

    public class TagRole
    {
        public Tag tag { get; set; }
        public string role { get; set; }
        public string roleId { get; set; }
    }

    public class SamsaraUser
    {
        public string name { get; set; }
        public string email { get; set; }
        public string authType { get; set; }
        public string organizationRole { get; set; }
        public string organizationRoleId { get; set; }
        public string id { get; set; }
        public List<TagRole> tagRoles { get; set; }
    }
}