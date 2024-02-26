using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ED.Atlas.Svc.TC.Ice.FE.Main
{
    public interface IAppSetting
    {
        string AtlasTrayportConnectionString { get; }
        string DealTopic { get; }
        string BusEndpoint { get; }
        string Password { get; }
        string Username { get; }
        string CompId { get; }
        string SenderCompId { get; }
        string TargetCompId { get; }
        string IceIp { get; }
        int IcePort { get; }
        List<string> IgnoreTradesByPriceBasis { get; }

    }
    public class AppSettings : IAppSetting
    {
        public string AtlasTrayportConnectionString
        {
            get
            {
                return GetConnectionString("AtlasTrayport");
            }
        }

        public string BusEndpoint
        {
            get
            {
                return GetAppSettings("Endpoint");
            }
        }

        public string DealTopic
        {
            get
            {
                return GetAppSettings("DealTopic");
            }
        }


        public string Password
        {
            get
            {
                return GetAppSettings("ApiPassword");
            }
        }

        public string Username
        {
            get
            {
                return GetAppSettings("ApiUserName");
            }
        }

        public string CompId
        {
            get
            {
                return GetAppSettings("ApiCompId");
            }
        }

        public string SenderCompId
        {
            get
            {
                return GetAppSettings("ApiSenderCompId");
            }
        }

        public string TargetCompId
        {
            get
            {
                return GetAppSettings("ApiTargetCompId");
            }
        }

        public string IceIp
        {
            get
            {
                return GetAppSettings("IceApiIp");
            }
        }
        public int IcePort
        {
            get
            {
                int port;
                int.TryParse(GetAppSettings("IceApiPort"), out port);
                return port;
            }
        }

        public List<string> IgnoreTradesByPriceBasis
        {
            get
            {
                return GetAppSettings("IgnoreTradesByPriceBasis").Split(';').ToList();
            }
        }

        private string GetConnectionString(string id)
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        private string GetAppSettings(string id)
        {
            return ConfigurationManager.AppSettings.Get(id);
        }
    }
}
