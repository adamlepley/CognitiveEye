using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CongnitiveEye.Forms.Models
{

    public partial class TrainingError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class TrainingError
    {
        public static TrainingError FromJson(string json) => JsonConvert.DeserializeObject<TrainingError>(json);
    }

    public static class Serialize
    {
        public static string ToJson(this TrainingError self) => JsonConvert.SerializeObject(self);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
