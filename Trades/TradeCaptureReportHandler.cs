using System;
using System.Collections.Generic;
using ED.Atlas.Svc.TC.Ice.FE.DbConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class TradeCaptureReportHandler : IHandle, ITradeCaptureReportHandler
    {
        private readonly List<string> _tradeCaptures;
        private readonly TradeCaptureReportMapping _tradeCaptureReportMapping;
        private readonly IBusService _busService;
        private readonly Dictionary<string, SecurityDefinition> _securityDefinitions;
        private readonly IceDomService _iceDomService;
        private readonly List<string> _ignoreTradesByPriceBasis;

        private static readonly log4net.ILog _tradeLog = log4net.LogManager.GetLogger(
            LoggerNames.Trades);

        public TradeCaptureReportHandler(List<string> tradeCaptures, 
            TradeCaptureReportMapping tradeCaptureReportMapping, 
            IBusService busService, 
            Dictionary<string, SecurityDefinition> securityDefinitions,
            IceDomService iceDomService,
             List<string> ignoreTradesByPriceBasis)
        {
            _tradeCaptures = tradeCaptures;
            _tradeCaptureReportMapping = tradeCaptureReportMapping;
            _securityDefinitions = securityDefinitions;
            _iceDomService = iceDomService;
            _ignoreTradesByPriceBasis = ignoreTradesByPriceBasis;
            _busService = busService;
        }

        private List<DateTime> _trades = new List<DateTime>();
        private int counter = 1;
        public void Handle(IMessage message)
        {
            if(!(message is ITradeCaptureReport))
            {
                return;
            }
            counter++;
            var tradeCaptureReport = message as ITradeCaptureReport;
            if (tradeCaptureReport.OriginalText.Contains("7369242"))
            {

            }
            var intermediateMessage = _tradeCaptureReportMapping.MapTrade(tradeCaptureReport, _securityDefinitions);
            
            if (intermediateMessage == null)
            {
                _tradeLog.Debug($"Mapped trade as null:  {message.OriginalText}");
                return;
            }


            _trades.Add(intermediateMessage.TradeDate);
            
            if (_ignoreTradesByPriceBasis.Contains(intermediateMessage.PriceBasisCode))
            {
                _tradeLog.Debug($"[IgnoreTradesByPriceBasis] Ignored trade: {intermediateMessage.Contract}. PriceBasisCode: {intermediateMessage.PriceBasisCode}");
            }
            else if (!_iceDomService.DoesTradeExist(intermediateMessage.Contract))
            {
                if (_iceDomService.SaveTrade(intermediateMessage, tradeCaptureReport.OriginalText))
                {
                    _busService.SendTradeIntermediateMessage(intermediateMessage);
                    _tradeCaptures.Add(tradeCaptureReport.OriginalText);
                }
            }
            else
            {
                Console.WriteLine("Trade already exists."); // TODO Implement additional check on existing trades. Duplicate ContractId are beginning to show up. Include TradeTime, it should be unique for a trade, even if retrieved after.
            }

        }
    }
}