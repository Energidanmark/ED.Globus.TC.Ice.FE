using System;
using System.Collections.Generic;
using System.Linq;
using ED.Atlas.Svc.TC.Ice.FE.DbConnectivity;
using Repository.Pattern.Ef6;
using Repository.Pattern.Infrastructure;
using Repository.Pattern.Repositories;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class BrokerMap : EntityBase, IIdObject
    {
        public int Id { get; set; }
        public string IceValue { get; set; }
        public string MapValue { get; set; }

        public override string ToString()
        {
            return $"#{Id}; IceValue: {IceValue}; MapValue: {MapValue}";
        }
    }
    public interface IAtlasDbContextFactory
    {
        AtlasIceDbContext Get();
    }

    public class AtlasIceDbContextFactory : IAtlasDbContextFactory
    {
        public AtlasIceDbContext Get()
        {
            return new AtlasIceDbContext();
        }


    }
    /// <summary>
    /// Gets an object that holds information about how much quantity should be multiplied.
    /// This is weird, but Elviz is.
    /// </summary>
    public class PriceBasisQuantityMultiplierProvider
    {
        private readonly IAtlasDbContextFactory _contextFactory;
        
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
           (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<PriceBasisQuantityMultiplier> PriceBasisQuantityMultipliers { get; set; }

        public PriceBasisQuantityMultiplierProvider(IAtlasDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            PriceBasisQuantityMultipliers = new List<PriceBasisQuantityMultiplier>();
        }

        public decimal GetMultiplier(string priceBasis)
        {
            decimal multiplier = 1m;

            if (!PriceBasisQuantityMultipliers.Any())
            {
                try
                {
                    using (var context = _contextFactory.Get())
                    {
                        IRepository<PriceBasisQuantityMultiplier> brokerMapRepository =
                            new Repository<PriceBasisQuantityMultiplier>(context);

                        PriceBasisQuantityMultipliers = brokerMapRepository.Query().Select().ToList();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to retrieve BrokerMappings {ex.Message}");
                }
            }

            var priceBasisQuantityMultiplier = PriceBasisQuantityMultipliers.FirstOrDefault(x => x.PriceBasis.Equals(priceBasis));
            if (priceBasisQuantityMultiplier != null)
            {
                multiplier = priceBasisQuantityMultiplier.QuantityMultiplier;
            }

            return multiplier;
            
        }

    }
}