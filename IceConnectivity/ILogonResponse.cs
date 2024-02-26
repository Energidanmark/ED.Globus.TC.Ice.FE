using System;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public interface ILogonResponse
    {
        string Id { get; set; }
        DateTime SendingTime { get; set; }

        bool IsLoggedIn { get; set; }
    }
}