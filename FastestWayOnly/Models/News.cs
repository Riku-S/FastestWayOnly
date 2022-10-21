using Discord;
using Discord.WebSocket;
using System.Diagnostics;

namespace FastestWayOnly.Models
{
    public class News
    {
        const int NewsCount = 6;
        public static List<string> NewsMessages = new List<string>();
        static DiscordSocketClient _client;
        static string MakeYoutubeEmbed(string link)
        {
            string[] parts = link.Split('/');
            return "<br /><iframe width=\"320\" height=\"263\" src=\"https://www.youtube.com/embed/" + parts[parts.Length - 1] + "\"></iframe>";
        }

        static string MakeNewsHtml(string message)
        {
            message = message.Trim();
            string embed = "";
            string total = "";

            int linecount = 0;
            foreach (string line in message.Split('\n'))
            {
                if (linecount++ > 0)
                    total += "<br />";

                int wordCount = 0;
                foreach (string word in line.Trim().Split(' '))
                {
                    if (wordCount == 0 & word.StartsWith('@'))
                        continue;

                    if (wordCount++ > 0)
                        total += " ";

                    if (word.StartsWith("http://") || word.StartsWith("https://"))
                    {
                        if (word.Contains("youtube.com") || word.Contains("youtu.be"))
                            embed = MakeYoutubeEmbed(word);

                        string newWord = "<a href='" + word + "'>" + word + "</a>";
                        total += newWord;
                    }
                    else
                    {
                        total += word;
                    }
                }
            }
            return total + embed;
        }

        static string ReturnEmptyIfEmote(string message)
        {
            string msg = message.Trim();
            if (msg.StartsWith(':') && msg.EndsWith(':') && !msg.Contains(' '))
                return "";

            return message;
        }

        static async Task GetNews()
        {
            Debug.WriteLine("Gathering news...");



            await _client.LoginAsync(TokenType.Bot, Properties.EnvVars.DiscordBotToken);
            await _client.StartAsync();

            var channel = await _client.GetChannelAsync(Properties.EnvVars.DiscordNewsChannelId) as ITextChannel;

            if (channel != null)
            {
                await channel.GetMessagesAsync(NewsCount).ForEachAsync((messages) =>
                {
                    foreach (var message in messages.ToList())
                    {
                        if (!String.IsNullOrEmpty(ReturnEmptyIfEmote(message.CleanContent.Trim())))
                        {
                            NewsMessages.Add(MakeNewsHtml(message.Content));
                        }
                    }
                });
            }
            await _client.StopAsync();
        }

        static void NewsCycle()
        {
            while (true)
            {
                GetNews().Wait();
                // Refresh hourly
                Thread.Sleep(3_600_000);
            }
        }

        public static void StartNewsCycle()
        {
            Debug.WriteLine("Starting news cycle...");
            DiscordSocketConfig discordSocketConfig = new DiscordSocketConfig();
            discordSocketConfig.GatewayIntents = GatewayIntents.All;
            _client = new DiscordSocketClient(discordSocketConfig);

            new Thread(NewsCycle).Start();
        }
    }
}
