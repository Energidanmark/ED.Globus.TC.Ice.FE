using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using log4net;

namespace ED.Atlas.Svc.TC.Ice.FE.Logging
{
    public class MessageLoggingHandler : IHandle
    {
        private static readonly log4net.ILog _defaultLog
            = LogManager.GetLogger(LoggerNames.DefaultLoggerName);
        public void Handle(IMessage message)
        {
            // 18-03-2021 Removed this log, trying to clean up the excessive logging.
            //if(message != null)
            //_defaultLog.Debug(message.OriginalText);
        }
    }
}