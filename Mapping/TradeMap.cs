using System.Data.Entity.ModelConfiguration;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class TradeMap : EntityTypeConfiguration<TradeMap>
    {
        public TradeMap()
        {
            ToTable("Trade");
        }
    }
}