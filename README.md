# Phím tắt
- Remove and Sort Usings: Ctrl + R + G

# Triển Khai Dự Án User Management

Công nghệ chính:
- .NET Web API, .NET 8.0
- Dapper ORM
- SQL Server và Stored Procedure
- BackgroundService / Worker Service
- Hangfire
- Quartz.NET
- RabbitMQ
- Kafka
- Outbox Pattern
- Redis
- Elasticsearch

# Tạo Solution Ban Đầu
Chạy tại thư mục bạn muốn tạo dự án, ví dụ:
```powershell
mkdir C:\Manager\ManagerUser\Source
cd C:\Manager\ManagerUser\Source
```
Tạo solution:
```powershell
mkdir C:\Manager\ManagerUser\Source
cd C:\Manager\ManagerUser\Source
```
Tạo các project:
```powershell
dotnet new webapi -n ManagerUser.Api -o src/ManagerUser.Api --framework net8.0 --use-controllers
dotnet new classlib -n ManagerUser.Application -o src/ManagerUser.Application --framework net8.0
dotnet new classlib -n ManagerUser.Domain -o src/ManagerUser.Domain --framework net8.0
dotnet new classlib -n ManagerUser.Infrastructure -o src/ManagerUser.Infrastructure --framework net8.0
dotnet new worker -n ManagerUser.Worker -o src/ManagerUser.Worker --framework net8.0
dotnet new classlib -n ManagerUser.Contracts -o src/ManagerUser.Contracts --framework net8.0
```
Thêm project vào solution:
```powershell
dotnet sln add src/ManagerUser.Api/ManagerUser.Api.csproj
dotnet sln add src/ManagerUser.Application/ManagerUser.Application.csproj
dotnet sln add src/ManagerUser.Domain/ManagerUser.Domain.csproj
dotnet sln add src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj
dotnet sln add src/ManagerUser.Worker/ManagerUser.Worker.csproj
dotnet sln add src/ManagerUser.Contracts/ManagerUser.Contracts.csproj
```
Thêm reference giữa các project:
```powershell
dotnet add src/ManagerUser.Api/ManagerUser.Api.csproj reference src/ManagerUser.Application/ManagerUser.Application.csproj
dotnet add src/ManagerUser.Api/ManagerUser.Api.csproj reference src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj
dotnet add src/ManagerUser.Application/ManagerUser.Application.csproj reference src/ManagerUser.Domain/ManagerUser.Domain.csproj
dotnet add src/ManagerUser.Application/ManagerUser.Application.csproj reference src/ManagerUser.Contracts/ManagerUser.Contracts.csproj
dotnet add src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj reference src/ManagerUser.Application/ManagerUser.Application.csproj
dotnet add src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj reference src/ManagerUser.Domain/ManagerUser.Domain.csproj
dotnet add src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj reference src/ManagerUser.Contracts/ManagerUser.Contracts.csproj
dotnet add src/ManagerUser.Worker/ManagerUser.Worker.csproj reference src/ManagerUser.Application/ManagerUser.Application.csproj
dotnet add src/ManagerUser.Worker/ManagerUser.Worker.csproj reference src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj
dotnet add src/ManagerUser.Worker/ManagerUser.Worker.csproj reference src/ManagerUser.Contracts/ManagerUser.Contracts.csproj
```
Tạo thư mục database và docs:
```powershell
New-Item -ItemType Directory -Force database
New-Item -ItemType Directory -Force database\tables
New-Item -ItemType Directory -Force database\procedures
New-Item -ItemType Directory -Force docs
```

# Package Cần Cài Theo Từng Project
ManagerUser.Api:
```powershell
dotnet add src/ManagerUser.Api package Swashbuckle.AspNetCore --version 6.6.2
dotnet add src/ManagerUser.Api package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add src/ManagerUser.Api package FluentValidation.AspNetCore --version 11.3.0
```
ManagerUser.Application
```powershell
dotnet add src/ManagerUser.Application package FluentValidation --version 11.9.2
dotnet add src/ManagerUser.Application package Microsoft.Extensions.Logging.Abstractions --version 8.0.0
```
ManagerUser.Infrastructure
```powershell
dotnet add src/ManagerUser.Infrastructure package Dapper --version 2.1.35
dotnet add src/ManagerUser.Infrastructure package Microsoft.Data.SqlClient --version 5.2.0
dotnet add src/ManagerUser.Infrastructure package RabbitMQ.Client --version 7.0.0
dotnet add src/ManagerUser.Infrastructure package Confluent.Kafka --version 2.14.0
dotnet add src/ManagerUser.Infrastructure package StackExchange.Redis --version 2.8.0
dotnet add src/ManagerUser.Infrastructure package Elastic.Clients.Elasticsearch --version 8.13.12
dotnet add src/ManagerUser.Infrastructure package MailKit --version 4.7.1
dotnet add src/ManagerUser.Infrastructure package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add src/ManagerUser.Infrastructure package Microsoft.Extensions.Logging.Abstractions --version 8.0.0
```
ManagerUser.Worker
```powershell
dotnet add src/ManagerUser.Worker package Microsoft.Extensions.Hosting --version 8.0.0
dotnet add src/ManagerUser.Worker package Microsoft.Extensions.Logging.Console --version 8.0.0
```
hangfire:
```powershell
dotnet add src/ManagerUser.Api package Hangfire.AspNetCore --version 1.8.14
dotnet add src/ManagerUser.Api package Hangfire.SqlServer --version 1.8.14
dotnet add src/ManagerUser.Worker package Hangfire.NetCore --version 1.8.14
dotnet add src/ManagerUser.Worker package Hangfire.SqlServer --version 1.8.14
```
Quartz.NET:
```powershell
dotnet add src/ManagerUser.Worker package Quartz.Extensions.Hosting --version 3.18.1
```
Test project:
```powershell
dotnet new xunit -n ManagerUser.Tests -o tests/ManagerUser.Tests --framework net8.0
dotnet sln add tests/ManagerUser.Tests/ManagerUser.Tests.csproj
dotnet add tests/ManagerUser.Tests/ManagerUser.Tests.csproj reference src/ManagerUser.Application/ManagerUser.Application.csproj
dotnet add tests/ManagerUser.Tests/ManagerUser.Tests.csproj reference src/ManagerUser.Domain/ManagerUser.Domain.csproj
dotnet add tests/ManagerUser.Tests/ManagerUser.Tests.csproj reference src/ManagerUser.Contracts/ManagerUser.Contracts.csproj
dotnet add tests/ManagerUser.Tests/ManagerUser.Tests.csproj reference src/ManagerUser.Infrastructure/ManagerUser.Infrastructure.csproj
dotnet add tests/ManagerUser.Tests/ManagerUser.Tests.csproj reference src/ManagerUser.Worker/ManagerUser.Worker.csproj
dotnet add tests/ManagerUser.Tests package FluentAssertions --version 6.12.0
dotnet add tests/ManagerUser.Tests package Moq --version 4.20.70
```

Create file:
```powershell
New-Item -ItemType File -Force database\procedures\Users.sql

New-Item -ItemType File -Force database\procedures\User_GetById.sql
New-Item -ItemType File -Force database\procedures\User_GetList.sql
New-Item -ItemType File -Force database\procedures\User_Create.sql
New-Item -ItemType File -Force database\procedures\User_Update.sql
New-Item -ItemType File -Force database\procedures\User_Delete.sql
New-Item -ItemType File -Force database\procedures\User_ChangeStatus.sql

```

# Background Service

SampleA_UserSnapshotWorker: giữ mẫu hiện tại, chỉnh lại config/log cho rõ.
SampleB_MultipleJobsWorker: chạy nhiều job tuần tự/song song.
SampleC_NonOverlappingWorker: chống job chạy chồng.
SampleD_BackgroundTaskQueueWorker: queue job nội bộ.
SampleE_OutboxDispatcherWorker: poll DB và xử lý message pending.
SampleF_HangfireComparison: so sánh khi nào không nên dùng BackgroundService thuần và Hangfire.
SampleG_QuartzComparison: so sánh khi nào không nên dùng BackgroundService thuần và Quartz.

# Quy tắc tạo nhánh và commit Git

## 1. Quy tắc nhánh chính

### `main`

Nhánh production. Code trên nhánh này phải là code đã release hoặc sẵn sàng release.

Không push trực tiếp vào `main`.

### `develop`

Nhánh tích hợp cho DEV.

Không push trực tiếp, chỉ được merge vào develop thông qua Pull Request..

### `release/*`

Dùng khi chuẩn bị release.

Ví dụ:

```bash
release/2026-07-05
release/v1.2.0
```

## 2. Quy tắc đặt tên nhánh

Format chung:

```bash
<type>/<ticket-id>-<mo-ta-ngan>
```

Ví dụ:

```bash
feature/us-28341-huy-ban-nhap-ke-hoach
bugfix/bug-1045-loi-bao-cao-2026
hotfix/bug-1099-loi-khong-dang-nhap
refactor/task-3001-toi-uu-store-report
docs/task-3010-cap-nhat-release-note
```

### Type được phép dùng

| Type       | Ý nghĩa                                     |
| ---------- | ------------------------------------------- |
| `feature`  | Làm chức năng mới                           |
| `bugfix`   | Sửa lỗi trong quá trình DEV/UAT             |
| `hotfix`   | Sửa lỗi production gấp                      |
| `release`  | Chuẩn bị bản release                        |
| `refactor` | Cải tổ code, không đổi nghiệp vụ            |
| `docs`     | Sửa tài liệu                                |
| `test`     | Thêm/sửa test                               |
| `chore`    | Việc phụ trợ: config, cleanup, build script |
| `spike`    | Nhánh nghiên cứu/thử nghiệm                 |

### Quy tắc bắt buộc

Tên nhánh phải viết thường, không dấu tiếng Việt, không khoảng trắng.

Dùng dấu `-` để ngăn cách từ.

Mỗi nhánh chỉ phục vụ một task, một bug hoặc một user story.

Không đặt tên chung chung như:

```bash
fixbug
update-code
test
new-branch
anh-fix
```

Nên đặt rõ nội dung như:

```bash
bugfix/bug-1045-fix-convert-date-report
feature/us-28341-add-cancel-draft-button
```

---

## 3. Quy tắc commit

Format commit:

```bash
<type>(<scope>): <noi-dung-ngan-gon>
```

Ví dụ:

```bash
feat(plan): add cancel draft button
fix(report): correct same-period data for 2026
fix(sql): handle null department id in report filter
refactor(api): split department report mapping logic
docs(release): add note for document type filter
chore(config): update connection setting for uat
```

Format đầy đủ khi cần mô tả thêm:

```bash
<type>(<scope>): <noi-dung-ngan-gon>

<mo-ta-chi-tiet-neu-can>

Refs: <ticket-id>
```

Ví dụ:

```bash
fix(report): correct same-period data for 2026

Update date filter logic to include historical data synchronized at end of day.

Refs: BUG-1045
```

---

## 4. Type commit được phép dùng

| Type       | Khi nào dùng                         |
| ---------- | ------------------------------------ |
| `feat`     | Thêm chức năng mới                   |
| `fix`      | Sửa lỗi                              |
| `refactor` | Sửa cấu trúc code, không đổi hành vi |
| `perf`     | Tối ưu hiệu năng                     |
| `docs`     | Sửa tài liệu                         |
| `style`    | Format code, indent, không đổi logic |
| `test`     | Thêm/sửa test                        |
| `build`    | Sửa build, dependency                |
| `ci`       | Sửa pipeline CI/CD                   |
| `chore`    | Việc phụ trợ, cleanup                |
| `revert`   | Revert commit trước đó               |

---

## 5. Scope commit nên dùng

Scope là khu vực bị ảnh hưởng.

Ví dụ scope phù hợp:

```bash
api
ui
sql
store
report
workflow
auth
notify
config
```

Ví dụ commit tốt:

```bash
fix(store): prevent duplicate temporary document
feat(workflow): add cancel action for saved draft
fix(sharepoint): replace style library css path
fix(api): decode language parameter correctly
refactor(sql): simplify department report query
```

---

## 6. Quy tắc viết nội dung commit

Nội dung commit phải mô tả việc đã làm, không mô tả chung chung.

Không nên:

```bash
fix bug
update code
commit code
sua loi
done
test
```

Nên:

```bash
fix(report): include child level data in level 2 summary
fix(sql): prevent duplicate SubmitSource column creation
feat(api): add language filter for department report
docs(note): update release note for document type filter
```

Một commit chỉ nên chứa một nhóm thay đổi logic.

Không gom nhiều việc không liên quan vào một commit.

Ví dụ không nên:

```bash
fix(api): update report, change css, add column, fix login
```

Nên tách ra:

```bash
fix(api): correct department report language
style(ui): update report filter spacing
fix(sql): add check before creating SubmitSource column
```

---

## 7. Quy tắc trước khi commit

Trước khi commit phải kiểm tra:

```bash
git status
git diff
```

Không commit file không liên quan.

Không commit file chứa password, token, connection string thật.

Không commit file backup, file build, file tạm nếu không cần thiết.

Ví dụ cần tránh:

```bash
*.bak
*.tmp
bin/
obj/
node_modules/
.env
appsettings.Production.json
```

Nếu có thay đổi database, commit phải nói rõ thay đổi gì.

Ví dụ:

```bash
fix(sql): add existence check before adding SubmitSource column
```

---

## 8. Quy tắc push nhánh

Push nhánh theo format:

```bash
git push origin <branch-name>
```

Ví dụ:

```bash
git push origin feature/us-28341-huy-ban-nhap-ke-hoach
```

Không push trực tiếp lên:

```bash
main
develop
release/*
```

trừ khi được phân quyền rõ ràng.

---

## 9. Quy tắc Pull Request

Tiêu đề PR nên theo format commit:

```bash
<type>(<scope>): <noi-dung-ngan-gon>
```

Ví dụ:

```bash
feat(workflow): add cancel draft action
fix(report): correct same-period report for 2026
fix(sql): prevent duplicate SubmitSource column
```

PR phải có mô tả:

```markdown
## Nội dung thay đổi
- ...

## Lý do thay đổi
- ...

## Cách test
- ...

## Ghi chú database/config
- ...
```
