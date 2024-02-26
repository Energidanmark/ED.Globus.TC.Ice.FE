namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public class TradeCaptureReportMessage : ITradeCaptureReport, IMessage
    {
        public TradeCaptureReportMessage(string message)
        {
            OriginalText = message;
        }
        public string OriginalText { get; set; }
    }
}