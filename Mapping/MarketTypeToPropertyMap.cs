using Repository.Pattern.Ef6;
using Repository.Pattern.Infrastructure;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class MarketTypeToPropertyMap : EntityBase, IIdObject
    {
        public int Id { get; set; }
        public string MarketTypeName { get; set; }
        public string Property { get; set; }
        public string MapValue { get; set; }

        public override string ToString()
        {
            return $"{Id} MarketTypeName: {MarketTypeName} Property: {Property} MapValue: {MapValue}";
        }
    }
}