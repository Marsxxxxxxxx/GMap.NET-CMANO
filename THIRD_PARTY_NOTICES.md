# 第三方与来源说明

本项目的维护起点是用户提供的 `GMap.NET_CMANO.exe` 1.7.5（文件时间 2018-03-18）及其 1.7.5 依赖。源码由 ILSpy 11.1.0.9782 恢复，不是原始作者提供的完整源码仓库。恢复后的原有版权属性仍然保留。

## GMap.NET

- 当前依赖：`GMap.NET.Core` 2.1.7、`GMap.NET.WinForms` 2.1.7。
- 上游：https://github.com/judero01col/GMap.NET
- 包声明的版权：Copyright (c) MainSoft Technology 2016-2022。
- 上游许可原文保存在 `licenses/GMap.NET.txt`，复制自上游仓库 LICENSE。
- 恢复的原程序还保留 `FLAT EARTH` / radioman 的版权属性。

## 恢复的 UI 与网页演示资源

- `BSE.Windows.Forms` 控件随原 EXE 一并恢复。该组件历史来源的版权标记为 BSE / Uwe Eichkorn；本地原包未附其独立许可文件，未给这些控件另行重新授权。
- Leaflet 0.5.1、Leaflet locate control、Proj4Leaflet 等网页演示资源来自原 EXE 的嵌入资源，保留源码及原有注释。
- `Demo.WindowsForms.leafletjs.dist.proj4js-compressed.js` 的原有头部明确标注 LGPL，并列出 Mike Adair、Richard Greenwood、Didier Richard 和 Stephen Irons。该文件的许可不应被 GMap.NET 的许可覆盖。
- 这些网页资源用于旧的 TileHost 演示，与桌面地图导出使用的重投影逻辑不同；不能因为包含 Proj4js 就认为桌面导出已完成重投影。

## NuGet 传递依赖

包括 System.Data.SQLite、Newtonsoft.Json、EntityFramework、System.Data.SqlClient、System.Resources.Extensions 及部分 .NET 支持库，精确版本与内容哈希见两个 `packages.lock.json`。各包遵循其自己的许可；依赖由 NuGet 还原，未将第三方 DLL 当作本项目源码提交。

## 发布范围

现有 CMANO 定制包没有单独的 README / LICENSE，也没有能证明全部定制修改授权范围的原作者源码。这份说明记录已知来源，不代表将整份恢复源码重新授予 MIT 或其他许可证。当前适合作为个人私有维护仓库；公开发布前应补全定制修改及历史组件的来源和授权信息，或者逐步替换为来源清楚的上游源码。

地图服务的访问、缓存和影像分发由对应图源的条款决定，软件库的许可不自动授予地图影像的分发权。
