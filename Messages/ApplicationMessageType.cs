namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public enum ApplicationMessageType
    {
        MarketDataRequest,
        MarketDataIncrementalRefresh,
        NewOrderSingle,
        OrderStatusRequest,
        ExecutionReport,
        BusinessMessageReject,
        RequestForPosition,
        PositionReport,
        TradeCaptureReportRequest
    }
}