using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED.Atlas.Svc.TC.Ice.FE.Mapping;
using ED.Atlas.Svc.TC.Shrd.Msg;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class IceDomService
    {
        private readonly AtlasIceDbContextFactory _contextFactory;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IceDomService(AtlasIceDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Sets trade to inActive.
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public string DeleteTrade(string contractId)
        {
            var sb = new StringBuilder();

            using (var context = _contextFactory.Get())
            {
                var trade = context.Trades.FirstOrDefault(x => x.ContractId.Equals(contractId) && x.Active);
                if (trade == null)
                {
                    sb.AppendLine($"Could not find trade with contractId: {contractId} which is also Active.");
                }
                else
                {
                    
                    trade.Active = false;
                    try
                    {
                        context.SaveChanges(new List<Trade> {trade});
                        sb.AppendLine($"Succesfully changed Active = false for trade with contractId: {contractId}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Failed to change Active => false for trade with contractId: {contractId}{Environment.NewLine}Message: {ex.Message}");
                        // log trade?
                    }
                }
            }

            _log.Debug(sb.ToString());

            return sb.ToString();
        }

        public bool DoesTradeExist(string contractId)
        {
            var doesTradeExist = false;
            try
            {
                using (var context = _contextFactory.Get())
                {
                    var trade = context.Trades.FirstOrDefault(x => x.ContractId.Equals(contractId) && x.Active);
                    if (trade != null)
                    {
                        doesTradeExist = true;
                        _log.Debug($"Trade with ContractId: {contractId} already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Debug($"Failed to look up trade. Exception: {ex.Message}");
            }
            
            return doesTradeExist;
            
        }

        public bool SaveTrade(TradeIntermediateMessage intermediateMessage, string originalTrade)
        {
            var sb = new StringBuilder();
            bool didSaveTrade = false;
            try
            {
            var trade = new Trade {AcquiredDateTimeUtc = intermediateMessage.AcquiredTimeUtc, Active = true, ContractId = intermediateMessage.Contract, FixMessage = originalTrade};

            using (var context = _contextFactory.Get())
            {

                context.SaveChanges(new List<Trade> {trade});
                sb.AppendLine($"Succesfully saved trade with contractId: {intermediateMessage.Contract}");
                didSaveTrade = true;
            }


            }
            catch (Exception ex)
            {
                sb.AppendLine($"Failed to save trade with contractId: {intermediateMessage.Contract} {Environment.NewLine}Message: {ex.Message}");
                didSaveTrade = false;
            }

            _log.Debug(sb.ToString());
            
            return didSaveTrade;
        }

        public List<Trade> GetTrades(DateTime timestamp)
        {
            var trades = new List<Trade>();

            using (var context = _contextFactory.Get())
            {
                trades = context.Trades.Where(x => x.AcquiredDateTimeUtc >= timestamp && x.Active).ToList();
            }

            return trades;
        }
    }
}