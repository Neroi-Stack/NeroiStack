using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Data;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using NeroiStack.Messages;

namespace NeroiStack.ViewModels;

public class ChatMessage : ObservableObject
{
	private string _text = string.Empty;
	private bool _isMe;
	private string _sender = string.Empty;

	public string Text
	{
		get => _text;
		set => SetProperty(ref _text, value);
	}

	public bool IsMe
	{
		get => _isMe;
		set => SetProperty(ref _isMe, value);
	}

	public string Sender
	{
		get => _sender;
		set => SetProperty(ref _sender, value);
	}

	public ChatMessage(string text, bool isMe, string sender = "")
	{
		Text = text;
		IsMe = isMe;
		Sender = sender;
	}
}

public partial class ChatBotViewModel : ViewModelBase
{
	private readonly IChatService _chatService;
	private readonly IServiceScopeFactory _scopeFactory;

	[ObservableProperty]
	private int _chatClassId;

	[ObservableProperty]
	private int _instanceId;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
	[NotifyCanExecuteChangedFor(nameof(StopGenerationCommand))]
	private bool _isLoading;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
	[NotifyCanExecuteChangedFor(nameof(StopGenerationCommand))]
	private bool _isGenerating;

	[ObservableProperty]
	private ObservableCollection<ChatMessage> messages = [];

	[ObservableProperty]
	private string messageInput = string.Empty;

	[ObservableProperty]
	private ObservableCollection<string> availableModels = ["➕ Add Local Model", "➕ Create Key"];

	[ObservableProperty]
	private string selectedModel = string.Empty;

	private CancellationTokenSource? _cancellationTokenSource;

	partial void OnSelectedModelChanged(string value)
	{
		if (value == "➕ Create Key")
		{
			// Open Key modal via KeyCreationVM
			KeyCreationVM.AddKeyCommand.Execute(null);
			
			SelectedModel = AvailableModels.FirstOrDefault(m => m != "➕ Create Key") ?? string.Empty;
		}
		else if (!string.IsNullOrEmpty(value) && InstanceId > 0)
		{
			// Save selected model to database
			_ = SaveSelectedModelAsync(value);
		}
	}

	private async Task SaveSelectedModelAsync(string selectedModel)
	{
		if (_chatInstanceService == null || InstanceId <= 0) return;

		try
		{
			await _chatInstanceService.UpdateSelectedModelAsync(InstanceId, selectedModel);
		}
		catch
		{
			// Log or handle error if needed
		}
	}

	[RelayCommand]
	private void AddLocalModel()
	{
		// Deprecated
	}

	// Command properties not auto-generated must be declared if used in XAML
	public IRelayCommand SendMessageCommand => SendMessageCommandInternalCommand;
	public IRelayCommand StopGenerationCommand => StopGenerationInternalCommand;

	private readonly IKeyManageService _keyManageService;
	private readonly IChatInstanceService _chatInstanceService;

	public KeyManagementViewModel KeyCreationVM { get; }

	public ChatBotViewModel(IChatService chatService, IServiceScopeFactory scopeFactory, IKeyManageService keyManageService, IChatInstanceService chatInstanceService)
	{
		_chatService = chatService;
		_scopeFactory = scopeFactory;
		_keyManageService = keyManageService;
		_chatInstanceService = chatInstanceService;
		
		KeyCreationVM = new KeyManagementViewModel(_keyManageService);

		_ = LoadModelsAsync();
	}

	// Default constructor for previewer or fallback
	public ChatBotViewModel() : this(null!, null!, null!, null!) { }

	private async Task LoadModelsAsync()
	{
		if (_keyManageService == null) return;

		var keys = await _keyManageService.GetAllKeysAsync();

		// Flatten all models from all keys
		var models = keys.SelectMany(k => k.Models)
						 .Distinct()
						 .OrderBy(m => m)
						 .ToList();

		Avalonia.Threading.Dispatcher.UIThread.Post(() =>
		{
			AvailableModels.Clear();
			foreach (var model in models)
			{
				AvailableModels.Add(model);
			}
			AvailableModels.Add("➕ Create Key");

			if (models.Any() && (string.IsNullOrEmpty(SelectedModel) || !models.Contains(SelectedModel)))
			{
				SelectedModel = models.First();
			}
		});
	}

	public void InitializeChat(int chatClassId, int instanceId, string name)
	{
		ChatClassId = chatClassId;
		InstanceId = instanceId;
		ModelName = name;

		IsLoading = true;
		Messages.Clear();

		// Load selected model for this instance
		_ = LoadSelectedModelAsync(instanceId);

		// Fire and forget initialization
		_ = InitializeSessionAsync(instanceId);
	}

	private async Task LoadSelectedModelAsync(int instanceId)
	{
		if (_chatInstanceService == null) return;

		try
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<IChatContext>();
				var instance = await context.ChatInstances.FindAsync(instanceId);
				if (instance?.SelectedModel != null && AvailableModels.Contains(instance.SelectedModel))
				{
					Avalonia.Threading.Dispatcher.UIThread.Post(() =>
					{
						SelectedModel = instance.SelectedModel;
					});
				}
			}
		}
		catch
		{
			// Log or handle error if needed
		}
	}

	private async Task InitializeSessionAsync(int instanceId)
	{
		try
		{
			// 1. Initialize AI Service Session
			await _chatService.InitializeAsync(instanceId);

			// 2. Load History from DB for UI
			using (var scope = _scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<IChatContext>();

				// Load History
				var memories = await context.ChatMemories
					.Where(m => m.ChatInstanceId == instanceId)
					.OrderByDescending(m => m.CreatedAt)
					.Take(50)
					.ToListAsync();

				// Add to Messages (On UI Thread)
				var historyList = memories.Select(m => new ChatMessage(
					   m.Content,
					   m.RoleType == RoleType.User,
					   m.RoleType == RoleType.User ? "User" : "Assistant"
				   )).Reverse().ToList();

				foreach (var msg in historyList)
				{
					Messages.Add(msg);
				}
			}
		}
		catch (Exception ex)
		{
			Messages.Add(new ChatMessage($"Error initializing chat: {ex.Message}", false, "System"));
		}
		finally
		{
			IsLoading = false;
		}
	}

	[ObservableProperty]
	private string _modelName = "Assistant";

	[RelayCommand(CanExecute = nameof(CanStopGeneration))]
	private void StopGenerationInternal()
	{
		_cancellationTokenSource?.Cancel();
	}

	private bool CanStopGeneration() => IsGenerating;
	private bool CanSendMessage() => !IsGenerating;

	[RelayCommand(CanExecute = nameof(CanSendMessage))]
	private async Task SendMessageCommandInternal()
	{
		await SendMessageAsync();
	}

	private async Task SendMessageAsync()
	{
		if (string.IsNullOrWhiteSpace(MessageInput))
			return;

		var userText = MessageInput;
		MessageInput = string.Empty;

		// User Message UI
		Messages.Add(new ChatMessage(userText, true, "User"));

		// Assistant Placeholder
		var assistantMsg = new ChatMessage("", false, "Assistant");

		IsLoading = true;
		IsGenerating = true;
		_cancellationTokenSource = new CancellationTokenSource();

		try
		{
			var supplier = SupplierEnum.OpenAI; // Default

			// Logic to find supplier for the selected model name
			// We need to look up which key/supplier has this model
			var keys = await _keyManageService.GetAllKeysAsync();
			var matchedKey = keys.FirstOrDefault(k => k.Models.Contains(SelectedModel));
			if (matchedKey != null)
			{
				supplier = matchedKey.Supplier;
			}

			var request = new InvokeChatRequest
			{
				ChatInstanceId = InstanceId,
				Text = userText,
				Supplier = supplier,
				ModelId = 0, // Not using ID anymore for string based models
				ModelName = SelectedModel, // Need to pass name
				Ct = _cancellationTokenSource.Token,
				OnChunk = async (chunk) =>
				{
					await Dispatcher.UIThread.InvokeAsync(() =>
					{
						// Add message and stop loading on first chunk
						if (!Messages.Contains(assistantMsg))
						{
							Messages.Add(assistantMsg);
							IsLoading = false;
						}
						assistantMsg.Text += chunk;
					});
				}
			};

			// We need to determine Supplier. Ideally this comes from Instance Config loaded earlier.
			// For now hardcoded or basic detection.
			// The service seems to look up config by "Group".

			var (resultText, _) = await Task.Run(() => _chatService.ChatAsync(request));

			// Ensure message is added if no chunks were received (non-streaming agents)
			if (!Messages.Contains(assistantMsg))
			{
				assistantMsg.Text = resultText;
				Messages.Add(assistantMsg);
			}
		}
		catch (OperationCanceledException)
		{
			// Handle cancellation: Show what we got so far, indicate stopped
			if (!Messages.Contains(assistantMsg) && !string.IsNullOrEmpty(assistantMsg.Text))
			{
				Messages.Add(assistantMsg);
			}
			IsLoading = false;
		}
		catch (Exception ex)
		{
			IsLoading = false;
			if (!Messages.Contains(assistantMsg)) Messages.Add(assistantMsg);
			assistantMsg.Text = $"Error: {ex.Message}";
		}
		finally
		{
			IsLoading = false;
			IsGenerating = false;
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}
	}
}
