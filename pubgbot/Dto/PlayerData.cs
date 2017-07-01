using System.Collections.Generic;

namespace pubgbot.Dto
{
    /// <summary>
    /// Player data that is scraped from https://pubgtracker.com/api/search?steamId=
    /// </summary>
    public class PlayerData
    {
        public int PubgTrackerId { get; set; }
        public string AccountId { get; set; }
        public int Platform { get; set; }
        public string PlayerName { get; set; }
        public string Avatar { get; set; }
        public string SteamName { get; set; }
        public long SteamId { get; set; }
        public object UserId { get; set; }
        public object selectedMatch { get; set; }
        public string selectedRegion { get; set; }
        public string defaultSeason { get; set; }
        public string selectedSeason { get; set; }
        public string LastUpdated { get; set; }
        public object Twitch { get; set; }
        public List<StatRegion> Stats { get; set; }
        public List<LiveTracking> LiveTracking { get; set; }
        public object MatchHistory { get; set; }
    }
}