using System;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class LogonResponse : ILogonResponse, IMessage
    {
        private readonly DateTime _sendingTime;

        public LogonResponse(bool isLoggedIn, DateTime sendingTime, string originalText)
        {
            IsLoggedIn = isLoggedIn;
            _sendingTime = sendingTime;
            OriginalText = originalText;
        }


        public string Id { get; set; }
        public DateTime SendingTime { get; set; }
        public string OriginalText { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}