using System;
using System.Collections.Generic;
using System.Linq;
using ED.Atlas.Svc.TC.Ice.FE.DbConnectivity;
using Repository.Pattern.Ef6;
using Repository.Pattern.Repositories;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class BrokerMappingProvider
    {
        private readonly IAtlasDbContextFactory _atlasDbContextFactory;

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
           (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<BrokerMap> BrokerMaps { get; set; }

        public BrokerMappingProvider(IAtlasDbContextFactory atlasDbContextFactory)
        {
            _atlasDbContextFactory = atlasDbContextFactory;
            BrokerMaps = new List<BrokerMap>();
        }

        public BrokerMap GetMapFor(string key)
        {
            if (!BrokerMaps.Any())
            {
                try
                {
                    using (var context = _atlasDbContextFactory.Get())
                    {
                        IRepository<BrokerMap> brokerMapRepository =
                            new Repository<BrokerMap>(context);

                        BrokerMaps = brokerMapRepository.Query().Select().ToList();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to retrieve BrokerMappings {ex.Message}");
                }
            }

            return BrokerMaps.FirstOrDefault(x=> x.IceValue.ToLower().Equals(key.ToLower()));
        }
    }
}