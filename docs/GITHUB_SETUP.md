# GitHub 上传、自动构建与版本发布

现有流程：本地修改 → Git 提交并推送 → GitHub Actions 编译和测试 → 下载运行包 → 在 Releases 发布经过验证的版本。

自动构建文件为 [build.yml](../.github/workflows/build.yml)，实际构建入口为 [build.ps1](../scripts/build.ps1)。它们与源码一起保存在 Git 仓库中。如果已经通过 GitHub Desktop 发布了仓库，可以直接从第 4 节查看自动构建，并按第 6 节继续更新，无需重复初始化或添加远程仓库。

## 1. 创建空仓库

1. 登录 GitHub，打开 [创建仓库页面](https://github.com/new?name=GMap.NET.CMANO&visibility=private)。
2. 仓库名称使用 `GMap.NET.CMANO`，描述可填写 `CMANO map overlay tool maintained with GMap.NET 2.1.7`。
3. 建议先选 **Private**：这是恢复源码，原始 CMANO 定制包的许可信息还需补全，见 [来源说明](../THIRD_PARTY_NOTICES.md)。
4. 不初始化 README、`.gitignore` 或 License。本地已有这些项目文件和来源记录。
5. 创建后复制仓库的 HTTPS 地址，例如 `https://github.com/YOUR-NAME/GMap.NET.CMANO.git`。

这与你的 GitHub 账号下新建一个项目对应；此时尚未上传代码。

## 2. 准备 Git 与提交者身份

打开 PowerShell，运行 `git --version`。若找不到命令，安装 [Git for Windows](https://gitforwindows.org/)，保留 Git Credential Manager 组件，然后重新打开 PowerShell。

本机目录曾出现属主与当前 Windows 账号不一致的情况。下面的 `$projectGitArgs` 只对每次命令信任这个已知项目目录，不修改全局信任列表。

```powershell
Set-Location 'C:\Users\Levitate\Documents\GitHub\GMap.NET-CMANO'
$projectGitArgs = @('-c', 'safe.directory=C:/Users/Levitate/Documents/GitHub/GMap.NET-CMANO')

# 将两项示例替换成你的真实提交者信息后执行。
git @projectGitArgs config --local user.name '你的显示名称'
git @projectGitArgs config --local user.email '你的GitHub提交邮箱'
```

提交者姓名和邮箱用于记录代码作者，不是 GitHub 登录凭证。邮箱可使用 GitHub **Settings → Emails** 页面显示的 `noreply` 邮箱。

当前目录已经是 Git 仓库，无需再次初始化。只有在其他电脑上使用不含 `.git` 的源码副本时，才需要先执行 `git init -b main`。

## 3. 首次上传

在同一个 PowerShell 窗口中执行以下命令，先把 URL 中的 `YOUR-NAME` 替换成你的 GitHub 用户名，并确保它是刚创建的仓库地址：

```powershell
git @projectGitArgs status
git @projectGitArgs add .
git @projectGitArgs diff --cached --stat
git @projectGitArgs commit -m 'Restore CMANO tool and upgrade GMap.NET to 2.1.7'
git @projectGitArgs remote add origin https://github.com/YOUR-NAME/GMap.NET.CMANO.git
git @projectGitArgs push -u origin main
```

首次 HTTPS 推送可能通过 Git Credential Manager 打开浏览器，按提示登录自己的 GitHub 账号。如果命令报错，应先处理该错误再继续；如果已经有 `origin`，先用 `git @projectGitArgs remote -v` 核对地址，不要重复添加。

推送完成后刷新 GitHub 仓库页面，应看到 `src/`、`tests/`、`scripts/`、`docs/`、`.github/`、README 和依赖锁文件。

| 内容 | 保存位置 |
| --- | --- |
| 源码、资源、测试、工作流、文档 | Git 仓库 |
| 自动编译产生的 ZIP 与 SHA-256 | Actions 的 Artifacts |
| 供使用者下载的已发布版本 | GitHub Releases 的附件 |
| 本地可直接运行的 EXE 和 DLL | 本机 `Release/`，由 `.gitignore` 排除 |

原始 1.7.5 EXE / DLL、`.tools/`、缓存、`bin/`、`obj/`、`dist/` 和 `Release/` 已排除，不会因正常执行 `git add .` 而上传。

## 4. 查看自动构建和下载

工作流在 `push`、`pull_request` 和手动触发时运行。GitHub 的 Windows 执行环境会准备 .NET 10 SDK，并依次执行：

1. 根据锁文件还原依赖。
2. 编译 .NET Framework 4.8 程序。
3. 离线验证地图程序集版本、界面资源、SQLite 与 PNG/PGW 导出。
4. 生成包含 EXE、DLL 和原生依赖的 ZIP，以及 SHA-256 校验文件。
5. 上传名为 `GMap.NET.CMANO-2.1.7-cmano.2-preview` 的构建产物。

进入仓库 **Actions → Build and verify Windows package → 某次运行记录**。成功后在 **Artifacts** 中下载产物。下载的外层归档内包含运行包 ZIP 和校验文件；再将运行包完整解压即可使用。

如果显示红色失败状态，打开失败步骤查看日志。如果需要手动重建，工作流页面提供 **Run workflow**，使用 `main` 分支。若仓库设置禁用了 Actions，先在仓库的 Actions 设置中启用，并允许工作流中使用的 GitHub 官方 actions。

当前配置不需要你添加个人访问令牌或自定义 Secrets。工作流只读取源码并上传构建产物，没有调用发布 Release 的 API。

本地构建已验证通过；远程 Actions 结果需要在首次推送后确认。

## 5. 发布可下载版本

建议先下载一次绿色成功运行的产物，确认真实地图和游戏导入结果。然后进入仓库 **Releases → Draft a new release**：

1. 选择对应的提交创建标签，例如 `v2.1.7-cmano.2-preview`。如果选 `main`，先确认它仍指向生成该运行包的提交；之后不要移动已经发布的标签。
2. 标题填写 `GMap.NET CMANO 2.1.7 — Preview 2`，说明这次升级内容、验证范围和已知限制。
3. 将构建产物中的运行包 ZIP 及 `.sha256` 文件上传为附件。
4. 本版勾选 **Set as a pre-release**，检查附件后发布。

使用者应下载附带 EXE/DLL 的运行包。GitHub 自动生成的 `Source code (zip)` 只包含源码，需要自行编译。

Actions 产物按仓库设置的期限保留；适合长期按版本管理的运行包可以作为 Releases 附件。私有仓库中的 Release 和产物同样受仓库访问权限控制。

## 6. 以后每次更新

开始修改前，可在工作目录干净的情况下同步远程修改：

```powershell
Set-Location 'C:\Users\Levitate\Documents\GitHub\GMap.NET-CMANO'
$projectGitArgs = @('-c', 'safe.directory=C:/Users/Levitate/Documents/GitHub/GMap.NET-CMANO')
git @projectGitArgs pull --ff-only
```

修改完成后，在同一窗口执行：

```powershell
./scripts/build.ps1
git @projectGitArgs status
git @projectGitArgs add .
git @projectGitArgs diff --cached --stat
git @projectGitArgs commit -m 'Describe this update'
git @projectGitArgs push
```

本地构建失败时先修复再继续。推送会再次触发 Actions；等该次运行成功并验证地图导出效果后，再发布一个新版本。

当前应用维护版本和 GMap.NET 库版本是分别管理的。例如后续可发布 `2.1.7-cmano.3-preview`，同时继续使用 GMap.NET 2.1.7。发新版本时同步修改 `Properties/AssemblyInfo.cs` 中的应用版本、构建脚本的 `$packageName`、工作流的产物名称及 `CHANGELOG.md`；Release 标签与运行包对应。

## 官方参考

- [上传本地仓库](https://docs.github.com/en/migrations/importing-source-code/using-the-command-line-to-import-source-code/adding-locally-hosted-code-to-github)
- [Windows 上的 GitHub 身份验证](https://docs.github.com/en/get-started/git-basics/caching-your-github-credentials-in-git?platform=windows)
- [配置提交邮箱](https://docs.github.com/en/account-and-profile/how-tos/email-preferences/setting-your-commit-email-address?platform=windows)
- [下载 Actions 产物](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/download-workflow-artifacts?tool=webui)
- [创建和管理 Release](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository)
