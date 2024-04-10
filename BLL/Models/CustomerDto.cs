namespace HM.BLL.Models;

public class CustomerDto
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string City { get; set; } = null!;
    public string DeliveryAddress { get; set; } = null!;
}
