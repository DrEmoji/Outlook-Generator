using _2CaptchaAPI;
using System;
using System.Collections.Generic;
using System.IO;
using Faker;
using Faker.Extensions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using _2CaptchaAPI.Enums;
using Jurassic;
using System.Net.Http;
using System.Security.Policy;
using Jurassic.Library;
using static _2CaptchaAPI._2Captcha;
using System.Text.RegularExpressions;

namespace OutlookGen
{
    public static class Utils
    {
        private static Dictionary<string, string> errors = new Dictionary<string, string>()
        {
            { "403", "Bad Username" },
            { "1040", "SMS Needed" },
            { "1041", "Enforcement Captcha" },
            { "1042", "Text Captcha" },
            { "1043", "Invalid Captcha" },
            { "1312", "Captcha Error" },
            { "450", "Daily Limit Reached" },
            { "1304", "OTP Invalid" },
            { "1324", "Verification SLT Invalid" },
            { "1058", "Username Taken" },
            { "1117", "Domain Blocked" },
            { "1181", "Reserved Domain" },
            { "1002", "Incorrect Password" },
            { "1009", "Password Conflict" },
            { "1062", "Invalid Email Format" },
            { "1063", "Invalid Phone Format" },
            { "1039", "Invalid Birth Date" },
            { "1243", "Invalid Gender" },
            { "1240", "Invalid first name" },
            { "1241", "Invalid last name" },
            { "1204", "Maximum OTPs reached" },
            { "1217", "Banned Password" },
            { "1246", "Proof Already Exists" },
            { "1184", "Domain Blocked" },
            { "1185", "Domain Blocked" },
            { "1052", "Email Taken" },
            { "1242", "Phone Number Taken" },
            { "1220", "Signup Blocked" },
            { "1064", "Invalid Member Name Format" },
            { "1330", "Password Required" },
            { "1256", "Invalid Email" },
            { "1334", "Eviction Warning Required" },
            { "100", "Bad Register Request" }
        };

        public static string ErrorHandler(string errorcode)
        {
            return errors[errorcode];
        }

        private static string GenerateRandomPassword()
        {
            string characters = $"{Faker.CompanyFaker.Name()}{Faker.NumberFaker.Number(4)}!#$!".Replace(" ", "");
            return characters;
        }

        private static int GenerateRandomNumber(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue + 1);
        }

        public static Dictionary<string, object> AccountInfo()
        {
            string first = NameFaker.FirstName();
            string last = NameFaker.LastName();
            string email = $"{Internet.UserName()}{GenerateRandomNumber(1, 3)}@outlook.com";
            Dictionary<string, object> Info = new Dictionary<string, object>
            {
                { "FirstName", first },
                { "LastName",  last },
                {"CheckAvailStateMap", new string[] { $"{email}:undefined" }},
                { "MemberName", email },
                { "password", GenerateRandomPassword() },
                { "BirthDate", $"{GenerateRandomNumber(1, 27)}:0{GenerateRandomNumber(1, 9)}:{GenerateRandomNumber(1990, 2004)}" }
            };
            return Info;

        }


        public static async Task<string> GenCaptchaKey()
        {
            Log("Solving Captcha",ConsoleColor.Cyan);
            var captcha = new _2Captcha("5d88b062e456c39bd6ddeea115454272");
            var captchasolution = await captcha.SolveFunCaptcha("B7D8911C-5CC8-A9A3-35B0-554ACEE604DA", "https://signup.live.com/signup", false, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36", "https://client-api.arkoselabs.com/"); // new Dictionary<string, string>{{ "blob", "undefined" }});
            Log("Captcha Solved", ConsoleColor.Cyan);
            return captchasolution.Response;
        }


        public static string GenAntiCaptchaKey(string websiteUrl, string siteKey)
        {
            string apiKey = "f6f9d94f0dbca186bb51e1e07507e2be";
            Dictionary<string, object> payload = new Dictionary<string, object>()
            {
                { "clientKey", apiKey },
                {
                    "task", new Dictionary<string, object>
                    {
                        { "type", "FunCaptchaTaskProxyless" },
                        { "websiteURL", websiteUrl },
                        { "websitePublicKey", siteKey },
                        { "funcaptchaApiJSSubdomain", "https://client-api.arkoselabs.com/fc/ca/" }
                    }
                }
            };
            //var payload = $"{{\"clientKey\":\"{apiKey}\",\"task\":{{\"type\":\"FunCaptchaTaskProxyless\",\"websiteURL\":\"{websiteUrl}\",\"websitePublicKey\":\"{siteKey}\",\"funcaptchaApiJSSubdomain\":\"client-api.arkoselabs.com\"}}, \"data\":\"y8yP3yIKbwMV1w\"}}";
            var response = Request("https://api.anti-captcha.com/createTask", HTTPMethods.Post, new WebHeaderCollection(), JsonConvert.SerializeObject(payload), false);

            var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(new StreamReader(response.GetResponseStream()).ReadToEnd());
            var taskId = responseObject["taskId"];
            Console.WriteLine($"Task ID: {taskId}");

            while (true)
            {
                System.Threading.Thread.Sleep(5000);
                response = Request("https://api.anti-captcha.com/getTaskResult", HTTPMethods.Post, new WebHeaderCollection(), $"{{\"clientKey\":\"{apiKey}\",\"taskId\":\"{taskId}\"}}", false);

                responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(new StreamReader(response.GetResponseStream()).ReadToEnd());
                var status = responseObject["status"];
                if (status == "ready")
                {
                    string solution = responseObject["solution"]["token"];
                    return solution;
                }
            }
        }


        public static string Encrypt(string password, string randomNum, string key)
        {
            ScriptEngine engine = new ScriptEngine();
            engine.SetGlobalValue("console", new Jurassic.Library.FirebugConsole(engine));
            engine.Execute(new WebClient().DownloadString("https://pastebin.com/raw/bRTr7eQ9"));
            var encryptFunction = engine.Evaluate<FunctionInstance>("encrypt");
            var result = encryptFunction.Call("encrypt", password, randomNum, key);

            return result.ToString();
        }

        public static string ExtractData(string data, string pattern)
        {
            Match match = Regex.Match(data, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }

        public static void Log(string message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        public static WebResponse Request(string Url, HTTPMethods method, WebHeaderCollection Headers, string PostData = null, bool KeepAlive = false, CookieContainer cookie = null, IWebProxy proxy = null)
        {
            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(Url);
            if (proxy != null) httpWebRequest.Proxy = proxy;
            if (Headers != null) httpWebRequest.Headers = Headers;
            httpWebRequest.Method = SendMethod(method);
            if (cookie != null)
                httpWebRequest.CookieContainer = cookie;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
            if (KeepAlive) httpWebRequest.KeepAlive = true;
            if (httpWebRequest.Method == "POST")
            {
                byte[] bytes = Encoding.UTF8.GetBytes(PostData);
                httpWebRequest.ContentLength = bytes.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                {
                    try
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    finally
                    {
                        stream?.Close();
                    }
                };
            }
                return httpWebRequest.GetResponse();
        }


        public static string SendMethod(HTTPMethods hTTPMethods)
        {
            switch (hTTPMethods)
            {
                case HTTPMethods.Get:
                    return "GET";
                case HTTPMethods.Post:
                    return "POST";
                case HTTPMethods.Put:
                    return "PUT";
            }
            return "GET";
        }

        public enum HTTPMethods : byte
        {
            Get,
            Post,
            Put,
        }
    }
}
