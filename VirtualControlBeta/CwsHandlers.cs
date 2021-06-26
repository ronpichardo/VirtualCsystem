using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtualControlBeta.Config;

namespace VirtualControlBeta
{
    public class ConfigSettingsReqHandler : IHttpCwsHandler
    {
        private string _roomId, _roomName, _shureIp;

        public ConfigSettingsReqHandler(string roomId, string roomName, string shureIp)
        {
            this._roomId = roomId;
            this._roomName = roomName;
            this._shureIp = shureIp;
        }

        void IHttpCwsHandler.ProcessRequest(HttpCwsContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            if (context.Request.HttpMethod == "GET")
            {
                JObject resObj = new JObject
                {
                    {
                        "data", new JObject
                        {
                            { "roomid", _roomId },
                            { "roomname", _roomName },
                            { "mtr-ip", _shureIp }
                        }
                    }
                };
                string jsonResponse = JsonConvert.SerializeObject(resObj);
                context.Response.Write(jsonResponse, true);
            }
            else if (context.Request.HttpMethod == "PUT")
            {
                context.Response.Write("{\"status\": \"Settings request handler to implement\"}", true);
            }
        }
    }

    public class RoutingRequestHandler: IHttpCwsHandler
    {
        public RoutingRequestHandler()
        {
            ErrorLog.Error("RoutingHandler has been initialized");
        }

        void IHttpCwsHandler.ProcessRequest(HttpCwsContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Write("RequestHandler made it here", true);
        }
    }
}
