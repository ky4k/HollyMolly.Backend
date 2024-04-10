namespace HM.DAL.Entities;

public class CustomerInfo
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string City { get; set; } = null!;
    public string DeliveryAddress { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
