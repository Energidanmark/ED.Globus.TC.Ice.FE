using System.Collections.Generic;
using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;
using ED.Atlas.Svc.TC.Ice.FE.Trades;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class LogonHandler : IHandle
    {
        private readonly FixMessageConstructor _fixMessageConstructor;
        private readonly List<OutgoingMessageFix44> _outgoingMessages;

        public LogonHandler(FixMessageConstructor fixMessageConstructor, List<OutgoingMessageFix44> outgoingMessages)
        {
            _fixMessageConstructor = fixMessageConstructor;
            _outgoingMessages = outgoingMessages;
        }

        public void Handle(IMessage message)
        {
            if (!(message is ILogonResponse))
            {
                return;
            }

            var logonResponse = message as ILogonResponse;

            if (!logonResponse.IsLoggedIn)
            {
                return;
            }

            //_outgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, 1, 114));
            //_outgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, 2, 17));
            // Add securityDefinition requests
            //ConsoleWriter.WriteReceived($"LogonResponse: {logonResponse.Id} Sent from receiver: {logonResponse.SendingTime}");
        }
    }
}