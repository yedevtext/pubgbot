using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pubgbot.Dto;
using PUBGSharp;
using PUBGSharp.Net.Model;

namespace pubgbot
{
    public class Player
    {
        public static async Task<StatsResponse> GetStats(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
                throw new ArgumentException("Value cannot be null or empty.", nameof(steamId));

            var httpClient = new HttpClient();
            var apiKey = ConfigurationManager.AppSettings["trackerHttpHeader"].Split(':')[1].Trim();
            var httpHeader = ConfigurationManager.AppSettings["trackerHttpHeader"].Split(':')[0];

            httpClient.DefaultRequestHeaders.Add(httpHeader, apiKey);

            //Get PUBG Nickname by SteamID(64 - bit number)
            //GET https://pubgtracker.com/api/search?steamId={STEAM ID}
            dynamic pubgNicknamebySteamId = JsonConvert.DeserializeObject(await httpClient.GetStringAsync($"https://pubgtracker.com/api/search?steamId={steamId}"));

            var statsClient = new PUBGStatsClient(apiKey);
            return await statsClient.GetPlayerStatsAsync(pubgNicknamebySteamId.Nickname.ToString());
        }
    }
}
