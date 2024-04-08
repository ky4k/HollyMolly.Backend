using HM.BLL.Models;
using HM.DAL.Entities;

namespace HM.BLL.Extensions;

public static class MappingExtensions
{
    public static UserDto ToUserDto(this User user, IEnumerable<string> roles)
    {
        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            DeliveryAddress = user.DeliveryAddress,
            Roles = roles
        };
    }
}
