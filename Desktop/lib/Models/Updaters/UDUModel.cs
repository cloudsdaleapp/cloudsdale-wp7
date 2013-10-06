using System;
using System.Net;
using System.Text;

namespace CloudsdaleWin7.lib.Models.Updaters
{
    //User Data Upload Model
    class UDUModel
    {
        private readonly static Session Current = App.Connection.SessionController.CurrentSession;
        private readonly static string Address = Endpoints.User.Replace("[:id]", Current.Id);
        private const string Type = "application/json";
        private const string Method = "PUT";
        public static string AuthToken = Current.AuthToken;

        public static void UpdateSessionModel(string token, string value)
        {
            var dataObject = @"{""$token$"":""$value$""".Replace("$token$", token).Replace("$value$", value);
            var data = Encoding.UTF8.GetBytes(dataObject);
            var request = WebRequest.CreateHttp(Address);
            request.Accept = Type;
            request.Method = Method;
            request.ContentType = Type;
            request.ContentLength = data.Length;
            request.Headers["X-Auth-Token"] = AuthToken;

            request.BeginGetRequestStream(ar =>
            {
                var reqs = request.EndGetRequestStream(ar);
                reqs.Write(data, 0, data.Length);
                reqs.Close();
                request.BeginGetResponse(a =>
                {
                    try
                    {
                        var response =
                        request.EndGetResponse(a);
                        response.Close();
                        App.Connection.SessionController.CurrentSession.ForceValidate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }, null);
            }, null);
        }
        public static void UpdateSessionModel(string token, bool value)
        {
            var dataObject = @"{""$token$"":$value$".Replace("$token$", token).Replace("$value$", value.ToString());
            var data = Encoding.UTF8.GetBytes(dataObject);
            var request = WebRequest.CreateHttp(Address);
            request.Accept = Type;
            request.Method = Method;
            request.ContentType = Type;
            request.ContentLength = data.Length;
            request.Headers["X-Auth-Token"] = AuthToken;

            request.BeginGetRequestStream(ar =>
            {
                var reqs = request.EndGetRequestStream(ar);
                reqs.Write(data, 0, data.Length);
                reqs.Close();
                request.BeginGetResponse(a =>
                {
                    try
                    {
                        var response =
                        request.EndGetResponse(a);
                        response.Close();
                        App.Connection.SessionController.CurrentSession.ForceValidate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }, null);
            }, null);
        }
    }
}
