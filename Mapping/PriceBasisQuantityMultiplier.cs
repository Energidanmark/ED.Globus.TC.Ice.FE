using Repository.Pattern.Ef6;
using Repository.Pattern.Infrastructure;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class PriceBasisQuantityMultiplier : EntityBase, IIdObject
    {
        public int Id { get; set; }
        public string PriceBasis { get; set; }
        public decimal QuantityMultiplier { get; set; }

        public override string ToString()
        {
            return $"#{Id}; PriceBasis: {PriceBasis}; QuantityMultiplier: {QuantityMultiplier}";
        }
    }
}