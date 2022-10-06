using System;
using System.Configuration;

namespace PolygonGSArbiter
{
    class Config
    {
        public static int GameserverID = Int32.Parse(ConfigurationManager.AppSettings.Get("GameserverID"));
        public static string MachineAddress = ConfigurationManager.AppSettings.Get("MachineAddress");
        public static string BaseUrl = ConfigurationManager.AppSettings.Get("BaseUrl");
        public static string BaseUrlHTTP = ConfigurationManager.AppSettings.Get("BaseUrlHTTP");
        public static string AccessKey = ConfigurationManager.AppSettings.Get("AccessKey");
        public static int ServicePort = Int32.Parse(ConfigurationManager.AppSettings.Get("ServicePort"));
        public static int InternalBasePort = Int32.Parse(ConfigurationManager.AppSettings.Get("InternalBasePort"));
        public static int ExternalPortOffset = Int32.Parse(ConfigurationManager.AppSettings.Get("ExternalPortOffset"));
        public static int MaximumJobs = Int32.Parse(ConfigurationManager.AppSettings.Get("MaximumJobs"));
    }
}
