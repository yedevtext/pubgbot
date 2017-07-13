using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;
using pubgbot.dbcontext;
using pubgbot.Types;
using Configuration = System.Configuration.Configuration;

namespace pubgbot
{
    class Program
    {
        private readonly string _token = ConfigurationManager.AppSettings["token"];
        private DiscordSocketClient _client;
        private pubgdbModel _pubgdbModel;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposing = false;
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            _cancellationTokenSource = new CancellationTokenSource();
            _pubgdbModel = new pubgdbModel();
            _client = new DiscordSocketClient();
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            _client.MessageReceived += MessageHandler;

            // Block this task until the program is closed.
            try
            {
                await Task.Delay(-1, _cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                // sowallow  the cancelation exception message as the app tries to restart on exception.
            }
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            if (!_isDisposing)
                Dispose();
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
        {
            if (!_isDisposing)
                Dispose();
        }


        /// <summary>
        /// hmm.... not sure this is proper or being used as i think it would be.
        /// I wanted to dispose of items after the process was closed.
        /// somehow after the Task.Delay(-1) being killed.
        /// </summary>
        /// <param name="obj"></param>
        public void Dispose()
        {
            _isDisposing = true;
            _client.Dispose();
            _pubgdbModel.Dispose();
            _cancellationTokenSource.Cancel();
        }

        private async Task MessageHandler(SocketMessage message)
        {
            await AddUser(message);
            await GetUserStats(message);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        #region Bot

        private async Task GetUserStats(SocketMessage message)
        {
            if (message.Content.Contains(BotCommands.StatsMe))
            {
                var userModel = await GetByDiscordUser(message);
                var player = new Player(userModel.SteamId);
                var soloRating = player.Data.Stats.FirstOrDefault(region => region.Region == userModel.Region &&
                                                                   region.Match == MatchType.Solo);
                var duoRating = player.Data.Stats.FirstOrDefault(region => region.Region == userModel.Region &&
                                                                   region.Match == MatchType.Duo);
                var squadRating = player.Data.Stats.FirstOrDefault(region => region.Region == userModel.Region &&
                                                                  region.Match == MatchType.Squad);

                await message.Channel.SendMessageAsync(
                    $"Stats for {message.Author.Username} Solo Rating: {soloRating?.Stats.FirstOrDefault(stat=> stat.field == StatType.Rating)?.displayValue}, Duo rating: {duoRating?.Stats.FirstOrDefault(stat => stat.field == StatType.Rating)?.displayValue}, Squad Rating: {squadRating?.Stats.FirstOrDefault(stat => stat.field == StatType.Rating)?.displayValue}");

                await UpdateStats(userModel, player);
            }
        }

        private async Task UpdateStats(User userModel, Player player)
        {
            using (var context = new pubgdbModel())
            {
                var oldStats = context.Stats.Where(stat => stat.SteamId == userModel.SteamId);
                if(oldStats.Any())
                    context.Stats.RemoveRange(oldStats);

                var newStats = player.Data.Stats.Where(region => region.Region == userModel.Region);
                //var stats = newStats.Where(region => region.Region)
                await Task.Delay(2);
                //return Task.Delay(2);
            }
        }

        private async Task AddUser(SocketMessage message)
        {
            if (message.Content.Contains(BotCommands.AddUser))
            {
                await AddOrUpdateByDiscordUser(message);
                await message.Channel.SendMessageAsync($"{message.Author.Username} has been added for Tracking.");
            }
        }

        #endregion


        #region Data

        private async Task AddOrUpdateByDiscordUser(SocketMessage message)
        {
            var existingUser = await GetByDiscordUser(message);

            if (existingUser == null)
            {
                _pubgdbModel.Users.Add(new User()
                {
                    DiscordName = message.Author.Username,
                    SteamId = Regex.Split(message.Content, " ")[1].Trim(),
                    Region = Regex.Split(message.Content, " ")?[2]?.Trim()
                });
                await _pubgdbModel.SaveChangesAsync();
            }
            else
            {
                existingUser.DiscordName = message.Author.Username;
                existingUser.SteamId = Regex.Split(message.Content, BotCommands.AddUser)[1].Trim();
                existingUser.Region = Regex.Split(message.Content, BotCommands.AddUser)?[2]?.Trim();

                _pubgdbModel.Users.Attach(existingUser);
                await _pubgdbModel.SaveChangesAsync();
            }
        }

        private async Task<User> GetByDiscordUser(SocketMessage message)
        {
            return await _pubgdbModel.Users.FirstOrDefaultAsync(user => user.DiscordName == message.Author.Username);
        }

        #endregion
    }
}