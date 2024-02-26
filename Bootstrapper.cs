using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using ED.Atlas.Svc.TC.Ice.FE.Heartbeats;
using ED.Atlas.Svc.TC.Ice.FE.IceConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Main;
using ED.Atlas.Svc.TC.Ice.FE.Mapping;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;
using ED.Atlas.Svc.TC.Ice.FE.Trades;
using ED.Atlas.Svc.TC.Shrd.Plat;
using log4net.Core;
using Microsoft.Practices.Unity;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace ED.Atlas.Svc.TC.Ice.FE
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
           (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Bootstrapper()
        {
            
        }
        //


        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Nancy.Json.JsonSettings.MaxJsonLength = Int32.MaxValue;
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            var appSetting = new AppSettings();
            var username = appSetting.Username;
            var password = appSetting.Password;
            var compId = appSetting.CompId;
            var senderCompId = appSetting.SenderCompId;
            var targetCompId = appSetting.TargetCompId;
            var endpoint = appSetting.BusEndpoint;
            string host = appSetting.IceIp; // Production
            int port = appSetting.IcePort;


            var unityContainer = new UnityContainer();
            var bus = RebusConfiguration.With("AtlasTrayport", endpoint, unityContainer).Start();
            var busService = new BusService(bus, appSetting);


            var fixMessageConstructor = new FixMessageConstructor(username, password, compId, senderCompId, targetCompId);
            var tcpFactory = new TcpFactory(host, port);
            var fixClient = new FixClient(tcpFactory, fixMessageConstructor, new HeartBeatTimer(fixMessageConstructor, 25));
            

            var securityDefinitions = new List<SecurityDefinition>();
            var securityDefinitionParser = new SecurityDefinitionParser();

            var atlasIceDbContextFactory = new AtlasIceDbContextFactory();
            var runner = new Runner(fixClient, fixMessageConstructor,
                new TradeCaptureReportMapping(new FieldMapper(), new DatabaseDataProvider(), new BrokerMappingProvider(atlasIceDbContextFactory), new PriceBasisQuantityMultiplierProvider(atlasIceDbContextFactory),
                    new PriceParser(), securityDefinitionParser), busService, new SecurityDefinitionParser(), new IceDomService(new AtlasIceDbContextFactory()), appSetting.IgnoreTradesByPriceBasis);

            //string endPoint = AppConfigAppSettings.Get("BusEndpoint", _log);
            //string dealTopic = AppConfigAppSettings.Get("DealTopic", _log);
            //string iceApiIp = AppConfigAppSettings.Get("IceApiIp", _log);
            //int iceApiPort;
            //int.TryParse(AppConfigAppSettings.Get("IceApiPort", _log), out iceApiPort);

            //string apiUserName = AppConfigAppSettings.Get("ApiUserName", _log);
            //string apiPassword = AppConfigAppSettings.Get("ApiPassword", _log);
            //string apiCompId = AppConfigAppSettings.Get("ApiCompId", _log);
            //string apiSenderCompId = AppConfigAppSettings.Get("ApiSenderCompId", _log);
            //string apiTargetCompId = AppConfigAppSettings.Get("ApiTargetCompId", _log);

            //iceApiIp = "63.247.113.201";
            //iceApiPort = 433;
            //var fixMessageConstructor = new FixMessageConstructor(apiUserName, apiPassword, apiCompId, apiSenderCompId, apiTargetCompId); // Test
            //var tcpFactory = new TcpFactory(iceApiIp, iceApiPort);

            //var fixClient = new FixClient(tcpFactory, fixMessageConstructor, new HeartBeatTimer(fixMessageConstructor, 25));

            //var unityContainer = new UnityContainer();
            //var appSetting = new AppSetting();
            //var bus = RebusConfiguration.With("AtlasTrayport", endPoint, unityContainer).Start();
            //var busService = new BusService(bus, appSetting);

            //var atlasIceDbContextFactory = new AtlasIceDbContextFactory();
            //var runner = new Runner(fixClient, fixMessageConstructor, new TradeCaptureReportMapping(new FieldMapper(), new MappingProvider(),
            //    new BrokerMappingProvider(atlasIceDbContextFactory),
            //    new PriceBasisQuantityMultiplierProvider(atlasIceDbContextFactory), new PriceParser()),
            //    busService, new SecurityDefinitionParser());

            container.Register(runner);
        }
    }
    public class PriceParser
    {
        public decimal ParsePrice(string valueToConvert)
        {
            try
            {
                valueToConvert = FormatPriceValueIndependentOfCultureInfo(valueToConvert);
                var culture = CultureInfo.CreateSpecificCulture("en-GB");

                decimal price =
                    (decimal)Convert.ChangeType(valueToConvert, typeof(decimal), culture);
                return price;
            }
            catch (InvalidCastException ex)
            {
                string foo = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                string foo = ex.Message;
                return -1;
            }
        }

        public decimal CorrectPrice(decimal value, string priceUnit)
        {
            if (priceUnit.ToLower().Contains("gbx"))
            {
                return value / 1000;
            }

            return value;
        }
        private string FormatPriceValueIndependentOfCultureInfo(string valueToConvert)
        {
            if (valueToConvert.Contains(".") && valueToConvert.Contains(","))
            {
                valueToConvert = valueToConvert.Replace(".", "TEMP");
                valueToConvert = valueToConvert.Replace(",", ".");
                valueToConvert = valueToConvert.Replace("TEMP", ",");
            }
            else
            {
                if (valueToConvert.Contains(","))
                    valueToConvert = valueToConvert.Replace(",", ".");
            }
            return valueToConvert;
        }
    }
}