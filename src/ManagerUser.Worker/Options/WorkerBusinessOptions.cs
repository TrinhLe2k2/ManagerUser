namespace ManagerUser.Worker.Options;

public sealed class WorkerBusinessOptions
{
    public const string SectionName = "WorkerBusiness";
    public int PageSize { get; set; } = 10;
    public int BackgroundIntervalSeconds { get; set; } = 60;
    public bool RunSampleBJobsInParallel { get; set; } = true;
    public int SampleCJobDurationSeconds { get; set; } = 90;
    // Số job tối đa được nằm chờ trong queue Sample D.
    // Khi đầy, producer sẽ chờ để tránh tăng memory vô hạn.
    public int SampleDQueueCapacity { get; set; } = 20;
    // Thời gian giả lập job chậm trong Sample D để dễ thấy queue hoạt động.
    public int SampleDJobDurationSeconds { get; set; } = 2;
}
