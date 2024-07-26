using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostResponse<T> where T : class
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = null!;
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = null!;
        [JsonPropertyName("translatedErrors")]
        public List<string> TranslatedErrors { get; set; } = null!;
        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = null!;
        [JsonPropertyName("info")]
        public InfoModel Info { get; set; } = null!;
        [JsonPropertyName("messageCodes")]
        public List<string> MessageCodes { get; set; } = null!;
        [JsonPropertyName("errorCodes")]
        public List<string> ErrorCodes { get; set; } = null!;
        [JsonPropertyName("warningCodes")]
        public List<string> WarningCodes { get; set; } = null!;
        [JsonPropertyName("infoCodes")]
        public List<string> InfoCodes { get; set; } = null!;
    }
    public class InfoModel
    {
        [JsonPropertyName("totalCount")]
        public int? TotalCount { get; set; }
    }
}
