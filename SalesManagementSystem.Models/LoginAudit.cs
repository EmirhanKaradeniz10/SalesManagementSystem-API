public class LoginAudit
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string? IpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime LoginTime { get; set; }

    public string? UserAgent { get; set; }
    public string? FailureReason { get; set; }
}