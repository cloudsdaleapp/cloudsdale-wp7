using System;
using System.Diagnostics;
using System.Net.Http;
using CloudsdaleWin7.Views;
using CloudsdaleWin7.lib.Models;
using Newtonsoft.Json;

namespace CloudsdaleWin7.lib.Helpers
{
    public static class BrowserHelper
    {
        private static bool IsCloudLink(string link)
        {
            if (link.Contains("cloudsdale.org/clouds")) return true;
            return false;
        }

        public static void FollowLink(string uri)
        {
            if (!IsCloudLink(uri))
            {
                Process.Start(uri);
                return;
            }
            var client = new HttpClient().AcceptsJson();
            var cloudId = uri.Split('/')[4];
            var response = client.GetStringAsync(Endpoints.Cloud.Replace("[:id]", cloudId));
            var cloudObject = (JsonConvert.DeserializeObject<WebResponse<Cloud>>(response.Result)).Result;

            JoinCloud(cloudObject);
        }

        public async static void JoinCloud(Cloud cloud)
        {
            cloud = await App.Connection.ModelController.UpdateCloudAsync(cloud);

            if (App.Connection.SessionController.CurrentSession.Clouds.Contains(cloud))
            {
                Main.Instance.Clouds.SelectedItem = cloud;
                return;
            }

            var client = new HttpClient
            {
                DefaultRequestHeaders =
                {
                    {"Accept", "application/json"},
                    {"X-Auth-Token", App.Connection.SessionController.CurrentSession.AuthToken}
                }
            };
            await client.PutAsync(Endpoints.CloudUserRestate.Replace("[:id]", cloud.Id).ReplaceUserId(
                        App.Connection.SessionController.CurrentSession.Id), new StringContent(""));

            cloud = await App.Connection.ModelController.UpdateCloudAsync(cloud);
            App.Connection.SessionController.CurrentSession.Clouds.Add(cloud);
            App.Connection.SessionController.RefreshClouds();
            Main.Instance.Clouds.SelectedIndex = Main.Instance.Clouds.Items.Count - 1;
        }
    }
}
