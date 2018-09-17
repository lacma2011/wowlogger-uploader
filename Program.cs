using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using MoonSharp.Interpreter;

namespace fish
{    class Program
    {
        private const string apiEndpoint = "http://your_wow_fishmongerer_setup.com/submit";

        static void Main(string[] args)
        {
            String fileName = args[0];
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string text = reader.ReadToEnd();
                var isJson = Program.IsValidJson(text);
                JToken obj;
                if (isJson)
                {
                    // Is JSON
                    obj = JToken.Parse(text);

                } else {
                    // Assume LUA

                    obj = Program.convertLua(text);
                }

                // send to API
                if (false)
                {
                    Program.send(obj);
                } else {
                    Program.sendCurl("12334Auth", "dev-id-1", obj);
                }
            }


        }

        private static JToken convertLua(string strInput)
        {
            string script = strInput + "\n  "  + " return FishLogData";
            DynValue res = Script.RunString(script);

            JArray array = new JArray(); 
            foreach (var key in res.Table.Keys)
            {
                array.Add(res.Table.Get(key).ToPrintString());
            }

            return JToken.Parse(array.ToString());
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static void send(JToken jsonObject)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var result = client.PostAsync(apiEndpoint, content).Result;
            Console.Write(result);
        }

        private static void sendCurl(string auth, string developerId, JToken jsonObject)
        {
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", auth);
                client.DefaultRequestHeaders.Add("Developer-Id", developerId);
                var result = client.PostAsync(apiEndpoint, content).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                Console.Write(resultContent);
            }
        }
    }
}
