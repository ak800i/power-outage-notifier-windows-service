using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PowerOutageNotifier
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // PowerOutageService.DummyForTesting().GetAwaiter().GetResult();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PowerOutageService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
