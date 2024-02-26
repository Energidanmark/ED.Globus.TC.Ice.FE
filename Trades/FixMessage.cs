namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class FixMessage
    {
        
        public FixMessage(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
        
    }
}