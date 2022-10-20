using Newtonsoft.Json;
using System.Diagnostics;

namespace FastestWayOnly.Models
{
    public class YouTube
    {
        static HttpClient client = new HttpClient();
        public static List<YouTubePlaylistSnippet> VideoList = new List<YouTubePlaylistSnippet>();
        public class YouTubeRelatedPlaylists
        {
            public string uploads { get; set; }
        }
        public class YouTubeContentDetails
        {
            public YouTubeRelatedPlaylists relatedPlaylists { get; set; }
        }
        public class YouTubeChannel
        {
            public YouTubeContentDetails contentDetails { get; set; }
        }

        public class YouTubeChannelListResponse
        {
            public YouTubeChannel[] items { get; set; }
        }
        public class YouTubeVideoId
        {
            public string videoId { get; set; }
        }
        public class YouTubePlaylistSnippet
        {
            public string publishedAt { get; set; }
            public string title { get; set; }
            public YouTubeVideoId resourceId { get; set; }
        }
        public class YouTubePlaylistItemListItem
        {
            public YouTubePlaylistSnippet snippet { get; set; }
        }
        public class YouTubePlaylistItemListResponse
        {
            public YouTubePlaylistItemListItem[] items { get; set; }
        }


        async static Task<string> GetPlaylistID(string userID)
        {
            string requestString = $"https://youtube.googleapis.com/youtube/v3/channels?part=snippet%2CcontentDetails&id={userID}&key={Properties.EnvVars.YouTubeAPIKey}";

            client.DefaultRequestHeaders.Clear();
            var responseMessage = await client.GetAsync(requestString);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (responseContent != null)
            {
                YouTubeChannelListResponse response = JsonConvert.DeserializeObject<YouTubeChannelListResponse>(responseContent);
                if (response.items != null && response.items.Length > 0)
                {
                    return response.items[0].contentDetails.relatedPlaylists.uploads;
                }
            }

            return "";
        }

        static async Task RequestVideosFromPlaylist(string playlistID)
        {
            client.DefaultRequestHeaders.Clear();
            string requestString = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={playlistID}&maxResults=2&key={Properties.EnvVars.YouTubeAPIKey}";
            var responseMessage = await client.GetAsync(requestString);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (responseContent != null)
            {
                YouTubePlaylistItemListResponse response = JsonConvert.DeserializeObject<YouTubePlaylistItemListResponse>(responseContent);
                if (response.items != null && response.items.Length > 0)
                {
                    foreach (var item in response.items)
                    {
                        VideoList.Add(item.snippet);
                    }
                }
            }
        }

        static async Task RequestVideos()
        {
            Debug.WriteLine("Requesting videos from Youtube");

            List<string> playlistIDs = new List<string>();

            foreach (Member member in Member.GetMembers())
            {
                if (!string.IsNullOrEmpty(member.YouTube.Trim()))
                {
                    string playlistId = GetPlaylistID(member.YouTube).Result;
                    playlistIDs.Add(playlistId);
                }
            }

            foreach (string playlistID in playlistIDs)
            {
                if (!string.IsNullOrEmpty(playlistID))
                {
                    await RequestVideosFromPlaylist(playlistID);
                }
            }

            VideoList.Sort((a, b) => b.publishedAt.CompareTo(a.publishedAt));
            if (VideoList.Count > 12)
            {
                VideoList = VideoList.GetRange(0, 12);
            }
        }

        static void VideoCycle()
        {
            RequestVideos().Wait();
            // Refresh daily
            Thread.Sleep(86_400_000);
            VideoCycle();
        }

        public static void StartVideoCycle()
        {
            Debug.WriteLine("Starting YouTube cycle...");
            new Thread(VideoCycle).Start();
        }
    }
}
