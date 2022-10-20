# fastest-way-only-fans
 Fastest Way Only website FastestWayOnlyFans.
 
 Made by Ors with ASP.NET core. 
 
 The site has following pages:
 - Home page with live streams on Twitch, latest YouTube videos of FWO members and a manually updated news feed (on a news Discord channel).
 - About page with a brief introduction to FWO and the recruitment process
 - Member list
 - List of all of our world records on Trackmania Exchange
 
 As of documenting this, the site is live at http://fastestwayonly.fans/.
 
 Known issues: Mobile view looks like shit with tables and news containers too wide.
 
 The file FastestWayOnly/Properties/EnvVars.cs is not included in the git repository. The file includes:
 - public const string YouTubeAPIKey = "[Youtube API key]";
 - public const string TwitchAuth = "[Twitch auth key]";
 - public const string TwitchClientId = "[Twitch client id]";
 - public const string DiscordBotToken = "[Discord bot token]";
 - public const ulong DiscordNewsChannelId = [Discord news channel id];