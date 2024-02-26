using System;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class Logout : ILogout, IMessage
    {
        private readonly DateTime _sendingTime;

        public Logout(DateTime sendingTime, string originalText)
        {
            _sendingTime = sendingTime;
            OriginalText = originalText;
        }

        public string OriginalText { get; set; }
    }
}