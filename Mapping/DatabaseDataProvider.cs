using System;
using System.Collections.Generic;
using System.Linq;
using ED.Atlas.Svc.TC.Ice.FE.DbConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Trades;
using Repository.Pattern.Ef6;
using Repository.Pattern.Repositories;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class DatabaseDataProvider
    {
        // TODO [2018-02-21 Robert Nogal] : How should we inject atlasDbContext?
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<MarketTypeToPropertyMap> MarketToPropertyMaps { get; set; }

        public DatabaseDataProvider()
        {
            MarketToPropertyMaps = new List<MarketTypeToPropertyMap>();
        }
        // TODO [2018-02-21 Robert Nogal] : How should we handle updates?
        public List<MarketTypeToPropertyMap> GetMapFor(string key)
        {
            // TODO [2018-02-21 Robert Nogal] : try / catch?
            if (!MarketToPropertyMaps.Any())
            {
                try
                {
                    using (var context = new AtlasIceDbContext())
                    {
                        IRepository<MarketTypeToPropertyMap> marketTypeToPropertyMapRepository =
                            new Repository<MarketTypeToPropertyMap>(context);

                        MarketToPropertyMaps = marketTypeToPropertyMapRepository.Query().Select().ToList();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to retrieve Markettype to property map for key: {key}. Msg: {ex.Message}");
                }
            }

            return MarketToPropertyMaps.Where(x => key.Contains(x.MarketTypeName)).ToList();
        }

        public List<DayAheadTrader> GetTraders()
        {
            List<DayAheadTrader> traders = new List<DayAheadTrader>();
           
                try
                {
                    using (var context = new AtlasIceDbContext())
                    {
                        IRepository<DayAheadTrader> marketTypeToPropertyMapRepository =
                            new Repository<DayAheadTrader>(context);

                        traders = marketTypeToPropertyMapRepository.Query().Select().ToList();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to retrieve DayAheadTraders. Msg: {ex.Message}");
                }
           

            return traders;
        }
    }
}