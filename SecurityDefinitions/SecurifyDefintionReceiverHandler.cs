using System.Collections.Generic;
using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions
{
    public class SecurifyDefintionReceiverHandler : IHandle
    {
        private readonly SecurityDefinitionParser _securityDefinitionParser;
        private Dictionary<string, SecurityDefinition> _securityDefinitions;


        public SecurifyDefintionReceiverHandler(SecurityDefinitionParser securityDefinitionParser, Dictionary<string, SecurityDefinition> securityDefinitions)
        {
            _securityDefinitionParser = securityDefinitionParser;
            _securityDefinitions = securityDefinitions;
        }

        public void Handle(IMessage message)
        {
            if (!(message is ISecurityDefinitionReceiver))
            {
                return;
            }

            var securityDefinition = message as IMessage;
            

            var secDefs = _securityDefinitionParser.ParseFixMessage(securityDefinition.OriginalText);
            foreach (var definition in secDefs)
            {
                if (!_securityDefinitions.ContainsKey(definition.Key))
                {
                    _securityDefinitions.Add(definition.Key, definition.Value);
                }
            }

            if (_securityDefinitions.Count > 0)
            {

            }
            
        }
    }
}