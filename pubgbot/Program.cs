﻿using System;
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
using Newtonsoft.Json;
using NLog;
using pubgbot.dbcontext;
using pubgbot.Types;
using PUBGSharp.Data;
using Configuration = System.Configuration.Configuration;
using Region = PUBGSharp.Data.Region;

namespace pubgbot
{
    class Program
    {
        private static Logger logToFile = LogManager.GetLogger("filelogger");
        private static Logger logToConsole = LogManager.GetLogger("consolelogger");

        private readonly string _token = ConfigurationManager.AppSettings["token"];
        private DiscordSocketClient _client;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposing = false;
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            _cancellationTokenSource = new CancellationTokenSource();

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
            _cancellationTokenSource.Cancel();
        }

        private async Task MessageHandler(SocketMessage message)
        {
            await AddUser(message);
            await GetUserStats(message);
        }

        //private Task Log(LogMessage msg)
        //{
        //    Console.WriteLine(msg.ToString());
        //    return Task.CompletedTask;
        //}

        private Task Log(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    logToFile.Fatal(msg);
                    logToConsole.Fatal(msg);
                    break;
                case LogSeverity.Error:
                    logToFile.Error(msg);
                    logToConsole.Error(msg);
                    break;
                case LogSeverity.Debug:
                    logToFile.Debug(msg);
                    logToConsole.Debug(msg);
                    break;
                case LogSeverity.Info:
                    logToFile.Info(msg);
                    logToConsole.Info(msg);
                    break;
                case LogSeverity.Warning:
                    logToFile.Warn(msg);
                    logToConsole.Warn(msg);
                    break;
                case LogSeverity.Verbose:
                    logToFile.Trace(msg);
                    logToConsole.Trace(msg);
                    break;
            }
            return Task.CompletedTask;
        }

        #region Bot

        private async Task GetUserStats(SocketMessage message)
        {
            if (message.Content.Contains(BotCommands.StatsMe))
            {
                try
                {
                    using (var context = new pubgdbModel())
                    {
                        var userModel = await context.Users.FirstOrDefaultAsync(
                            user => user.DiscordName == message.Author.Username);
                        var player = await Player.GetStats(userModel.SteamId);
                        var soloRating = player.Stats.FirstOrDefault(
                            region => region.Region == userModel.Region &&
                                      region.Mode == Mode.Solo);
                        var duoRating = player.Stats.FirstOrDefault(
                            region => region.Region == userModel.Region &&
                                      region.Mode == Mode.Duo);
                        var squadRating = player.Stats.FirstOrDefault(
                            region => region.Region == userModel.Region &&
                                      region.Mode == Mode.Squad);
                        logToConsole.Trace(
                            $"Stats for {message.Author.Username} Solo Rating: {soloRating?.Stats.FirstOrDefault(stat => stat.Stat == StatType.Rating)?.Value}, Duo rating: {duoRating?.Stats.FirstOrDefault(stat => stat.Stat == StatType.Rating)?.Value}, Squad Rating: {squadRating?.Stats.FirstOrDefault(stat => stat.Stat == StatType.Rating)?.Value}");
                        await message.Channel.SendMessageAsync(
                            $"Stats for {message.Author.Username} Solo Rating: {soloRating?.Stats.FirstOrDefault(stat => stat.Stat == StatType.Rating)?.Value}, Duo rating: {duoRating?.Stats.FirstOrDefault(stat => stat.Stat == StatType.Rating)?.Value}, Squad Rating: {squadRating?.Stats.FirstOrDefault(stat => stat.Stat == StatType.Rating)?.Value}");
                    }
                }
                catch (Exception e)
                {
                    await message.Channel.SendMessageAsync(
                        $"pubgbot failed to retrieve stats for {message.Author.Username}.");
                    logToFile.Error(e);
                    logToConsole.Error(e);
                    //Console.WriteLine(e);
                    throw;
                }
            }
        }

        private async Task AddUser(SocketMessage message)
        {
            if (message.Content.Contains(BotCommands.AddMe))
            {
                await AddOrUpdateByDiscordUser(message);
                await message.Channel.SendMessageAsync($"{message.Author.Username} has been added for Tracking.");
            }
        }

        #endregion


        #region Data

        private async Task AddOrUpdateByDiscordUser(SocketMessage message)
        {
            User newuser = null;
            User existingUser = null;
            using (var context = new pubgdbModel())
            {
                existingUser = await context.Users.FirstOrDefaultAsync(
                    user => user.DiscordName == message.Author.Username);
                logToConsole.Trace(existingUser == null
                    ? "No User found."
                    : $"User retrieved from database {existingUser.DiscordName}, {existingUser.SteamId}");
                if (existingUser == null)
                {
                    var regiontemp = Regex.Split(message.Content, " ")?[2]?.Trim();


                    newuser = new User()
                    {
                        DiscordName = message.Author.Username,
                        SteamId = Regex.Split(message.Content, " ")[1].Trim(),
                        Region = RegionConverter.Convert(Regex.Split(message.Content, " ")?[2])
                    };

                    context.Users.Add(newuser);
                    await context.SaveChangesAsync();
                    logToConsole.Trace(
                        $"Added new user to database Discord User: {message.Author.Username} --- Database model ---> DiscordName: {newuser.DiscordName}, Steam id: {newuser.SteamId}");
                }
                else
                {
                    existingUser.DiscordName = message.Author.Username;
                    existingUser.SteamId = Regex.Split(message.Content, BotCommands.AddMe)[1].Trim();
                    existingUser.Region =
                        RegionConverter.Convert(Regex.Split(message.Content, BotCommands.AddMe)?[2]?.Trim());
                    //context.Entry(existingUser).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                    logToConsole.Trace(
                        $"Updated existing user to database Discord User: {message.Author.Username} --- Database model ---> DiscordName: {existingUser.DiscordName}, Steam id: {existingUser.SteamId}");
                }
            }
        }

        //private async Task<User> GetByDiscordUser(SocketMessage message)
        //{
        //    using (var context = new pubgdbModel())
        //    {
        //        return await context.Users.FirstOrDefaultAsync(
        //            user => user.DiscordName == message.Author.Username);
        //    }
        //}

        #endregion
    }

    internal class RegionConverter
    {
        private static List<string> regionList = new List<string>() {"AGG", "NA", "EU", "AS", "OC", "SA", "SEA"};

        public static Region Convert(string s)
        {
            if (string.IsNullOrEmpty(s)) return Region.NA;
            if (!regionList.Contains(s.Trim().ToUpper())) return Region.NA;
            switch (s)
            {
                case "AGG":
                    return Region.AGG;
                case "NA":
                    return Region.NA;
                case "EU":
                    return Region.EU;
                case "AS":
                    return Region.AS;
                case "OC":
                    return Region.OC;
                case "SA":
                    return Region.SA;
                case "SEA":
                    return Region.SEA;
                default:
                    return Region.NA;
            }
        }
    }
}