using System;

namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public interface ITestReqId
    {
        int Id { get; set; }
        DateTime SendingTime { get; set; }
    }
}