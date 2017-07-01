using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pubgbot.Dto;

namespace pubgbot
{
    public class Player
    {
        public PlayerData Data { get; set; }

        public Player(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
                throw new ArgumentException("Value cannot be null or empty.", nameof(steamId));
            var web = new HtmlWeb();
            var document = web.Load("https://pubgtracker.com/api/search?steamId="+steamId);
            
            var playerDataRaw = document.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/script[3]")[0].InnerText.Split('=')[1].Split(';')[0];
            var json = JObject.Parse(playerDataRaw);
            Data = JsonConvert.DeserializeObject<PlayerData>(json.ToString());
        }
    }
}
