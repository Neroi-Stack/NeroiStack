using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;

namespace NeroiStack.ViewModels;

public partial class ModelItemViewModel : ObservableObject
{
	[ObservableProperty]
	private string _name = string.Empty;

	public ModelItemViewModel(string name)
	{
		Name = name;
	}
}

public partial class KeyItemViewModel : ObservableObject
{
	public Guid Id { get; }

	[ObservableProperty]
	private SupplierEnum _supplier;

	[ObservableProperty]
	private string _keyDisplay = string.Empty;

	[ObservableProperty]
	private string _endpointDisplay = string.Empty;

	[ObservableProperty]
	private int _modelCount;

	public KeyItemViewModel(KeyVM vm)
	{
		Id = vm.Id;
		Supplier = vm.Supplier;
		KeyDisplay = string.IsNullOrEmpty(vm.Key) ? "(No Key)" : (vm.Key.Length > 4 ? "..." + vm.Key[^4..] : "***");
		EndpointDisplay = string.IsNullOrEmpty(vm.Endpoint) ? "(Default)" : vm.Endpoint;
		ModelCount = vm.Models.Count;
	}
}

public sealed partial class KeyManagementViewModel : ViewModelBase
{
	private readonly IKeyManageService _keyManageService;

	public ObservableCollection<KeyItemViewModel> Keys { get; } = new();

	[ObservableProperty]
	private bool _isModalOpen;

	[ObservableProperty]
	private bool _isEditorVisible;

	[ObservableProperty]
	private bool _isDeleteConfirmVisible;

	[ObservableProperty]
	private string _modalTitle = string.Empty;

	[ObservableProperty]
	private KeyVM? _currentKey;

	// For editing in modal
	[ObservableProperty]
	private string _currentKeyInput = string.Empty;
	[ObservableProperty]
	private string _currentEndpointInput = string.Empty;
	[ObservableProperty]
	private SupplierEnum _currentSupplierInput;
	public ObservableCollection<ModelItemViewModel> CurrentModels { get; } = new();

	public System.Collections.Generic.List<SupplierEnum> AvailableSuppliers { get; } = System.Enum.GetValues(typeof(SupplierEnum)).Cast<SupplierEnum>().ToList();


	public KeyManagementViewModel(IKeyManageService keyManageService)
	{
		_keyManageService = keyManageService;
		_ = LoadAllConfigsAsync();
	}

	public async Task LoadAllConfigsAsync()
	{
		if (_keyManageService == null) return;
		var keyVms = await _keyManageService.GetAllKeysAsync();
		Keys.Clear();
		foreach (var key in keyVms)
		{
			Keys.Add(new KeyItemViewModel(key));
		}
	}

	[RelayCommand]
	public void AddKey()
	{
		ModalTitle = "Add Key";
		CurrentKey = new KeyVM { Supplier = SupplierEnum.OpenAI };
		InitializeEditor(CurrentKey);
		IsEditorVisible = true;
		IsDeleteConfirmVisible = false;
		IsModalOpen = true;
	}

	[RelayCommand]
	public void EditKey(KeyItemViewModel item)
	{
		// Need to fetch full key details? We only have summary in item?
		// Better to refetch or store KeyVM in item. KeyVM is lightweight.
		// Let's refetch to be safe or just use ID.
		_ = EditKeyAsync(item.Id);
	}

	private async Task EditKeyAsync(Guid id)
	{
		var key = await _keyManageService.GetAllKeysAsync(); // Optimization: GetById
		var target = key.FirstOrDefault(k => k.Id == id);
		if (target != null)
		{
			ModalTitle = "Edit Key";
			CurrentKey = target;
			InitializeEditor(target);
			IsEditorVisible = true;
			IsDeleteConfirmVisible = false;
			IsModalOpen = true;
		}
	}

	private void InitializeEditor(KeyVM key)
	{
		CurrentSupplierInput = key.Supplier;
		CurrentKeyInput = key.Key ?? "";
		CurrentEndpointInput = key.Endpoint ?? "";
		CurrentModels.Clear();
		foreach (var m in key.Models)
		{
			CurrentModels.Add(new ModelItemViewModel(m));
		}
		if (CurrentModels.Count == 0)
		{
			CurrentModels.Add(new ModelItemViewModel(string.Empty));
		}
	}

	[RelayCommand]
	public void AddModelToCurrent()
	{
		CurrentModels.Add(new ModelItemViewModel(string.Empty));
	}

	[RelayCommand]
	public void RemoveModelFromCurrent(ModelItemViewModel model)
	{
		CurrentModels.Remove(model);
	}

	[RelayCommand]
	public void DeleteKey(KeyItemViewModel item)
	{
		_ = DeleteKeyAsyncUI(item.Id);
	}

	private async Task DeleteKeyAsyncUI(Guid id)
	{
		var key = await _keyManageService.GetAllKeysAsync();
		var target = key.FirstOrDefault(k => k.Id == id);
		if (target != null)
		{
			CurrentKey = target;
			IsEditorVisible = false;
			IsDeleteConfirmVisible = true;
			IsModalOpen = true;
		}
	}

	[RelayCommand]
	public async Task ConfirmDeleteAsync()
	{
		if (CurrentKey != null)
		{
			await _keyManageService.DeleteKeyAsync(CurrentKey.Id);
			await LoadAllConfigsAsync();
		}
		CloseModal();
	}

	[RelayCommand]
	public async Task SaveAsync()
	{
		if (CurrentKey == null) return;

		var vm = new KeyVM
		{
			Id = CurrentKey.Id,
			Supplier = CurrentSupplierInput,
			Key = CurrentKeyInput,
			Endpoint = CurrentEndpointInput,
			Models = CurrentModels.Select(m => m.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
		};

		await _keyManageService.SaveKeyAsync(vm);
		await LoadAllConfigsAsync();
		CloseModal();
	}

	[RelayCommand]
	public void CloseModal()
	{
		IsModalOpen = false;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = false;
		CurrentKey = null;
	}
}