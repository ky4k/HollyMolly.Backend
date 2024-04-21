namespace HM.BLL.Models.Common;

public class OperationResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public OperationResult()
    {
    }
    public OperationResult(bool succeeded)
    {
        Succeeded = succeeded;
    }
    public OperationResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }
}

public class OperationResult<T> : OperationResult
{
    public T? Payload { get; set; }

    public OperationResult()
    {
    }
    public OperationResult(bool succeeded) : base(succeeded)
    {
    }
    public OperationResult(bool succeeded, string message) : base(succeeded, message)
    {
    }
    public OperationResult(bool succeeded, T payload) : this(succeeded)
    {
        Payload = payload;
    }

    public OperationResult(bool succeeded, string message, T payload) : this(succeeded, message)
    {
        Payload = payload;
    }
}
