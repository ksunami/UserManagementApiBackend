namespace UserApi.Options;

public class RateLimitingOptions
{
    public int Limit { get; set; }
    public int WindowMinutes { get; set; }
}