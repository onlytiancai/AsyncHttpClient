using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AsynHttpClient.Http;

namespace AsynHttpClient
{
    static class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.Completed += client_Completed;

            var urls = UrlGenerator.GetLinksFromUrl();

            urls.ForEach<string>(client.Fetch);
            //urls.ForEach<string>(Console.WriteLine);
            Console.ReadKey();
            Console.WriteLine("count:{0}",client.Count);
            PrintGcInfo();
        }

        private static void client_Completed(HttpFetchResult result)
        {
            if (result.Error == null)
                Console.WriteLine("{0}\r\n{1}", result.Url, result.Response);
            else
                Console.WriteLine("{0}\r\n{1}", result.Url, result.Error);
        }

        private static void PrintGcInfo()
        {
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                Console.WriteLine("\tGen " + i + ": \t\t" + GC.CollectionCount(i));
            }
        }
    }












}
