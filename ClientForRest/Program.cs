using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using POIWebServiceContracts;

namespace ClientForRest
{
    class Program

    {
        static void Main(string[] args)
        {
         //   ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://localhost:8000");
           // cf.Endpoint.Behaviors.Add(new WebHttpBehavior());
         //   IService channel = cf.CreateChannel();
           // channel.PutPointOfInterest("Gustavsborg");
           // channel.PutPointOfInterestImage("test", "1", new FileStream("sol.jpg", FileMode.Open));
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://localhost:8000/api/image/test.jpg");
            request.Method = "POST";
            request.ContentType = "image/jpeg";
            

            Stream reqStream = request.GetRequestStream();
            reqStream.Write(GetData("sol.jpeg"), 0, GetData("sol.jpeg").Length);
            reqStream.Close();
            
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            Console.WriteLine("HTTP/{0} {1} {2}", resp.ProtocolVersion, (int)resp.StatusCode, resp.StatusDescription + resp.Headers);

            Console.WriteLine("Press enter to continue");
            Console.ReadLine();

            HttpWebRequest putrequest = (HttpWebRequest)HttpWebRequest.Create("http://localhost:8000/api/poi/6/image/1/vinter.jpg?description=finfinbilddetta");
            putrequest.Method = "PUT";
            putrequest.ContentType = "image/jpg";


            Stream putreqStream = putrequest.GetRequestStream();
            putreqStream.Write(GetData("vinter.jpg"), 0, GetData("vinter.jpg").Length);
            putreqStream.Close();

            HttpWebResponse putresp = (HttpWebResponse)putrequest.GetResponse();
            Console.WriteLine("HTTP/{0} {1} {2}", putresp.ProtocolVersion, (int)putresp.StatusCode, putresp.StatusDescription + putresp.Headers);
            //Console.WriteLine(channel.GetPointOfInterest("1"));

            Console.Read();
        }

        static byte[] GetData(string file) 
        {
            FileStream stream = File.OpenRead(file);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            stream.Close();
            return data;
        }

            
    }
}
