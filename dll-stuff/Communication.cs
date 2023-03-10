using System;
using System.IO;
using System.Net;
using System.Web;

namespace JabeDll {
    public static class Chatbot {
        // Sends a message to Streamerbot to use Streamerbot's chatbot instead of Lumia
        // Yes, it can be dumb to send something to yourself, but it gives us flexibility
        public static void ToStreamerbot(string stuff) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.HttpHandlerUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            
            // TODO Format this better so users can use it to send messages via Streamerbot
            string data = "{"
                        + "\"action\":{"
                        + "\"id\" : \"" + Settings.ActionId + "\","
                        + "\"name\" : \"" + Settings.ActionName + "\""
                        + "},"
                        + "\"args\" : {"
                        + "\"message\" : \"" + stuff + "\""
                        + "}"
                        + "}";
            Data.Log(data);

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
                writer.Write(data);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        // Sends a message to Lumiastream API to use their chatbot to send a message to the according platform
        public static void ToLumiastream(string platform, string stuff) {
            if (stuff != "" && platform != "") {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.LumiaAPIUrl + Settings.LumiaAPIToken);
                request.Method = "POST";
                request.ContentType = "application/json";
                
                string data = "{"
                            + "\"type\" : \"chatbot-message\","
                            + "\"params\" : {"
                            + "\"value\" : \"" + stuff + "\" , "
                            + "\"platform\" : \"" + platform + "\""
                            + "}"
                            + "}";
                            Data.Log(data);

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
                    writer.Write(data);
                }
                var response = (HttpWebResponse)request.GetResponse();
            }
        }
    }
}