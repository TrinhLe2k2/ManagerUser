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

    // Chu kỳ Sample E poll bảng OutboxMessages để tìm message pending.
    public int SampleEPollIntervalSeconds { get; set; } = 5;
    // Số message tối đa dispatcher claim trong một lần poll.
    public int SampleEBatchSize { get; set; } = 10;
    // Khi claim message, worker khóa mềm message trong khoảng này.
    // Nếu worker chết giữa chừng, message sẽ được claim lại sau khi hết lock timeout.
    public int SampleELockTimeoutSeconds { get; set; } = 60;
    // Nếu xử lý lỗi nhưng chưa quá MaxRetryCount, message sẽ được hẹn retry sau số giây này.
    public int SampleERetryDelaySeconds { get; set; } = 30;
}
