using ManagerUser.Application.Common.Models;

namespace ManagerUser.Application.Common.Abstractions.Persistence;

// Repository này xem bảng OutboxMessages như một hàng đợi nằm trong DB.
//
// Khác với Sample D dùng in-memory Channel:
// - Sample D mất job nếu process restart.
// - Sample E giữ message trong DB, worker restart vẫn poll lại được message pending.
public interface IOutboxMessageRepository
{
    // Claim nghĩa là "nhận quyền xử lý tạm thời" một nhóm message.
    // Stored procedure sẽ đổi Status từ Pending sang Processing và đặt LockedUntil.
    // Nhờ vậy nếu có nhiều worker chạy cùng lúc, một message không bị claim trùng trong cùng thời điểm.
    Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(int batchSize, int lockTimeoutSeconds, CancellationToken cancellationToken = default);

    // Sau khi publish/xử lý thành công, message được đánh dấu Processed.
    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default);

    // Nếu publish/xử lý lỗi, message được tăng RetryCount.
    // Nếu chưa quá MaxRetryCount, nó quay lại Pending sau retryDelaySeconds.
    // Nếu đã quá giới hạn, nó chuyển sang Failed.
    Task MarkFailedAsync(Guid id, string error, int retryDelaySeconds, CancellationToken cancellationToken = default);
}
