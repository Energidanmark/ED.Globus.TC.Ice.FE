using System.Collections.Generic;
using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.IceConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.Trades;

namespace ED.Atlas.Svc.TC.Ice.FE.TestReqIdMessages
{
    public class TestReqIdHandler : IHandle
    {
        private readonly FixMessageConstructor _fixMessageConstructor;
        private readonly List<OutgoingMessageFix44> _messages;

        public TestReqIdHandler(FixMessageConstructor fixMessageConstructor, List<OutgoingMessageFix44> messages)
        {
            _fixMessageConstructor = fixMessageConstructor;
            _messages = messages;
        }

        public void Handle(IMessage message)
        {
            if (!(message is ITestReqId))
            {
                return;
            }

            var testReqId = message as ITestReqId;
            
            _messages.Add(_fixMessageConstructor.CreateTestReqId(SessionQualifier.QUOTE, testReqId.Id));
        }
    }

 
}