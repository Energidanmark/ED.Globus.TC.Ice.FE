using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class LogoutHandler : IHandle
    {
        private readonly FixClient _fixClient;

        public LogoutHandler(FixClient fixClient)
        {
            _fixClient = fixClient;
        }

        public void Handle(IMessage message)
        {
            if (!(message is ILogout))
            {
                return;
            }

            var logout = message as ILogout;

            _fixClient.Close();
        }
    }
}