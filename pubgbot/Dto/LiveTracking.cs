﻿namespace pubgbot.Dto
{
    public class LiveTracking
    {
        public int Match { get; set; }
        public string MatchDisplay { get; set; }
        public int Season { get; set; }
        public int RegionId { get; set; }
        public string Region { get; set; }
        public string Date { get; set; }
        public double Delta { get; set; }
        public double Value { get; set; }
        public string message { get; set; }
    }
}