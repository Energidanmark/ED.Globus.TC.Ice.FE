using System;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.Heartbeats
{
    public class HeartbeatMessage : IHeartbeatMessage, IMessage
    {
        public HeartbeatMessage(DateTime sendingTime, string originalText)
        {
            SendingTime = sendingTime;
            OriginalText = originalText;
        }

        public DateTime SendingTime { get; set; }
        public string CreateFix()
        {
            // Use message constructor to create the fix message

            return string.Empty;
        }

        public string OriginalText { get; set; }
    }
}