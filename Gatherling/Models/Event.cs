using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatherling.Models
{
    public class Event
    {
        internal IGatherlingApi Gatherling;

        [JsonProperty("name")]
        public string Name { get; set; }


        public string Channel { get; set; }

        public string Series { get; set; }

        public string Host { get; set; }

        public SubEvent Main { get; set; }

        public SubEvent Finals { get; set; }

        public string[] Unreported { get; set; }

        protected internal Standing[] Standings;

        public Task<Round> GetCurrentPairings()
        {
            return Gatherling.GetCurrentPairings(this);
        }

        public Dictionary<int, Round> Rounds { get; } = new Dictionary<int, Round>();

        public override string ToString()
        {
            return $"<{Name}>";
        }

        public override bool Equals(object obj)
        {
            if (obj is Event e)
                return e.Name == this.Name;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public Event(IGatherlingApi api)
        {
            Gatherling = api ?? throw new ArgumentNullException(nameof(api));
        }

        public Event(string name, JObject data, IGatherlingApi api)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Event Name is null", nameof(name));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Gatherling = api ?? throw new ArgumentNullException(nameof(api));
            Name = name;
            Channel = data.Value<string>("mtgo_room");
            if (Channel != null && !Channel.StartsWith("#"))
                Channel = "#" + Channel;
            Series = data.Value<string>("series");
            //Unreported = data.Value<string[]>("unreported");
            //Standings = data.Value<Standing[]>("standings");
        }
    }
}
