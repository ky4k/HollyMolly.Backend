namespace HM.DAL.Constants;

public static class OrderStatuses
{
    private static readonly string[] _validStatuses =
    [
        "Created",
        "Payment Received",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled"
    ];
    public static bool IsValidStatus(string status) => _validStatuses.Contains(status);
}
