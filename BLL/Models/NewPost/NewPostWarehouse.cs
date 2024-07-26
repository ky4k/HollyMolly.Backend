using System.Text.Json.Serialization;

namespace HM.BLL.Models.NewPost;

public class NewPostWarehouse
{
    [JsonPropertyName("SiteKey")]
    public string SiteKey { get; set; } = null!;
    [JsonPropertyName("Description")]
    public string Description { get; set; } = null!;
    [JsonPropertyName("DescriptionRu")]
    public string DescriptionRu { get; set; } = null!;
    [JsonPropertyName("ShortAddress")]
    public string ShortAddress { get; set; } = null!;
    [JsonPropertyName("ShortAddressRu")]
    public string ShortAddressRu { get; set; } = null!;
    [JsonPropertyName("Phone")]
    public string Phone { get; set; } = null!;
    [JsonPropertyName("TypeOfWarehouse")]
    public string TypeOfWarehouse { get; set; } = null!;
    [JsonPropertyName("Ref")]
    public string Ref { get; set; } = null!;
    [JsonPropertyName("Number")]
    public string Number { get; set; } = null!;
    [JsonPropertyName("CityRef")]
    public string CityRef { get; set; } = null!;
    [JsonPropertyName("CityDescription")]
    public string CityDescription { get; set; } = null!;
    [JsonPropertyName("CityDescriptionRu")]
    public string CityDescriptionRu { get; set; } = null!;
    [JsonPropertyName("SettlementRef")]
    public string SettlementRef { get; set; } = null!;
    [JsonPropertyName("SettlementDescription")]
    public string SettlementDescription { get; set; } = null!;
    [JsonPropertyName("SettlementAreaDescription")]
    public string SettlementAreaDescription { get; set; } = null!;
    [JsonPropertyName("SettlementRegionsDescription")]
    public string SettlementRegionsDescription { get; set; } = null!;
    [JsonPropertyName("SettlementTypeDescription")]
    public string SettlementTypeDescription { get; set; } = null!;
    [JsonPropertyName("SettlementTypeDescriptionRu")]
    public string SettlementTypeDescriptionRu { get; set; } = null!;
    [JsonPropertyName("Longitude")]
    public string Longitude { get; set; }
    [JsonPropertyName("Latitude")]
    public string Latitude { get; set; }
    [JsonPropertyName("PostFinance")]
    public string PostFinance { get; set; } = null!;
    [JsonPropertyName("BicycleParking")]
    public string BicycleParking { get; set; } = null!;
    [JsonPropertyName("PaymentAccess")]
    public string PaymentAccess { get; set; } = null!;
    [JsonPropertyName("POSTerminal")]
    public string POSTerminal { get; set; } = null!;
    [JsonPropertyName("InternationalShipping")]
    public string InternationalShipping { get; set; } = null!;
    [JsonPropertyName("SelfServiceWorkplacesCount")]
    public string SelfServiceWorkplacesCount { get; set; } = null!;
    [JsonPropertyName("TotalMaxWeightAllowed")]
    public string TotalMaxWeightAllowed { get; set; } = null!;
    [JsonPropertyName("PlaceMaxWeightAllowed")]
    public string PlaceMaxWeightAllowed { get; set; } = null!;
    [JsonPropertyName("SendingLimitationsOnDimensions")]
    public NewPostDimensions SendingLimitationsOnDimensions { get; set; } = null!;
    [JsonPropertyName("ReceivingLimitationsOnDimensions")]
    public NewPostDimensions ReceivingLimitationsOnDimensions { get; set; } = null!;
    [JsonPropertyName("Reception")]
    public NewPostSchedule Reception { get; set; } = null!;
    [JsonPropertyName("Delivery")]
    public NewPostSchedule Delivery { get; set; } = null!;
    [JsonPropertyName("Schedule")]
    public NewPostSchedule Schedule { get; set; } = null!;
    [JsonPropertyName("DistrictCode")]
    public string DistrictCode { get; set; } = null!;
    [JsonPropertyName("WarehouseStatus")]
    public string WarehouseStatus { get; set; } = null!;
    [JsonPropertyName("WarehouseStatusDate")]
    public string WarehouseStatusDate { get; set; } = null!;
    [JsonPropertyName("CategoryOfWarehouse")]
    public string CategoryOfWarehouse { get; set; } = null!;
    [JsonPropertyName("RegionCity")]
    public string RegionCity { get; set; } = null!;
    [JsonPropertyName("WarehouseForAgent")]
    public string WarehouseForAgent { get; set; } = null!;
    [JsonPropertyName("MaxDeclaredCost")]
    public string MaxDeclaredCost { get; set; } = null!;
    [JsonPropertyName("DenyToSelect")]
    public string DenyToSelect { get; set; } = null!;
    [JsonPropertyName("PostMachineType")]
    public string PostMachineType { get; set; } = null!;
    [JsonPropertyName("PostalCodeUA")]
    public string PostalCodeUA { get; set; } = null!;
    [JsonPropertyName("OnlyReceivingParcel")]
    public string OnlyReceivingParcel { get; set; } = null!;
    [JsonPropertyName("WarehouseIndex")]
    public string WarehouseIndex { get; set; } = null!;
}



