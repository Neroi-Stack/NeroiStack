# NeroiStack

A powerful AI agent application with multi-platform support.
`Still under development; pull requests are welcome.`

<img width="959" height="505" alt="image" src="https://github.com/user-attachments/assets/6bb5ef39-6938-456d-97bf-91f1a67537db" />

[here is neroi stack wiki](https://github.com/Neroi-Stack/NeroiStack/wiki)

## Features
- MCP-Plugin architecture for extensibility
- Integration with OpenAI and other AI services
- Support for Multi-Agent Orchestration
- **Multi-platform support**: Desktop (Windows, macOS, Linux), Web (WASM), Android, and iOS

## Platform Support

NeroiStack now supports multiple platforms:
- ✅ **Desktop** - Windows, macOS, Linux (Fully functional)
- 🔧 **Web** - WebAssembly (Framework ready, needs single-view layout implementation)
- 🔧 **Android** - Mobile devices (Framework ready, needs single-view layout implementation)
- 🔧 **iOS** - iPhone and iPad (Framework ready, needs single-view layout implementation)

The Desktop platform is fully functional and can be used in production. The Browser, Android, and iOS platforms have the framework in place but require additional work to implement single-view layouts and platform-specific services.

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture information.

## Roadmap
- [ ] Expand plugin ecosystem for third-party integrations
- [ ] Optimize performance for large-scale AI tasks
- [ ] agentic workflows and collaboration features
- [ ] STT-TTS support

## Installation
1. Ensure you have `.NET 10 SDK` installed. You can download it from the [.NET website](https://dotnet.microsoft.com/download/dotnet/10.0).

## Getting Started

### Desktop Application

1. Clone the repository, and open it in your IDE
	```bash
	git clone https://github.com/tse-wei-chen/NeroiStack.git
	```

2. If you use Visual Studio Code, just Press `F5` hard.

3. Build the project using `.NET CLI`
	```bash
	dotnet build NeroiStack.Desktop
	```

4. Run the application
	```bash
	dotnet run --project NeroiStack.Desktop/NeroiStack.Desktop.csproj
	```

### Web Application (WASM)

1. Install the WASM workload:
	```bash
	dotnet workload install wasm-tools
	```

2. Build and run:
	```bash
	dotnet build NeroiStack.Browser
	dotnet run --project NeroiStack.Browser
	```

### Mobile Applications

For Android:
```bash
dotnet workload install android
dotnet build NeroiStack.Android -f net10.0-android
```

For iOS (requires macOS):
```bash
dotnet workload install ios
dotnet build NeroiStack.iOS -f net10.0-ios
```

See [MIGRATION.md](MIGRATION.md) for detailed migration information from the legacy single-platform version.

## Multi-Agent Orchestration Strategies
NeroiStack supports various multi-agent orchestration strategies to enable complex AI workflows. Below are some of the key strategies illustrated with diagrams:
1. Concurrent
	```mermaid
	graph TD
		Input[Input] --> Agent1[Agent 1]
		Input[Input] --> Agent2[Agent 2]
		Input[Input] --> Agent3[Agent 3]
		Agent1[Agent 1] --> C[Collector]
		Agent2[Agent 2] --> C[Collector]
		Agent3[Agent 3] --> C[Collector]
		C[Collector] --> Output[Output]
	```
2. Sequential
	```mermaid
	graph TD
		Input[Input] --> Agent1[Agent 1]
		Agent1[Agent 1] --> Agent2[Agent 2]
		Agent2[Agent 2] --> Agent3[Agent 3]
		Agent3[Agent 3] --> Output[Output]
	```
3. Group Chat
	```mermaid
	graph TD
		Input[Input] --> GroupChatManager[GroupChatManager]
		GroupChatManager[GroupChatManager] <--> Agent1[Agent 1]
		GroupChatManager[GroupChatManager] <--> Agent2[Agent 2]
		GroupChatManager[GroupChatManager] <--> Agent3[Agent 3]
		GroupChatManager[GroupChatManager] --> Output[Output]
	```	
4. Handoff
	```mermaid
	graph TD
		Input[Input] --> A1[Entry Agent]
		A1 -- Transfer --> A2[Specialized Agent A]
		A1 -- Transfer --> A3[Specialized Agent B]
		A2 -- Complete --> Output[Output]
		A3 -- Complete --> Output[Output]
	```
5. Magentic
	```mermaid
	graph TD
		Input[Input] --> M[Magentic Manager]
		M -- Select & Call --> A1[Agent 1]
		A1 -- Observation --> M
		M -- Select & Call --> A2[Agent 2]
		A2 -- Observation --> M
		M -- "Satisfied (Done)" --> Output[Output]
	```
