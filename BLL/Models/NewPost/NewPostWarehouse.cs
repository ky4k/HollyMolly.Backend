namespace HM.BLL.Models.NewPost;

public class NewPostWarehouse
{
    public string SiteKey { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DescriptionRu { get; set; } = null!;
    public string ShortAddress { get; set; } = null!;
    public string ShortAddressRu { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string TypeOfWarehouse { get; set; } = null!;
    public string Ref { get; set; } = null!;
    public string Number { get; set; } = null!;
    public string CityRef { get; set; } = null!;
    public string CityDescription { get; set; } = null!;
    public string CityDescriptionRu { get; set; } = null!;
    public string SettlementRef { get; set; } = null!;
    public string SettlementDescription { get; set; } = null!;
    public string SettlementAreaDescription { get; set; } = null!;
    public string SettlementRegionsDescription { get; set; } = null!;
    public string SettlementTypeDescription { get; set; } = null!;
    public string SettlementTypeDescriptionRu { get; set; } = null!;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public string PostFinance { get; set; } = null!;
    public string BicycleParking { get; set; } = null!;
    public string PaymentAccess { get; set; } = null!;
    public string POSTerminal { get; set; } = null!;
    public string InternationalShipping { get; set; } = null!;
    public string SelfServiceWorkplacesCount { get; set; } = null!;
    public string TotalMaxWeightAllowed { get; set; } = null!;
    public string PlaceMaxWeightAllowed { get; set; } = null!;
    public NewPostDimensions SendingLimitationsOnDimensions { get; set; } = null!;
    public NewPostDimensions ReceivingLimitationsOnDimensions { get; set; } = null!;
    public NewPostSchedule Reception { get; set; } = null!;
    public NewPostSchedule Delivery { get; set; } = null!;
    public NewPostSchedule Schedule { get; set; } = null!; 
    public string DistrictCode { get; set; } = null!;           
    public string WarehouseStatus { get; set; } = null!;
    public string WarehouseStatusDate { get; set; } = null!;
    public string CategoryOfWarehouse { get; set; } = null!;
    public string RegionCity { get; set; } = null!;
    public string WarehouseForAgent { get; set; } = null!;
    public string MaxDeclaredCost { get; set; } = null!;
    public string DenyToSelect { get; set; } = null!;
    public string PostMachineType { get; set; } = null!;
    public string PostalCodeUA { get; set; } = null!;
    public string OnlyReceivingParcel { get; set; } = null!;
    public string WarehouseIndex { get; set; } = null!;
}



