using System.Data.Entity.ModelConfiguration;
using ED.Atlas.Svc.TC.Ice.FE.Mapping;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class BrokerMapMap : EntityTypeConfiguration<BrokerMap>
    {

        public BrokerMapMap()
        {
            ToTable("BrokerMaps");
        }
    }
}