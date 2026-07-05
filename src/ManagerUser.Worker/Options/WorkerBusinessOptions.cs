namespace ManagerUser.Worker.Options;

public sealed class WorkerBusinessOptions
{
    public const string SectionName = "WorkerBusiness";
    public int PageSize { get; set; } = 10;
    public int BackgroundIntervalSeconds { get; set; } = 60;
    public bool RunSampleBJobsInParallel { get; set; } = true;
    public int SampleCJobDurationSeconds { get; set; } = 90;
}
