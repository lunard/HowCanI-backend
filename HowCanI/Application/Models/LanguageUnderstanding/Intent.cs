using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HowCanI.Application.Models.LanguageUnderstanding
{
    public class Intent
    {
        public Intent()
        {
            Entities = new List<Entity>();
        }
        public string Query { get; set; }

        [JsonProperty("topScoringIntent")]
        public TopScoringIntent TopIntent { get; set; }

        public List<Entity> Entities { get; set; }
    }

    public class TopScoringIntent
    {
        public string Intent { get; set; }

        public double Score { get; set; }
    }

    public class Entity
    {
        [JsonProperty("entity")]
        public string Name { get; set; }

        public string Type { get; set; }

        public double Score { get; set; }
    }
}
