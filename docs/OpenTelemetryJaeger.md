# C# OpenTelemetry + Jaeger 範例

這個範例示範如何使用 .NET 10 控制台應用程式透過 OpenTelemetry 收集下列操作的追蹤資訊，並送到 Jaeger 檢視：

- 透過 `HttpClient` 呼叫 `https://www.google.com/`。
- 使用 `StackExchange.Redis` 存取 Redis。
- 使用 `Npgsql` 存取 PostgreSQL。

## 專案位置

應用程式原始碼位於 [`src/OpenTelemetryDemo`](../src/OpenTelemetryDemo)。

## 快速開始

1. 啟動 Redis、PostgreSQL 與 Jaeger：

   ```bash
   docker compose up -d
   ```

2. （可選）設定連線字串與 Jaeger 代理主機位置。以下為預設值，未設定時會直接使用：

   ```bash
   export Redis__ConnectionString="localhost:6379"
   export Postgres__ConnectionString="Host=localhost;Username=otel;Password=otel;Database=otel"
   export Jaeger__Host="localhost"
   export Jaeger__Port=6831
   ```

3. 執行範例應用程式：

   ```bash
   dotnet run --project src/OpenTelemetryDemo/OpenTelemetryDemo.csproj
   ```

4. 開啟 Jaeger 介面 `http://localhost:16686`，選取服務名稱 `OpenTelemetryDemo` 後即可看到 `HttpClient`、Redis 與 PostgreSQL 的追蹤資料。

## 範例程式重點

- `Program.cs` 透過 `AddOpenTelemetry()` 註冊 `HttpClient`、Redis 與 Npgsql 的自動化儀表收集，並設定 Jaeger 匯出器。
- `TelemetryWorker` 在應用程式啟動時依序對 Google、PostgreSQL 與 Redis 發出請求，讓上述儀表產生範例追蹤資料。

## 清除資源

完成後可停止並移除容器：

```bash
docker compose down
```
