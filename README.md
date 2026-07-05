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
