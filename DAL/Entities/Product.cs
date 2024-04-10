﻿namespace HM.DAL.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
    public int TimesRated { get; set; }
    public string Category { get; set; } = null!;
    public int StockQuantity { get; set; }
    public List<string> Images { get; set; } = new();
    public List<ProductFeedback> Feedbacks { get; set; } = new();
    public List<OrderRecord> OrderRecords { get; set; } = new();
}
