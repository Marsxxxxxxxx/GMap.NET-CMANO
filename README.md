# GMap.NET CMANO — 2.1.7 维护预览版

为 CMANO 制作地图叠加层的 Windows 工具。这个项目从现有的 2018 年 GMap.NET CMANO 1.7.5 可执行文件恢复源码，将地图控件升级到 NuGet 的 **GMap.NET.Core 2.1.7 / GMap.NET.WinForms 2.1.7**，为后续改进清晰度和地理定位建立可编译的维护基线。

这是非官方维护预览版。恢复源码不是原作者的完整开发仓库；保留已有地图与 CMANO 导出流程；Preview 4 重做主界面与导出窗口，详细来源见 [来源记录](docs/SOURCE_PROVENANCE.md)。

## 运行

1. 解压整个 `GMap.NET.CMANO-2.1.7-cmano.4-preview.zip`。
2. 保留同目录下的所有 DLL，以及 `x86` / `x64` 等子目录。
3. 启动 `GMap.NET_CMANO.exe`。需要 Windows 和 **.NET Framework 4.8**；运行程序不需要 .NET 10 SDK。

当前工作目录根部的原始 1.7.5 程序保持原样。不要把新版 DLL 单独覆盖到旧 EXE 旁边；升级版应从自己的解压目录运行。

## 新界面（Preview 4）

主界面采用顶部操作栏、左侧设置页面和大地图工作区。常用流程按「选择地图 → 定位 → 确定范围 → 导出」组织，支持十进制度边界、平移/框选切换、当前视野选区和原始像素尺寸预览。标记、路线、缓存与旧工具保留在独立页面中。

使用方法见 [新版界面说明](docs/UI_GUIDE.md)。下图为 150% 缩放下的真实窗口，地图使用离线测试网格。

![Preview 4 工作台](docs/images/workspace-preview4.png)

## 缓存位置

Preview 3 默认在 EXE 所在目录下创建 `cache/`，地图瓦片数据库、地理编码/路线缓存和 IP 查询缓存都使用这个位置。例如从 `Release/GMap.NET_CMANO.exe` 启动时，缓存位于 `Release/cache/`。从快捷方式或其他工作目录启动也使用同一个 EXE 相对路径。

程序目录需要允许当前用户写入。推荐把完整运行包放在自己的工具目录。需要保留原有缓存时，关闭程序，将 `%LOCALAPPDATA%/GMap.NET/` 中的内容复制到新 `cache/` 中，保留 `TileDBv5/` 等子目录结构。程序首次使用新位置时不会自动复制旧缓存。

`cache/` 已加入 Git 忽略规则，构建打包也会排除已生成的缓存内容。

## 4K 屏幕与高 DPI

Preview 2 在 Windows 10 1703 及更新版本（包括 Windows 11）上启用 `PerMonitorV2`，主窗口和导出窗口按屏幕 DPI 缩放，自绘标题栏与图标同步缩放。可以保留 Windows 的 150% 等缩放比例。

从 `Release/GMap.NET_CMANO.exe` 或完整解压后的运行包启动，并保留同目录的 `GMap.NET_CMANO.exe.config`。若曾在 EXE 的“属性 → 兼容性 → 更改高 DPI 设置”中启用“替代高 DPI 缩放行为”，先取消该覆盖并重新启动程序，以使用程序自己的 DPI 适配。

自动验证会检查实际窗口 DPI 模式、主界面与导出窗口尺寸，以及自绘标题栏在 100% → 150% → 200% → 100% 之间缩放时是否发生重复缩放或舍入漂移。本次本机实际渲染检查使用 144 DPI（150%）；200% 为控件缩放测试，混合 DPI 多显示器拖动仍需实际确认。旧的小尺寸位图资源没有重新制作。

界面清晰度与导出地图的像素分辨率分别控制。这次更新不改变导出缩图或 PGW 坐标计算规则。

Preview 4 的新控件采用原生 DPI 布局和矢量绘制按钮，在本机 150% 缩放下验证主界面、紧凑窗口及导出窗口。

配置依据：[Microsoft 的 Windows Forms 高 DPI 支持说明](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8)。

## 这次升级包含什么

- 将两个 GMap.NET 包精确固定为 2.1.7，并提交 NuGet 锁文件。
- 将程序目标从 .NET Framework 4.0 提升到 4.8，生成所需的程序集绑定重定向。
- 适配地理编码状态、地图缓存属性和单例访问的 API 变化。
- 保留区域选择、地图源选择、PNG + `.pgw` 文件命名与默认启用 Worldfile，重新设计操作界面。
- 移除使用无关 `pingtest.com` 的联网判断，避免 ICMP 被禁用时强制进入离线缓存模式。
- 移除旧 IP 查询演示功能中硬编码的第三方密钥；该可选演示只在设置 `GMAP_IPINFODB_API_KEY` 时尝试工作，其旧服务本身尚未验证。
- 提供离线验证、独立运行包和 GitHub Actions 构建配置。
- Preview 2 增加高 DPI 配置、窗口与自绘面板缩放，并验证 150% 下的实际界面布局。
- Preview 3 将默认缓存移至 EXE 同目录的 cache/，增加实际 SQLite 缓存读写验证。
- Preview 4 重做界面与导出窗口，增加十进制度边界输入、直接框选、像素尺寸预览、输入校验和后台导出状态。

## 当前验证范围

离线验证会检查新界面的导航、坐标与鼠标选区、窗口尺寸变化和导出输入校验，并在真实 WinForms 消息循环中点击导出按钮，检查后台进度与完成状态。验证也会真正加载两个 2.1.7.0 程序集，打开原生 SQLite，构造主界面及资源，并把本地合成瓦片交给现有导出器，检查 513 × 387 PNG 和对应六行 PGW。不会请求公共地图服务。

这能确认基础升级与导出流程可工作，**不能证明所有在线地图源仍可用，也不能代替 CMANO / CMO 中的实测对齐**。构建保留了恢复代码中的 5 个旧警告（未赋值的字段和过时的异常类型）；没有通过关闭警告来隐藏它们。

## 清晰度与坐标：尚未改动的旧行为

- 导出器仍默认使用 **10 MB 文件大小限制**。超过限制时，会反复将宽、高缩小到 80%；每次只保留约 64% 的像素数量。
- 同时保留带时间戳的原始 PNG 和 PGW。需要原始细节时，可先使用这组文件；不要将原始 PNG 与缩小版的 PGW 混用。
- 缩小版 PGW 仍沿用旧的 `1.25^缩放次数` 比例计算，并未改为读取最终实际像素尺寸。
- 当前导出器仍将图源投影下的拼图直接配上经纬度线性比例。大范围、高纬度地图的投影误差尚未修正。
- Worldfile 的像素中心约定、小数点文化设置以及不同图源的坐标基准仍需要针对性修正和游戏内验证。

下一阶段按 [精度与画质改进计划](docs/ROADMAP.md) 实施。升级地图控件本身不会自动提升源影像分辨率或修正这些坐标问题。

## 本地构建

需要 Windows、.NET Framework 4.8 和 .NET 10 SDK（C# 14）。安装 SDK 后，在仓库根目录运行：

```powershell
./scripts/build.ps1
```

脚本优先使用 `.tools/dotnet/dotnet.exe`，没有项目内 SDK 时使用系统 `dotnet`。依赖缓存、验证输出和 SDK 都不纳入 Git。

两个项目都显式引用 `Microsoft.NETFramework.ReferenceAssemblies 1.0.3`，由 NuGet 提供 .NET Framework 4.8 引用程序集。这样，无论构建机器是否安装开发包，锁定还原都使用相同的依赖列表；运行程序仍需要 .NET Framework 4.8。

构建脚本执行锁定依赖还原、Release 编译、离线验证和打包。最终文件在 `dist/` 中，并附 SHA-256 校验文件。源码可以在不保留原始 EXE / DLL 的新电脑上构建；不需要再次反编译。

构建脚本也会在项目根目录生成 `Release/` 子目录，内含可直接运行的 `GMap.NET_CMANO.exe`、相关 DLL 和 SQLite 原生依赖。双击其中的 EXE 即可运行；复制到其他位置时应复制整个 `Release/` 目录。

## 放到你自己的 GitHub 仓库

参见 [GitHub 建库与首次上传](docs/GITHUB_SETUP.md)。已包含自动构建工作流；首次推送后会生成可下载的 Actions 产物。工作流不会自动发布 Release，也没有加入程序自动更新功能。

## 来源与许可

保留原作者版权信息。上游 GMap.NET 的许可文本在 `licenses/GMap.NET.txt`，其他组件和恢复源码的许可状态见 [第三方与来源说明](THIRD_PARTY_NOTICES.md)。本目录中原有的 CMANO 定制包没有附带单独许可证，因此没有把整份恢复源码擅自重新声明为 MIT 授权。
