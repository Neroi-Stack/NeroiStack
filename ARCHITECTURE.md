# NeroiStack 架构说明

## 项目结构

NeroiStack 现在采用多平台架构，支持 Desktop、Web (WebAssembly)、Android 和 iOS 平台。

### 核心项目

- **NeroiStack.Core**: 共享 UI 核心库，包含所有 Views、ViewModels 和平台无关代码
  - ViewModels: 所有视图模型
  - Views: 所有 AXAML 视图文件
  - Component: 可重用的 UI 组件（如模态框）
  - Messages: 消息传递类
  - Converters: 值转换器
  - Assets: 共享资源文件

- **NeroiStack.Agent**: AI Agent 业务逻辑核心
  - Agent 管理
  - Semantic Kernel 集成
  - 策略模式实现

- **NeroiStack.Common**: 共享工具和通用代码
  - 加密服务
  - MIME 类型处理
  - 其他通用功能

### 平台项目

- **NeroiStack.Desktop**: Windows/macOS/Linux 桌面应用
  - 使用 `IClassicDesktopStyleApplicationLifetime`
  - 完整的依赖注入配置
  - SQLite 数据库支持
  - **状态**: ✅ 完全可用

- **NeroiStack.Browser**: Web 应用 (WebAssembly)
  - 使用 `ISingleViewApplicationLifetime`
  - 需要 `wasm-tools` workload
  - **状态**: 🔧 框架就绪，需要完成单视图布局和浏览器特定服务

- **NeroiStack.Android**: Android 应用
  - 最低 API Level 21
  - 使用 Avalonia.Android
  - **状态**: 🔧 框架就绪，需要完成单视图布局和移动特定服务

- **NeroiStack.iOS**: iOS 应用
  - 最低 iOS 11.0
  - 使用 Avalonia.iOS
  - **状态**: 🔧 框架就绪，需要完成单视图布局和移动特定服务

### 测试项目

- **NeroiStack.Agent.UnitTest**: Agent 模块的单元测试

## 构建说明

### 前置要求

- .NET 10.0 SDK 或更高版本
- 对于 Browser 平台，需要安装 WASM 工具链

### Desktop 平台

构建和运行桌面应用：

```bash
dotnet build NeroiStack.Desktop
dotnet run --project NeroiStack.Desktop
```

### Browser 平台

Browser 平台需要先安装 WASM 工具链：

```bash
dotnet workload install wasm-tools
```

然后构建和运行：

```bash
dotnet build NeroiStack.Browser
dotnet run --project NeroiStack.Browser
```

### Android 平台

构建 Android 应用：

```bash
dotnet build NeroiStack.Android -f net10.0-android
```

### iOS 平台

构建 iOS 应用（需要 macOS）：

```bash
dotnet build NeroiStack.iOS -f net10.0-ios
```

## 架构特点

### 代码共享

- UI 代码 100% 共享（Views 和 ViewModels）
- 业务逻辑通过 NeroiStack.Agent 和 NeroiStack.Common 共享
- 平台特定代码仅限于启动和配置

### 依赖注入

所有平台都使用 Microsoft.Extensions.DependencyInjection 进行依赖注入：

- Services: 业务服务（ChatService, AgentManageService 等）
- ViewModels: 所有视图模型
- DbContext: Entity Framework Core 数据库上下文

### 数据存储

- Desktop: SQLite 数据库（neroi_chats.db）
- Browser: 可能需要使用不同的存储方案（IndexedDB 等）
- Mobile: 平台特定的存储路径

## 开发指南

### 添加新功能

1. 在 NeroiStack.Core 中添加 ViewModel 和 View
2. 确保使用平台无关的 API
3. 如需平台特定功能，在对应平台项目中实现

### 命名空间约定

- Core 库保持原有的 `NeroiStack` 命名空间
- 平台项目使用各自的命名空间：
  - `NeroiStack.Desktop`
  - `NeroiStack.Browser`
  - `NeroiStack.Android`
  - `NeroiStack.iOS`

### 迁移说明

原有的 `NeroiStack` 项目已被保留为 Legacy 项目，所有功能已迁移到新的多平台架构。建议在验证新架构稳定后移除旧项目。

## 许可证

遵循项目根目录的 LICENSE 文件。
