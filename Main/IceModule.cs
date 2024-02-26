using System;
using System.Collections.Generic;
using System.Linq;
using ED.Atlas.Svc.TC.Ice.FE.Trades;
using Nancy;
using Nancy.Extensions;
using Nancy.Json;

namespace ED.Atlas.Svc.TC.Ice.FE.Main
{
    public class Trades
    {
        public Trades(List<Trade> newTrades)
        {
            if (newTrades.Any())
            {
                NumberOfTrades = newTrades.Count;
                ContractIds = newTrades.Select(x => x.ContractId).ToList();
                StoredTrades = newTrades;
            }
        }
        public int NumberOfTrades { get; set; }
        public List<string> ContractIds { get; set; }
        public List<Trade> StoredTrades { get; set; }
    }
    public class IceModule : NancyModule
    {
        private readonly Runner _runner;
        private readonly IceDomService _iceDomService;
        private DateTime prevTimeStamp = DateTime.Now;
        private IceRoutes _iceRoutes = new IceRoutes();


        public IceModule(Runner runner, IceDomService iceDomService) : base("/v1")
        {
            _runner = runner;
            _iceDomService = iceDomService;

            Get[$"/{_iceRoutes.Run}"] = p =>
            {
                _runner.SendSubscriptionRequest();
                return 1;

                //var timestamp = DateTime.UtcNow;
                //_runner.AddTradeCaptureRequest();
                //var storedTrades = _iceDomService.GetTrades(timestamp);
                //var trades = new Trades(storedTrades);
                //return JSONHelper.ToJSON(trades);
            };

            Post[$"{_iceRoutes.Delete}"] = p =>
            {
                var requestAsStr = Request.Body.AsString();
                var result = _iceDomService.DeleteTrade(requestAsStr);
                return JSONHelper.ToJSON(result);
            };
        }
    }
    public static class JSONHelper
    {
        public static string ToJSON(this object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return serializer.Serialize(obj);
        }

        public static string ToJSON(this object obj, int recursionDepth)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;
            return serializer.Serialize(obj);
        }
    }
}