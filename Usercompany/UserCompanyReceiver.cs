using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;

namespace ED.Atlas.Svc.TC.Ice.FE.Usercompany
{
    public class UserCompanyReceiver : IMessage, IUserCompanyReceiver
    {
        public UserCompanyReceiver(string originalText)
        {
            OriginalText = originalText;
        }
        public string OriginalText { get; set; }
    }
}