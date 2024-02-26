using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE.Handlers
{
    public interface IHandle
    {
        void Handle(IMessage message);
    }
    
}