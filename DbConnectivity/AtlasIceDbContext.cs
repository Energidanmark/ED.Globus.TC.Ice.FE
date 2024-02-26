using System.Data.Entity;
using ED.Atlas.Svc.TC.Ice.FE.Mapping;
using ED.Atlas.Svc.TC.Ice.FE.Trades;
using Repository.Pattern.Ef6;

namespace ED.Atlas.Svc.TC.Ice.FE.DbConnectivity
{
   
    public class AtlasIceDbContext : DataContext
    {
        public AtlasIceDbContext() : base("name=AtlasIce")
        {
            Database.SetInitializer<AtlasIceDbContext>(null);
        }

        public virtual IDbSet<MarketTypeToPropertyMap> MarketTypeToPropertyMaps { get; set; }
        public virtual IDbSet<PriceBasisQuantityMultiplier> PriceBasisQuantityMultipliers { get; set; }
        public virtual IDbSet<BrokerMap> BrokerMaps { get; set; }
        public virtual IDbSet<Trade> Trades { get; set; }
        public virtual IDbSet<DayAheadTrader> DayAheadTraders { get; set; }
    }
 
}