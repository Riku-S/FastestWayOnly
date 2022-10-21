using System.Diagnostics;
using System.Text.Json.Nodes;

namespace FastestWayOnly.Models
{
    public class Twitch
    {
        static HttpClient client = new HttpClient();
        public static List<string> ChannelNamesTwitch = new List<string>();
        static async Task GetStreamsTwitch()
        {
            string requestString = "https://api.twitch.tv/helix/streams?";
            int memberCount = 0;
            foreach (Member member in Member.GetMembers())
            {
                if (!string.IsNullOrEmpty(member.Twitch))
                {
                    string andString = memberCount++ == 0 ? "" : "&";
                    requestString += andString + "user_login=" + member.Twitch;
                }
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Properties.EnvVars.TwitchAuth);
            client.DefaultRequestHeaders.Add("Client-Id", Properties.EnvVars.TwitchClientId);

            HttpResponseMessage response = await client.GetAsync(requestString);
            string contents = await response.Content.ReadAsStringAsync();

            List<string> channelNames = new List<string>();

            var node = JsonNode.Parse(contents);
            if (node != null)
            {
                var nodeObject = node.AsObject();
                var dataNode = nodeObject["data"];
                if (dataNode != null)
                {
                    foreach (var item in dataNode.AsArray())
                    {
                        if (item != null && item["type"] != null && item["type"].ToString() == "live")
                        {
                            channelNames.Add(item["user_name"].ToString());
                        }
                    }
                }
            }
            ChannelNamesTwitch = channelNames;
        }
        static void StreamCycle()
        {
            while (true)
            {
                GetStreamsTwitch().Wait();
                // Refresh every 5 minutes
                Thread.Sleep(300_000);
            }
        }
        public static void StartStreamCycle()
        {
            Debug.WriteLine("Starting Twitch cycle...");
            new Thread(StreamCycle).Start();
        }
    }
}
