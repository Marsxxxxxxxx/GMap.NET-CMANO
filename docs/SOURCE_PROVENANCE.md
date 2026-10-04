# 源码恢复与版本来源

记录日期：2026-10-04。

最初目录只有以下运行文件及一个旧文件目录，没有 `.sln`、`.csproj` 或 C# 源码。原配置声明 `.NETFramework,Version=v4.0`，主程序和地图库文件版本均为 1.7.5。

| 原始文件 | SHA-256 |
| --- | --- |
| GMap.NET_CMANO.exe | `84a7194686f044139435d6b2e67f4424b664707c003be00f1e892d7918ffcddd` |
| GMap.NET_CMANO.exe.config | `68cbb1295389a1bd6b830debfd0a8bb0a88bee2522304f5894c710912021194c` |
| GMap.NET.Core.dll | `b0337b27ec0c3b35d26d33f644ea3c328238b1d6df040c6fe39319e7f1e5c7b4` |
| GMap.NET.WindowsForms.dll | `18ff5a07a3cc82711b4f80a089ca8539380c2b6f248e2f2cf0e546a4a691561f` |
| GMap.NET.WindowsForms.xml | `10bb7f79541bc4d17e9c6ab824a639813f2a15a2d9e2823ec5167482a0ef2a5d` |

恢复工具为 NuGet 官方源发布的 ILSpy CLI 11.1.0.9782。使用项目导出方式恢复 C# 和资源，然后修改目标框架、包引用以及有变化的 API。

当前源码中的类型名、局部变量命名、部分结构和设计器代码反映了反编译结果；原始注释、提交记录和未编入 EXE 的文件无法恢复。主程序代码以及 BSE 控件不是从当前 GMap.NET 上游仓库直接复制的。

构建使用的 GMap.NET 库来自 NuGet 的 2.1.7 包，而不是本地原始 1.7.5 DLL。运行包中两个地图程序集的实际版本均已验证为 2.1.7.0。应用自己的维护预览版本为 2.1.7-cmano.1-preview，文件版本 2.1.7.1。

本次主要修改位于：

- `src/GMap.NET.CMANO/GMap.NET_CMANO.csproj`：net48、2.1.7 包引用和资源/绑定设置。
- `src/GMap.NET.CMANO/Demo.WindowsForms/MainForm.cs`：API 适配、启动联网判断、可选旧 IP 查询密钥、版本标题及离线验证入口。
- `src/GMap.NET.CMANO/Demo.WindowsForms/StaticImage.cs`：将旧单例引用替换为 `GMaps.Instance`，导出算法暂时保留。
- `src/GMap.NET.CMANO/Properties/AssemblyInfo.cs`：维护预览版本和测试程序集可见性。
- `src/GMap.NET.CMANO/app.config`：.NET Framework 4.8 运行目标。

当前结果是基础升级验证，不包含真实地图服务的全面测试，不包含游戏内验证，也不代表后续画质与定位修复已完成。
