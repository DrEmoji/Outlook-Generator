using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using OutlookGen;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.IO;
using static OutlookGen.Utils;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Collections;
using Newtonsoft.Json;
using Jurassic.Library;
using static System.Net.Mime.MediaTypeNames;

namespace OutlookGen
{
    public static class Outlook
    {
        public static void Generate()
        {
            string key = "";
            string randomNum = "";
            string ski = "";
            string uaid = "";
            string tcxt = "";
            string hfid = "";
            string apiCanary = "";
            string reference = "";
            string uifvr = "";
            string captchahgid = "";
            string siteid = "";
            string scid = "";
            string hgid = "";
            string rdm = "";
            string mkt = "";
            string country = "";
            CookieContainer cookieContainer = new CookieContainer();
            Dictionary<string, object> AccountInfo = Utils.AccountInfo();
            WebResponse response = Request("https://signup.live.com/signup", HTTPMethods.Get, new WebHeaderCollection()
            {
               { "Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8" }
            }, string.Empty, true, cookieContainer);
            string responseContent = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Utils.Log("Request Data",ConsoleColor.Blue);
            Console.WriteLine("");
            foreach (var kvp in AccountInfo)
            {
                Log($"{kvp.Key}: {kvp.Value}", ConsoleColor.Cyan);
            }
            Console.WriteLine();
            reference = responseContent.Replace(Dns.GetHostEntry("signup.live.com").AddressList[0].ToString(), "signup.live.com");
            Match match = Regex.Match(responseContent, @"Key=""(.*?)""; var randomNum=""(.*?)""; var SKI=""(.*?)""");
            if (match.Success)
            {
                key = match.Groups[1].Value.Replace(@"e=10001;m=","");
                randomNum = match.Groups[2].Value;
                ski = match.Groups[3].Value;

                Utils.Log("Key: " + key,ConsoleColor.Cyan);
                Console.WriteLine();
                Utils.Log("RandomNum: " + randomNum, ConsoleColor.Cyan);
                Console.WriteLine();
                Utils.Log("SKI: " + ski, ConsoleColor.Cyan);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Values not found in the response.");
            }

            Match match2 = Regex.Match(responseContent, @"t0=([\s\S]*)w\[""\$Config""]=");
            if (match2.Success)
            {
                string t0Value = match2.Groups[1].Value.Replace(";", "");

                // Parse JSON data
                JObject jsonData = JObject.Parse(t0Value);

                // Extract values
                uaid = jsonData["clientTelemetry"]["uaid"].Value<string>();
                tcxt = jsonData["clientTelemetry"]["tcxt"].Value<string>();
                apiCanary = jsonData["apiCanary"].Value<string>();

                Utils.Log("UAID: " + uaid, ConsoleColor.Cyan);
                Console.WriteLine();
                Utils.Log("TCXT: " + tcxt, ConsoleColor.Cyan);
                Console.WriteLine();
                Utils.Log("ApiCanary: " + apiCanary, ConsoleColor.Cyan);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("t0 value not found in the response.");
            }

            hfid = Utils.ExtractData(responseContent, @"\""fid\"":\""([^\""]+)\""");
            Utils.Log("FID: " + hfid, ConsoleColor.Cyan);
            Console.WriteLine();

            hgid = Utils.ExtractData(responseContent, "\"Signup_BirthdatePage_Client\":(\\d+)");
            Utils.Log("HGID: " + hgid, ConsoleColor.Cyan);
            Console.WriteLine();

            captchahgid = Utils.ExtractData(responseContent, "\"Signup_HipEnforcementPage_Client\":(\\d+)");
            Utils.Log("CAPTCHA HGID: " + captchahgid, ConsoleColor.Cyan);
            Console.WriteLine();

            uifvr = Utils.ExtractData(responseContent, "\"uiflvr\"\\s*:\\s*(\\d+)");
            Utils.Log("Uiflvr: " + uifvr, ConsoleColor.Cyan);
            Console.WriteLine();

            siteid = Utils.ExtractData(responseContent, "\"siteId\":\"(\\d+)\"");
            Utils.Log("SiteId: " + siteid, ConsoleColor.Cyan);
            Console.WriteLine();

            scid = Utils.ExtractData(responseContent, "\"scid\":(\\d+)");
            Utils.Log("SCID: " + scid, ConsoleColor.Cyan);
            Console.WriteLine();

            rdm = Utils.ExtractData(responseContent, "\"isRdm\":(\\d+)");
            Utils.Log("RDM: " + rdm, ConsoleColor.Cyan);
            Console.WriteLine();

            mkt = Utils.ExtractData(responseContent, "\"mkt\":\"([^\"]+)\"");
            Utils.Log("MKT: " + mkt, ConsoleColor.Cyan);
            Console.WriteLine();

            country = Utils.ExtractData(responseContent, @"""countryCode""\s*:\s*""([^""]+)""");
            Utils.Log("Country: " + country, ConsoleColor.Cyan);
            Console.WriteLine();

            string cipher = Utils.Encrypt(AccountInfo["password"].ToString(), randomNum, key);
            Utils.Log($"Cipher: {cipher}\n", ConsoleColor.Cyan);
            bool Created = true;
            bool UsingCaptcha = true;
            string encAttemptToken = "";
            string captcha = "";
            string dfpRequestId = "";
            while (Created)
            {
                if (UsingCaptcha)
                {
                    captcha = Utils.GenCaptchaKey().Result; //Utils.GenAntiCaptchaKey("https://signup.live.com/?lic=1", "B7D8911C-5CC8-A9A3-35B0-554ACEE604DA");
                    Console.WriteLine("Captcha: " + captcha);
                    Console.WriteLine();
                }

                WebHeaderCollection headers = new WebHeaderCollection
                {
                    { "accept-language", "en-GB,en-US;q=0.9,en;q=0.8" },
                    { "cache-control", "no-cache" },
                    { "canary", apiCanary },
                    { "hpgid", hgid },
                    { "origin", "https://signup.live.com" },
                    { "pragma", "no-cache" },
                    { "scid", scid },
                    { "sec-ch-ua", "\" Not A;Brand\";v=\"107\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\"" },
                    { "sec-ch-ua-mobile", "?0" },
                    { "sec-ch-ua-platform", "\"Windows\"" },
                    { "sec-fetch-dest", "empty" },
                    { "sec-fetch-mode", "cors" },
                    { "sec-fetch-site", "same-origin" },
                    { "tcxt", tcxt },
                    { "uaid", uaid },
                    { "uiflvr", uifvr },
                    { "x-ms-apitransport", "xhr" },
                    { "x-ms-apiversion", "2" },
                    { "referrer", "https://signup.live.com/?lic=1" }
                };

                Dictionary<string, object> payload = new Dictionary<string, object>
                {
                    { "RequestTimeStamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                    { "EvictionWarningShown", new List<object>() },
                    { "UpgradeFlowToken", new Dictionary<string, object>() },
                    { "MemberNameChangeCount", 1 },
                    { "MemberNameAvailableCount", 1 },
                    { "MemberNameUnavailableCount", 0 },
                    { "CipherValue", cipher },
                    { "SKI", ski },
                    { "Country", country },
                    { "IsOptOutEmailDefault", true },
                    { "IsOptOutEmailShown", true },
                    { "IsOptOutEmail", true },
                    { "LW", true },
                    { "SiteId", siteid },
                    { "IsRDM", int.Parse(rdm) },
                    { "WReply", null },
                    { "ReturnUrl", null },
                    { "SignupReturnUrl", "" },
                    { "uiflvr", int.Parse(uifvr) },
                    { "uaid", uaid },
                    { "SuggestedAccountType", "EASI" },
                    { "SuggestionType", "Prefer" },
                    { "encAttemptToken", encAttemptToken },
                    { "dfpRequestId", dfpRequestId },
                    { "scid", int.Parse(scid) },
                    { "hpgid", int.Parse(hgid) }
                };

                if (UsingCaptcha)
                {
                    Console.WriteLine("Adding Captcha Data");
                    payload.Add("HFId", hfid);
                    payload.Add("HType", "enforcement");
                    payload.Add("HSol", captcha);
                    payload.Add("HPId", "B7D8911C-5CC8-A9A3-35B0-554ACEE604DA");
                    payload["hpgid"] = int.Parse(captchahgid);
                }

                foreach (var kvp in AccountInfo)
                {
                    if (kvp.Key != "password")
                    {
                        payload.Add(kvp.Key, kvp.Value);
                    }
                    //payload.Add(kvp.Key, kvp.Value);
                }

                var responsed = Request($"https://signup.live.com/API/CreateAccount?lic=1", HTTPMethods.Post, headers, JsonConvert.SerializeObject(payload), true, cookieContainer);
                string responseContentd = new StreamReader(responsed.GetResponseStream()).ReadToEnd();
                //Console.WriteLine(responseContentd);
                dynamic json = JsonConvert.DeserializeObject(responseContentd);
                Utils.Log("Response Data", ConsoleColor.DarkRed);
                Console.WriteLine();
                string errorcode = json["error"]["code"];
                tcxt = json["error"]["telemetryContext"];
                Utils.Log("UPDATED TCXT: " + tcxt, ConsoleColor.Red);
                Console.WriteLine();
                encAttemptToken = responseContentd.Split(':')[4].Split(',')[0].Split('"')[1].Split('"')[0].Replace("\\u002f", "/").Replace("\\u003a", ":").Replace("\\u002b", "+").Replace("\\u0026", "&").Replace("\\u003d", "=");
                encAttemptToken = encAttemptToken.Substring(0, encAttemptToken.Length - 1);
                Utils.Log("encAttemptToken: " + encAttemptToken, ConsoleColor.Red);
                Console.WriteLine();
                dfpRequestId = responseContentd.Split(':')[5].Split('"')[1].Split('"')[0].Replace(@"\","");
                Utils.Log("dfpRequestId: " + dfpRequestId, ConsoleColor.Red);
                Console.WriteLine();

                if (errorcode == "1041")
                {
                    Console.WriteLine("\nCaptcha Failed\n");
                    UsingCaptcha = true;
                    //captcha = Utils.GenCaptchaKey().Result;
                    //Console.WriteLine("Captcha: " + captcha);
                }
                else
                {
                    //Console.WriteLine(json["error"]["data"]["encAttemptToken"]);
                    Utils.Log($"{errorcode} - {Utils.ErrorHandler(errorcode)}", ConsoleColor.Yellow);
                    Created = false;
                }
            }
        }
    }
}
