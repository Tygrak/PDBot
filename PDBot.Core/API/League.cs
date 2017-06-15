﻿using Newtonsoft.Json.Linq;
using PDBot.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.API
{
    public static class League
    {
        private static string API_TOKEN = null;

        static League()
        {
            if (File.Exists("PDM_API_KEY.txt"))
                API_TOKEN = File.ReadAllText("PDM_API_KEY.txt");
            else
                API_TOKEN = "null";
        }

        public class Card
        {

            public Card(JToken blob)
            {
                Name = new CardName(blob.Value<string>("name"));
                Quantity = blob.Value<int>("n");
            }

            public int Quantity { get; private set; }
            public CardName Name { get; private set; }
        }

        public class Deck
        {

            public Deck(JToken blob)
            {
                Id = blob.Value<int>("id");
                Name = blob.Value<string>("name");
                Person = blob.Value<string>("person");
                Wins = blob.Value<int>("wins");
                Losses = blob.Value<int>("losses");
                Draws = blob.Value<int>("draws");
                HasRecord = blob.Value<bool>("has_record");
                CanPlay = blob.SelectToken("can_play")?.Select(jt => jt.ToObject<string>())?.ToArray();
                MainBoard = blob.SelectToken("maindeck").Select(c => new Card(c)).ToArray();
                SideBoard = blob.SelectToken("sideboard").Select(c => new Card(c)).ToArray();
                CompetitionName = blob.Value<string>("competition_name");
            }

            public string Name { get; internal set; }
            public string Person { get; internal set; }
            public int Wins { get; internal set; }
            public int Losses { get; internal set; }
            public int Draws { get; internal set; }
            public string[] CanPlay { get; internal set; }
            public bool HasRecord { get; private set; }
            public Card[] MainBoard { get; private set; }
            public Card[] SideBoard { get; private set; }
            public string CompetitionName { get; private set; }
            public int Id { get; private set; }

            public override string ToString()
            {
                string str = $"{Name} by {Person}";
                if (HasRecord)
                {
                    str += $" ({Wins}-{Losses})";
                }
                return str;
            }

            public bool ContainsCard(string name)
            {
                return MainBoard.Any(c => c.Name.Equals(name)) || SideBoard.Any(c => c.Name.Equals(name));
            }

            public bool Retire()
            {
                System.Collections.Specialized.NameValueCollection nameValueCollection = new System.Collections.Specialized.NameValueCollection();
                nameValueCollection.Add("api_token", API_TOKEN);
                var v = Encoding.UTF8.GetString(wc.UploadValues($"/api/league/drop/{Person}", nameValueCollection));
                File.WriteAllText("drop.json", v);
                var blob = JToken.Parse(v);
                if (blob.Type == JTokenType.Null)
                {
                    return false;
                }

                bool error = ((blob as JObject).TryGetValue("error", out var _));
                return !error;
            }
        }

        static WebClient wc => new WebClient()
        {
            BaseAddress = "http://pennydreadfulmagic.com/",
            Encoding=Encoding.UTF8
        };

        public static Deck GetRunSync(string player)
        {
            var task = GetRun(player);
            task.Wait();
            return task.Result;
        }

        public static async Task<Deck> GetRun(string player)
        {
            try
            {

                string v = await wc.DownloadStringTaskAsync($"/api/league/run/{player}");
                var blob = JToken.Parse(v);
                if (blob.Type == JTokenType.Null)
                {
                    return null;
                }

                Deck run = new Deck(blob);
                return run;
            }
            catch (Exception c)
            {
                Console.WriteLine(c);
                return null;
            }
        }

        public static Deck GetDeck(int id)
        {
            var blob = wc.DownloadString($"/api/decks/{id}");
            JObject jObject = JObject.Parse(blob);
            if (jObject.Type == JTokenType.Null)
                return null;
            return new Deck(jObject);
        }

        public static void UploadResults(Deck winningRun, Deck losingRun, string record)
        {
            wc.UploadValues("/report/", new System.Collections.Specialized.NameValueCollection
            {
                //{ "api_token", API_TOKEN },
                { "entry", winningRun.Id.ToString() },
                { "opponent", losingRun.Id.ToString() },
                { "result", record },
                { "draws", "0" }
            });
        }
    }
}
