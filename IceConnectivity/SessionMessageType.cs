namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public enum SessionMessageType
    {
        Logon,
        Logout,
        Heartbeat,
        TestRequest,
        Resend,
        Reject,
        SequenceReset,
        TradeCaptureReportRequest,
        SecurityDefinitionRequest,
        UserCompanyRequest
    }
}