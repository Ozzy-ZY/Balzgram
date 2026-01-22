namespace Infrastructure.Configurations;

public class QueryPerformanceSettings
{
    public const string SectionName = "QueryPerformance";
    
    public int SlowQueryThresholdMs { get; set; } = 10;
}
