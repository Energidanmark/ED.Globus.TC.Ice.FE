using System;
using Repository.Pattern.Ef6;
using Repository.Pattern.Infrastructure;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class Trade : EntityBase, IIdObject
    {
        public int Id { get; set; }
        public string ContractId { get; set; }
        public string FixMessage { get; set; }
        public DateTime AcquiredDateTimeUtc { get; set; }
        public bool Active { get; set; }

        public override string ToString()
        {
            return $"Id {Id}; {ContractId}";
        }
    }

    public class DayAheadTrader : EntityBase, IIdObject
    {
        public int Id {get;set; }
        public string TraderName {get;set; }
    }
}