using System;
using System.Configuration;
using ED.Atlas.Svc.TC.Ice.FE.Main;
using ED.Atlas.Svc.TC.Shrd.Msg;
using Rebus.Bus;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public interface IBusService
    {
        void SendTradeIntermediateMessage(TradeIntermediateMessage message);
    }

    public class BusService : IBusService
    {
        private readonly IBus _bus;
        private readonly IAppSetting _appSetting;
        private IBus bus;
        private AppSettings appSetting;

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BusService(IBus bus, IAppSetting appSetting)
        {
            _bus = bus;
            _appSetting = appSetting;
        }

        public void SendTradeIntermediateMessage(TradeIntermediateMessage message)
        {
            try
            {
                _bus.Advanced.Topics.Publish(_appSetting.DealTopic, message);
            }
            catch (Exception ex)
            {
                _log.Debug($"Failed to publish tradeIntermediateMessage with contractId: {message.Contract}");
            }
        }
    }

    
}