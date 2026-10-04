# 版本记录

## 2.1.7-cmano.1-preview — 2026-10-04

- 从 2018 年的 CMANO 定制程序恢复可独立构建的源码与资源。
- 将 GMap.NET.Core 和 GMap.NET.WinForms 从 1.7.5 升至 2.1.7。
- 目标框架提升到 .NET Framework 4.8，AnyCPU 在 64 位 Windows 上优先使用 64 位进程。
- 修复新旧版本 API 命名差异，移除无关 ICMP 联网探测。
- 保留旧的 CMANO PNG/PGW 导出行为与原始操作界面。
- 增加离线界面/SQLite/导出验证、锁定依赖构建、ZIP 打包与 GitHub Actions 配置。
- 记录尚待修正的多次缩图、坐标比例估算和投影误差。

验证范围不包括公共地图源的全面可用性与游戏内叠加效果；当前版本为维护预览版。
