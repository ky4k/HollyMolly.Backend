namespace HM.BLL.Models.Orders;

public class CustomerCreateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string City { get; set; } = null!;
    public string DeliveryAddress { get; set; } = null!;
}
