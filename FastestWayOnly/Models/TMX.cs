using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FastestWayOnly.Models
{
    public class TMX
    {
        public class JsonUser
        {
            public int UserId { get; set; }
            public string Name { get; set; }
        }
        public class JsonReplay
        {
            public int ReplayId { get; set; }
            public int ReplayTime { get; set; }
            public int ReplayScore { get; set; }
            public JsonUser User { get; set; }
        }
        public class JsonTrack
        {
            public int TrackId { get; set; }
            public string TrackName { get; set; }
            public int PrimaryType { get; set; }
            public JsonReplay WRReplay { get; set; }
        }

        public class JsonRecordList
        {
            public bool More { get; set; }
            public JsonTrack[] Results { get; set; }
        }

        public class HtmlRecordList
        {
            public string UrlStart { get; set; }
            public int MapCount { get; set; }
            public int LBType { get; set; }
            public JsonRecordList RecordList { get; set; }
            public HtmlRecordList(string urlStart, int mapCount, int lBType)
            {
                UrlStart = urlStart;
                MapCount = mapCount;
                LBType = lBType;
                RecordList = new JsonRecordList { Results = new JsonTrack[0] };
            }

            public void GetRecords()
            {
                GetTM1XRecords(UrlStart, MapCount, LBType, RecordList);
            }

            public string GetTableHtml()
            {
                string str = "<table><tr><th style=\"text-align: left\">Track</th><th style=\"text-align: left\">Player</th><th style=\"text-align: left\">Time</th></tr>";
                foreach (var result in RecordList.Results)
                {
                    string recordText = result.PrimaryType == 3 ? result.WRReplay.ReplayScore + "pts" : MillisToTime(result.WRReplay.ReplayTime);
                    str += $"<tr><td style=\"text-align: left\"><a href=\"{UrlStart}/trackshow/{result.TrackId}\">{result.TrackName}</a></td>" +
                        $"<td style=\"text-align: left\"><span style=\"font-weight: bold\">{result.WRReplay.User.Name}</span></td>" +
                        $"<td style=\"text-align: left\"><a href=\"{UrlStart}/recordgbx/{result.WRReplay.ReplayId}\">{recordText}</a></td></tr>";
                }
                str += "</table>";
                return str;
            }

            public void MoveToListIfContains(HtmlRecordList destination, string[] sequences)
            {
                List<JsonTrack> newDestinationResults = new();

                foreach (var result in RecordList.Results)
                {
                    if (sequences.Any((s) => result.TrackName.Contains(s)))
                    {
                        newDestinationResults.Add(result);
                    }
                }
                destination.RecordList.Results = newDestinationResults.ToArray();
            }

            public void MoveToListIfNotContains(HtmlRecordList destination, string[] sequences)
            {
                List<JsonTrack> newDestinationResults = new();

                foreach (var result in RecordList.Results)
                {
                    if (!sequences.Any((s) => result.TrackName.Contains(s)))
                    {
                        newDestinationResults.Add(result);
                    }
                }
                destination.RecordList.Results = newDestinationResults.ToArray();
            }
        }

        public static HtmlRecordList TMNFRecords = new HtmlRecordList("https://tmnf.exchange", 65, 2);
        public static HtmlRecordList ESWCRecords = new HtmlRecordList("https://nations.tm-exchange.com", 121, 2);
        public static HtmlRecordList TMUXRecords = new HtmlRecordList("https://tmuf.exchange", 711, 2);
        public static HtmlRecordList StarRecords = new HtmlRecordList("https://tmuf.exchange", 147, 5);
        public static HtmlRecordList TMUFRecords = new HtmlRecordList("https://tmuf.exchange", 245, 0);
        public static HtmlRecordList TMNRemakeRecords = new HtmlRecordList("https://tmuf.exchange", 120, 0);
        public static HtmlRecordList TMORecords = new HtmlRecordList("https://tmuf.exchange", 182, 0);
        public static HtmlRecordList TMSRecords = new HtmlRecordList("https://tmuf.exchange", 164, 0);

        static string MillisToTime(int time, bool millis = false)
        {
            int minutes = time / 60_000;
            int seconds = (time % 60_000) / 1_000;
            int fractions = millis ? time % 1_000 : (time % 1_000) / 10;
            return $"{minutes.ToString("d2")}:{seconds.ToString("d2")}.{fractions.ToString("d2")}";
        }

        static HttpClient client = new HttpClient();

        static void GetTM1XRecordsFromJson(string jsonString, JsonRecordList recordList)
        {
            var allRecords = JsonConvert.DeserializeObject<JsonRecordList>(jsonString);
            //Debug.WriteLine("More: " + allRecords.More + ", Results: " + allRecords.Results.Length);
            List<JsonTrack> results = new List<JsonTrack>();
            foreach (var result in allRecords.Results)
            {
                if (Array.Exists(Models.Member.GetLogins(), id => id.Equals((ulong)result.WRReplay.User.UserId)))
                {
                    results.Add(result);
                }
            }

            recordList.Results = results.ToArray();
        }
        static async Task GetTM1XRecordsWithRequest(string urlStart, int mapCount, int lBType, JsonRecordList recordList)
        {
            Debug.WriteLine($"Requesting {mapCount} records from {urlStart}");
            var jsonContent = await client.GetAsync($"{urlStart}/api/tracks?lbtype={lBType}&order1=11&count={mapCount}&fields=TrackId,TrackName,PrimaryType,WRReplay.ReplayId,WRReplay.ReplayTime,WRReplay.ReplayScore,WRReplay.User.UserId,WRReplay.User.Name");
            var jsonString = await jsonContent.Content.ReadAsStringAsync();
            GetTM1XRecordsFromJson(jsonString, recordList);
        }

        static void GetTM1XRecords(string urlStart, int mapCount, int lBType, JsonRecordList recordList)
        {
            GetTM1XRecordsWithRequest(urlStart, mapCount, lBType, recordList).Wait();

        }
        static void TMXCycle()
        {
            TMNFRecords.GetRecords();
            ESWCRecords.GetRecords();
            TMUXRecords.GetRecords();
            StarRecords.GetRecords();
            TMUXRecords.MoveToListIfContains(TMNRemakeRecords, new string[] { "TMN" });
            TMUXRecords.MoveToListIfContains(TMORecords, new string[] { "TMO", "TM Demo", "TMO Demo", "TMPU" });
            TMUXRecords.MoveToListIfContains(TMSRecords, new string[] { "TMS" });
            TMUXRecords.MoveToListIfNotContains(TMUFRecords, new string[] { "TMN", "TMO", "TMS", "Demo" });

            // Refresh daily
            Thread.Sleep(86_400_000);
            TMXCycle();
        }

        public static void StartTMXCycle()
        {
            Debug.WriteLine("Starting TMX cycle...");
            new Thread(TMXCycle).Start();
        }
    }
}
