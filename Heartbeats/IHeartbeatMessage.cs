using System;

namespace ED.Atlas.Svc.TC.Ice.FE.Heartbeats
{
    public interface IHeartbeatMessage
    {
        DateTime SendingTime { get; set; }
    }
}