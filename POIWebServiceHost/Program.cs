using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
//using System.ServiceModel.Web;
using Microsoft.Http;
using Microsoft.ServiceModel.Web;

using POIWebServiceContracts;
//using Autodocs.ServiceModel.Web;

namespace POIWebServiceHost
{
    class Program
    {

        public static Uri HostAddress { get; set; }

        static void Main(string[] args)
        {
            
            WebServiceHost2 host = new WebServiceHost2(typeof(Service),false, new Uri("http://localhost:8000/api"));
            HostAddress = host.BaseAddresses.First();
            host.TransferMode = TransferMode.Streamed;
            host.MaxMessageSize = 2147483647;
           
            //ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IService),  new WebHttpBinding() {  TransferMode = TransferMode.Streamed, MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 65536 }, "");
            
            //Console.WriteLine("började skitpå " + ep.Address);
            //  ep.Behaviors.Add(new WebHttpBehavior());
            //ep.Behaviors.Add(new AutodocsBehavior());
            


         
            //host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "").Behaviors.Add(new WebHttpBehavior());
           // ServiceDebugBehavior stp = host.Description.Behaviors.Find<ServiceDebugBehavior>();
         //   stp.HttpHelpPageEnabled = false;

            host.Open();
            Console.WriteLine("Service is up and running");
            Console.WriteLine("Press enter to quit ");
            Console.ReadLine();
            host.Close();
        }
    }
}
