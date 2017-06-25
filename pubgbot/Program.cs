using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;
using pubgbot.dbcontext;
using Configuration = System.Configuration.Configuration;

namespace pubgbot
{
    class Program
    {
        private static string _token = ConfigurationManager.AppSettings["token"];
        private static string _url = "https://pubgtracker.com/api/search?steamId=";
        private static HttpClient _httpClient;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _httpClient = new HttpClient();
            var client = new DiscordSocketClient();

            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, _token);
            await client.StartAsync();

            client.MessageReceived += MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1).ContinueWith(Dispose);
        }

        private void Dispose(Task obj)
        {
            _httpClient.Dispose();
            obj.Dispose();
        }


        private async Task MessageReceived(SocketMessage message)
        {
            await AddUser(message);
            await GetUserStats(message);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


        private static async Task GetUserStats(SocketMessage message)
        {
            if (message.Content.Contains("!statsme"))
            {
                HttpResponseMessage httpresults;
                User userModel;
                using (var context = new pubgdbModel())
                {
                    userModel = await context.Users.FirstOrDefaultAsync(user => user.Name == message.Author.Username);

                    //httpresults = _httpClient.GetAsync(_url + userModel.SteamId).GetAwaiter().GetResult();
                }

                var web = new HtmlWeb();
                var document = web.Load(_url + userModel.SteamId);
                //"https://pubgtracker.com/api/search?steamId=76561198000166636"

                //         //*[@id="profile"]/div[2]/div[2]/div[1]/div[2]/div[2]/div[1]/span[2]
                //var soloRating = document.DocumentNode
                //    .SelectNodes("//*[@id=\"profile\"]/div[2]/div[2]/div[1]/div[2]/div[2]/div[1]/span[2]")
                //    .First()
                //    .Attributes["value"].Value;
                //*[@id="profile"]/div[2]/div[2]/div[1]/div[2]/div[2]/div[1]/span[2]
                //var soloRating = document.DocumentNode.SelectNodes("//*[@id=\"profile\"]/div[2]/div[2]/div[1]/div[2]/div[2]/div[1]/span[2]");
                //Regex.Replace(
                //    $@"{
                //            document.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/script[3]")[0].InnerText
                //                .Split('=')[1]
                //        }", @"\", " ");

                var playerDataRaw = document.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/script[3]")[0]
                    .InnerText.Split('=')[1].Split(';')[0];

                var playerData = playerDataRaw.Replace(@"\", " ");

                var x = playerData;
                await message.Channel.SendMessageAsync($"Stats for {message.Author.Username} SoloRating: ");
            }
        }

        private static async Task AddUser(SocketMessage message)
        {
            if (message.Content.Contains("!addme"))
            {
                using (var context = new pubgdbModel())
                {
                    context.Users.Add(new User()
                    {
                        Name = message.Author.Username,
                        SteamId = Regex.Split(message.Content, "!addme")[1].Trim()
                    });

                    await context.SaveChangesAsync();
                }
                await message.Channel.SendMessageAsync($"Pong! {message.Author.Username}");
            }
        }
    }
}