# NeroiStack

![GitHub Release](https://img.shields.io/github/v/release/Neroi-Stack/NeroiStack) ![GitHub License](https://img.shields.io/github/license/Neroi-Stack/NeroiStack) ![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Neroi-Stack/NeroiStack/installer.yml)

A powerful AI agent application. 
`Still under development; pull requests are welcome.`


<img width="959" height="505" alt="image" src="https://github.com/user-attachments/assets/6bb5ef39-6938-456d-97bf-91f1a67537db" />

[here is neroi stack wiki](https://github.com/Neroi-Stack/NeroiStack/wiki)

## Features
- MCP-Plugin architecture for extensibility
- Integration with OpenAI and other AI services
- Support for Multi-Agent Orchestration

## Roadmap
- [ ] Expand plugin ecosystem for third-party integrations
- [ ] Optimize performance for large-scale AI tasks
- [ ] agentic workflows and collaboration features
- [ ] STT-TTS support

## Installation
1. Ensure you have `.NET 10 SDK` installed. You can download it from the [.NET website](https://dotnet.microsoft.com/download/dotnet/10.0).

## Getting Started
1. Clone the repository, and open it in your IDE
	```bash
	git clone https://github.com/tse-wei-chen/NeroiStack.git
	```

2. If you use Visual Studio Code, just Press `F5` hard.

3. Build the project using `.NET CLI`
	```bash
	dotnet build
	```

4. Run the application
	```bash
	dotnet run --project NeroiStack/NeroiStack.csproj
	```

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
