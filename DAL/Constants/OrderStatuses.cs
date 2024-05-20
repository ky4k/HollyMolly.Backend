namespace HM.DAL.Constants;

public static class OrderStatuses
{
    public static string Created => "Created";
    public static string PaymentReceived => "Payment Received";
    public static string Processing => "Processing";
    public static string Shipped => "Shipped";
    public static string Delivered => "Delivered";
    public static string Cancelled => "Cancelled";
    private static readonly string[] _validStatuses =
    [
        Created,
        PaymentReceived,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    ];
    public static bool IsValidStatus(string status) => _validStatuses.Contains(status);
}
