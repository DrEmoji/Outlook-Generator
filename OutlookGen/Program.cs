using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OutlookGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Outlook.Generate();
            Console.ReadLine();
        }
    }
}
