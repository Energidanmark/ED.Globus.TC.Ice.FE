using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions
{
    public interface ISecurityDefinitionReceiver
    {
        
    }
    public class SecurityDefinitionReceiver : IMessage, ISecurityDefinitionReceiver
    {
        public SecurityDefinitionReceiver(string originalText)
        {
            OriginalText = originalText;
        }

        public string OriginalText { get; set; }
    }
}