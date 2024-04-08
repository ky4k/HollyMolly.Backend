﻿namespace HM.BLL.Models;

public class ProfileUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? DeliveryAddress { get; set; }
}
