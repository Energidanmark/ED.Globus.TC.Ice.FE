using System.Collections.Generic;

namespace ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions
{
    public class SecurityDefinitionRegistry
    {
        private readonly Dictionary<string, SecurityDefinition> _securityDefinitions = new Dictionary<string, SecurityDefinition>();

        public SecurityDefinitionRegistry(string securityDefinition )
        {
            
        }
        public SecurityDefinition Get(string code)
        {
            return _securityDefinitions[code];
        }
    }
}