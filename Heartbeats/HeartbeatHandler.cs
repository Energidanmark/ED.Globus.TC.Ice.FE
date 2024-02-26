using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.Heartbeats
{
    public class HeartbeatHandler : IHandle
    {
        private static readonly log4net.ILog _heartbeatLog = log4net.LogManager.GetLogger(
            LoggerNames.Heartbeats);

        public void Handle(IMessage message)
        {
            if (!(message is IHeartbeatMessage))
            {
                return;
            }

            var heartbeatMessage = message as IHeartbeatMessage;
            _heartbeatLog.Debug($"Received heartbeat: {message.OriginalText}");
        }
    }
}