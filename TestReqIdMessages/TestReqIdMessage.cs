using System;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.TestReqIdMessages
{
    public class TestReqIdMessage : ITestReqId, IMessage
    {
        public TestReqIdMessage(int testReqId, DateTime sendingTime, string originalText)
        {
            Id = testReqId;
            SendingTime = sendingTime;
            OriginalText = originalText;
        }

        public int Id { get; set; }
        public DateTime SendingTime { get; set; }
        public string OriginalText { get; set; }
    }
}