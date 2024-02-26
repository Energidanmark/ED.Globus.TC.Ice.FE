using System;
using System.Threading;
using ED.Atlas.Svc.TC.Ice.FE.Common;
using ED.Atlas.Svc.TC.Ice.FE.IceConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.Trades;
using log4net;

namespace ED.Atlas.Svc.TC.Ice.FE.Heartbeats
{
    public class HeartBeatTimer
    {
        // TODO [2018-01-17 Robert Nogal] :  Message creation as delegate?
        private readonly FixMessageConstructor _messageconstructor;
        public int Duration { get; set; }
        private bool _stop = false;
        private static readonly log4net.ILog _heartbeatLog = LogManager.GetLogger(LoggerNames.HeartbeatLoggerName);
        public HeartBeatTimer(FixMessageConstructor messageconstructor, int duration)
        {
            _messageconstructor = messageconstructor;
            Duration = duration;
        }

        private Timer _timer;

        public void Run()
        {
            _timer = new Timer(state => OnTimesUp(), null, TimeSpan.FromSeconds(Duration), Timeout.InfiniteTimeSpan);

        }

        public void Stop()
        {
            _stop = true;
        }

        public event EventHandler<OutgoingMessageFix44> TimesUp; // Could be any, but do color for now, therefor string

        private void OnTimesUp()
        {
            if (_stop) // TODO [2018-02-12 Robert Nogal] : Handle this correctly. I think the Timer goes out of scope and therefor is acting like its been Disposed.
            {
                return;
            }

            ConsoleWriter.Write("Timesup");
            _heartbeatLog.Debug("Time is up, need to send heartbeat");
            var handler = TimesUp;
            handler?.Invoke(this, _messageconstructor.CreateHeartBeatMessage(SessionQualifier.QUOTE));

            _timer.Dispose();
        }
    }
}