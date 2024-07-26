using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostSchedule
    {
        [JsonPropertyName("Monday")]
        public string Monday { get; set; } = null!;
        [JsonPropertyName("Tuesday")]
        public string Tuesday { get; set; } = null!;
        [JsonPropertyName("Wednesday")]
        public string Wednesday { get; set; } = null!;
        [JsonPropertyName("Thursday")]
        public string Thursday { get; set; } = null!;
        [JsonPropertyName("Friday")]
        public string Friday { get; set; } = null!;
        [JsonPropertyName("Saturday")]
        public string Saturday { get; set; } = null!;
        [JsonPropertyName("Sunday")]
        public string Sunday { get; set; } = null!;
    }
}
