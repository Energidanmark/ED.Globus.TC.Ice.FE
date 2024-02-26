using System.Data.Entity.ModelConfiguration;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class PriceBasisQuantityMultiplierMap : EntityTypeConfiguration<PriceBasisQuantityMultiplier>
    {
        public PriceBasisQuantityMultiplierMap()
        {
            ToTable("PriceBasisQuantityMultipliers");
        }
    }
}