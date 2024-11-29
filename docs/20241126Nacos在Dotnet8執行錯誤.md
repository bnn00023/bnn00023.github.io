---
layout: default
title: "Nacos在Dotnet8執行錯誤"
permalink: /Nacos在Dotnet8執行錯誤/
---

## 問題
Nacos在原本.Net 7的專案可以執行正常，但是.Net 8執行卻會跑出Exception。

## 錯誤訊息
``` log
2024-11-26 15:56:59 info: Nacos.Microsoft.Extensions.Configuration.NacosV2ConfigurationProvider[0]
2024-11-26 15:56:59       Remove All Listeners
2024-11-26 15:56:59 Unhandled exception. System.IndexOutOfRangeException: Index was outside the bounds of the array.
2024-11-26 15:56:59    at Nacos.AspNetCore.UriTool.GetUri(IFeatureCollection features, String ip, Int32 port, String preferredNetworks)
2024-11-26 15:56:59    at Nacos.AspNetCore.V2.RegSvcBgTask.StartAsync(CancellationToken cancellationToken)
2024-11-26 15:56:59    at Microsoft.Extensions.Hosting.Internal.Host.<StartAsync>b__15_1(IHostedService service, CancellationToken token)
2024-11-26 15:56:59    at Microsoft.Extensions.Hosting.Internal.Host.ForeachService[T](IEnumerable`1 services, CancellationToken token, Boolean concurrent, Boolean abortOnFirstException, List`1 exceptions, Func`3 operation)
2024-11-26 15:56:59    at Microsoft.Extensions.Hosting.Internal.Host.StartAsync(CancellationToken cancellationToken)
2024-11-26 15:56:59    at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)
2024-11-26 15:56:59    at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)
2024-11-26 15:56:59    at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.Run(IHost host)
```

## 行動
上GitHub專案nacos-sdk-csharp將尋找問題點
``` C#
// 4. --urls
var cmdArgs = Environment.GetCommandLineArgs();
if (cmdArgs != null && cmdArgs.Any())
{
    var cmd = cmdArgs.FirstOrDefault(x => x.StartsWith("--urls", StringComparison.OrdinalIgnoreCase));

    if (!string.IsNullOrWhiteSpace(cmd))
    {
        address = cmd.Split('=')[1];

        var url = ReplaceAddress(address, preferredNetworks);

        var uris = url.Split(splitChars).Select(x => new Uri(x));

        foreach (var item in uris)
        {
            if (!IPAddress.TryParse(item.Host, out _))
            {
                throw new Nacos.V2.Exceptions.NacosException("Invalid ip address from --urls");
            }
        }

        return uris;
    }
}
```

在以上程式中觀察到有使用到Argument切割--urls參數，但是在Docker file中的寫法是所以在分割--urls得不到address，引發IndexOutOfRangeException
``` Dockerfile
ENTRYPOINT ["dotnet", "NacosApi7.dll" , "--urls", "http://+:12345"]
```

## 解決方法
發現"--urls+http://+:12345"與"--urls", "http://+:12345"這兩種寫法是產生的結果是一樣。
``` Dockerfile
ENTRYPOINT ["dotnet", "NacosApi7.dll" , "--urls+http://+:12345"]
```

## 疑問
但是為什麼.Net7可以但是.Net8不行，這個問題沒有解決

## 分析
有發現到錯誤的程式是為了拿到Host的Address與Port，從Argument的步驟有註解是第四步，往前找看有哪些步驟。

1. 從Config中取得，但是原本的專案就沒有設定，所以跳過
``` C#
 // 1. config
 if (!string.IsNullOrWhiteSpace(ip))
 {
     // it seems that nacos don't return the scheme
     // so here use http only.
     return new List<Uri> { new Uri($"http://{ip}:{appPort}") };
 }

 // 1.1. Ip is null && Port has value
 if (string.IsNullOrWhiteSpace(ip) && appPort != 80)
 {
     return new List<Uri> { new Uri($"http://{GetCurrentIp(preferredNetworks)}:{appPort}") };
 }
```

2. 從IServerAddressesFeature取得設定， 痾... 沒有碰過這個Interface，假設沒用到，跳過。
``` C#
// 2. IServerAddressesFeature
if (features != null)
{
    var addresses = features.Get<IServerAddressesFeature>();
    var addressCollection = addresses?.Addresses;

    if (addressCollection != null && addressCollection.Any())
    {
        var uris = new List<Uri>();
        foreach (var item in addressCollection)
        {
            var url = ReplaceAddress(item, preferredNetworks);
            uris.Add(new Uri(url));
        }

        return uris;
    }
}
```

3. 從環境變數ASPNETCORE_URLS取得，為了固定啟動的Port，在Dockerfile中有設定urls參數，應該有關係。
``` C#
// 3. ASPNETCORE_URLS
address = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrWhiteSpace(address))
{
    var url = ReplaceAddress(address, preferredNetworks);

    var uris = url.Split(splitChars).Select(x => new Uri(x));

    foreach (var item in uris)
    {
        if (!IPAddress.TryParse(item.Host, out _))
        {
            throw new Nacos.V2.Exceptions.NacosException("Invalid ip address from ASPNETCORE_URLS");
        }
    }

    return uris;
}
```

## 尋找文件
發現了這個文件https://learn.microsoft.com/zh-tw/dotnet/core/compatibility/containers/8.0/aspnet-port，原來是.Net 8有了`ASPNETCORE_HTTP_PORTS` 環境變數設定啟用的Port，故不再設定`ASPNETCORE_URLS` ，導致引發的錯誤。

.Net7
``` log
info: NacosApi7.EnvService[0]
      ASPNETCORE_URLS: http://+:80
info: NacosApi7.EnvService[0]
      ASPNETCORE_HTTP_PORTS: (null)
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:12345
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
```

.Net8
``` log
info: NacosApi8.EnvService[0]
      ASPNETCORE_URLS: (null)
info: NacosApi8.EnvService[0]
      ASPNETCORE_HTTP_PORTS: 8080
warn: Microsoft.AspNetCore.Hosting.Diagnostics[15]
      Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://+:12345'.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:12345
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
```