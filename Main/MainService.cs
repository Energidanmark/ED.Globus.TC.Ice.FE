using System;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using System.Threading;

namespace ED.Atlas.Svc.TC.Ice.FE.Main
{
    public class MainService
    {
        private readonly string _portnumber;
        private CancellationToken token = new CancellationToken();
        private IDisposable _owinService;
        public MainService(string portnumber)
        {
            _portnumber = portnumber;
        }

        public void Start()
        {
            _owinService = WebApp.Start<StartUp>($"http://+:{_portnumber}");
        }

        public void Stop()
        {
            _owinService.Dispose();
        }
    }
}