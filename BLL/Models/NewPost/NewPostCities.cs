using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostCities
    {
        [JsonPropertyName("Description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("DescriptionRu")]
        public string DescriptionRu { get; set; } = null!;

        [JsonPropertyName("Ref")]
        public string Ref { get; set; } = null!;

        [JsonPropertyName("Delivery1")]
        public string Delivery1 { get; set; } = null!;

        [JsonPropertyName("Delivery2")]
        public string Delivery2 { get; set; } = null!;

        [JsonPropertyName("Delivery3")]
        public string Delivery3 { get; set; } = null!;

        [JsonPropertyName("Delivery4")]
        public string Delivery4 { get; set; } = null!;

        [JsonPropertyName("Delivery5")]
        public string Delivery5 { get; set; } = null!;

        [JsonPropertyName("Delivery6")]
        public string Delivery6 { get; set; } = null!;

        [JsonPropertyName("Delivery7")]
        public string Delivery7 { get; set; } = null!;

        [JsonPropertyName("Area")]
        public string Area { get; set; } = null!;

        [JsonPropertyName("SettlementType")]
        public string SettlementType { get; set; } = null!;

        [JsonPropertyName("IsBranch")]
        public string IsBranch { get; set; } = null!;

        [JsonPropertyName("PreventEntryNewStreetsUser")]
        public string PreventEntryNewStreetsUser { get; set; } = null!;

        [JsonPropertyName("Conglomerates")]
        public string Conglomerates { get; set; } = null!;

        [JsonPropertyName("CityID")]
        public string CityID { get; set; } = null!;

        [JsonPropertyName("SettlementTypeDescriptionRu")]
        public string SettlementTypeDescriptionRu { get; set; } = null!;

        [JsonPropertyName("SettlementTypeDescription")]
        public string SettlementTypeDescription { get; set; } = null!;
    }
}
