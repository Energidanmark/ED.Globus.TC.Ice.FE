using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Mapping;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;
using ED.Atlas.Svc.TC.Shrd.Msg;
using log4net;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class TradeCaptureReportMapping
    {
        private readonly FieldMapper _fieldMapper;
        private readonly DatabaseDataProvider _databaseDataProvider;
        private readonly BrokerMappingProvider _brokerMappingProvider;
        private readonly PriceBasisQuantityMultiplierProvider _priceBasisQuantityMultiplierProvider;
        private readonly PriceParser _priceParser;
        private readonly SecurityDefinitionParser _securityDefinitionParser;

        private const string EmirDefault = "EMIR Risk Reducing";
        //TradeMappingLog
        private static readonly log4net.ILog _log = LogManager.GetLogger(LoggerNames.Trades);
        public TradeCaptureReportMapping(FieldMapper fieldMapper, DatabaseDataProvider databaseDataProvider, BrokerMappingProvider brokerMappingProvider, PriceBasisQuantityMultiplierProvider priceBasisQuantityMultiplierProvider, PriceParser priceParser, SecurityDefinitionParser securityDefinitionParser)
        {
            _fieldMapper = fieldMapper;
            _databaseDataProvider = databaseDataProvider;
            _brokerMappingProvider = brokerMappingProvider;
            _priceBasisQuantityMultiplierProvider = priceBasisQuantityMultiplierProvider;
            _priceParser = priceParser;
            _securityDefinitionParser = securityDefinitionParser;
        }

        public TradeIntermediateMessage MapTrade(ITradeCaptureReport tradeCaptureReport, Dictionary<string, SecurityDefinition> securityDefinitions)
        {
           
            if (tradeCaptureReport == null || 
                tradeCaptureReport.OriginalText.Contains("sibh2") ||
                tradeCaptureReport.OriginalText.Contains("chfh2"))
            {
                return null;
            }

            //var bar = securityDefinitions.Keys.OrderBy(x => x).ToList();
            var t = new TradeIntermediateMessage();
            t.Guid = Guid.NewGuid();
            
            var contractId = _fieldMapper.ParseField(17, tradeCaptureReport.OriginalText);
            t.Contract = contractId;
          
            var priceStr = _fieldMapper.ParseField(31, tradeCaptureReport.OriginalText);
            t.Price = _priceParser.ParsePrice(priceStr);

            var qtyStr = _fieldMapper.ParseField(32, tradeCaptureReport.OriginalText);
            decimal qty;
            decimal.TryParse(qtyStr, out qty);

            
            // 1 = Buy 2 = Sell
            var direction = _fieldMapper.ParseField(54, tradeCaptureReport.OriginalText);
            t.Direction = direction == "1" ? "B" : "S";

            var beginDateStr = _fieldMapper.ParseField(916, tradeCaptureReport.OriginalText);
            var beginDate = DateTime.ParseExact(beginDateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
            t.BeginDateTimeUtc = beginDate.ToUniversalTime();

            var endDateStr = _fieldMapper.ParseField(917, tradeCaptureReport.OriginalText);
            var endDate = DateTime.ParseExact(endDateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
            t.EndDateTimeUtc = endDate.ToUniversalTime();

            var tradeDateStr = _fieldMapper.ParseField(016, tradeCaptureReport.OriginalText);
            DateTime tradeDate;
            DateTime.TryParse(tradeDateStr, out tradeDate);
            t.TradeDate = tradeDate;

            t.AcquiredTimeUtc = DateTime.UtcNow;

            var tradeTimeStr = _fieldMapper.ParseField(60, tradeCaptureReport.OriginalText);
            var tradeTime = DateTime.ParseExact(tradeTimeStr, "yyyyMMdd-HH:mm:ss.FFF", CultureInfo.InvariantCulture);
            t.TradeTimeUtc = tradeTime;
            t.ExecutionVenueCode = "ICE";
            t.TradeDate = tradeTime.ToLocalTime().Date;
            t.DataProviderAtlasCode = "ICE";
            t.StatusCode = "Saved";
            t.AcquiredTimeUtc = DateTime.UtcNow;
            
            var tradeSecurityDefinitionValue = _fieldMapper.ParseField(48, tradeCaptureReport.OriginalText);
            tradeSecurityDefinitionValue = _securityDefinitionParser.CleanKeyOrKeyValue(tradeSecurityDefinitionValue);
        
            SecurityDefinition securityDefinition = null;
            if (new List<string>(securityDefinitions.Keys).Any(x => x.StartsWith(tradeSecurityDefinitionValue)))
            {

            }
            securityDefinitions.TryGetValue(tradeSecurityDefinitionValue, out securityDefinition);
            t.CounterpartyPortfolioCode = "ICE ABN";

            // This must be set, so we will use this value as default.
            t.EmirCode = EmirDefault;

            if (securityDefinition == null)
            {
                _log.Debug($"Failed SecurityDefinition for contractId: {t.Contract}; SecurityDefinition lookup: [{tradeSecurityDefinitionValue}] OriginalTrade: {tradeCaptureReport.OriginalText}");
                return null;
            }
            else
            {

                var maps = _databaseDataProvider.GetMapFor(securityDefinition.Name);
                if (!maps.Any())
                {
                    _log.Debug($"Found SecurityDefinition but name: {securityDefinition.Name} found no Property maps.");
                    return null;
                }

                foreach (var marketTypeToPropertyMap in maps)
                {
                    PropertyInfo prop = typeof(TradeIntermediateMessage).GetProperty(marketTypeToPropertyMap.Property.Trim(), BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null || !prop.CanWrite || string.IsNullOrEmpty(marketTypeToPropertyMap.Property.Trim()))
                        continue;


                    var val = Convert.ChangeType(marketTypeToPropertyMap.MapValue?.Trim() ?? string.Empty, prop.PropertyType);
                    prop.SetValue(t, val, null);
                }
            }

            // T quantity must be set AFTER the PropertyMaps. PriceBasis is required to be set before quantity.
            t.Quantity = _priceBasisQuantityMultiplierProvider.GetMultiplier(t.PriceBasisCode) * qty;
         
            t.AtlasTraderCode = _fieldMapper.ParseTrader(tradeCaptureReport.OriginalText, _databaseDataProvider.GetTraders().Select(x=> x.TraderName).ToList());
            if (string.IsNullOrEmpty(t.AtlasTraderCode))
            {
                // not for day ahead or a new trader must be created.
                _log.Debug($"Trader was null for ContractId: {t.Contract}: {tradeCaptureReport.OriginalText}");
                return null;
            }

            var brokerValue = _fieldMapper.ParseValueForPartyRoleTag(56, tradeCaptureReport.OriginalText);
            var brokerMap = _brokerMappingProvider.GetMapFor(brokerValue);
            if (brokerMap != null)
            {
                t.BrokerCode = brokerMap.MapValue;
            }

            return t;
        }
    }
}