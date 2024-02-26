using System;
using ED.Atlas.Svc.Rest.Routes;

namespace ED.Atlas.Svc.TC.Ice.FE
{
    public class IceRoutes : ModuleRoutesBase
    {
        public IceRoutes(Uri absoluteuri) : base(absoluteuri)
        {
            
        }

        public IceRoutes()
        {
            
        }

        public string Run => $"/run";
        public string Delete => $"/delete";
    }
}