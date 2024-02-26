using Owin;

namespace ED.Atlas.Svc.TC.Ice.FE.Main
{
    public class StartUp
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseNancy();
        }
    }
}