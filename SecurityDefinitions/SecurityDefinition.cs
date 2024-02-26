using System;

namespace ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions
{
    public interface ISecurityDefinition
    {
        
    }
    public class SecurityDefinition : ISecurityDefinition
    {
        public SecurityDefinition(string name, string quantityUnit, string currencyUnit)
        {
            Name = name;
            QuantityUnit = quantityUnit;
            CurrencyUnit = currencyUnit;
        }

        public string Name { get; set; }

        public string QuantityUnit { get; set; }
        public string CurrencyUnit { get; set; }
        public override string ToString()
        {
            return $"{Name}; {QuantityUnit}; {CurrencyUnit};";
        }
    
    }
}