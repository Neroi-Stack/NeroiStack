using System;
using System.Collections.Generic;
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
	private readonly IKeyManageService _service;
	private readonly KeyManagementViewModel _parent;

	public Guid Id { get; private set; }

	[ObservableProperty]
	private SupplierEnum _supplier;

	[ObservableProperty]
	private string _key = string.Empty;

	[ObservableProperty]
	private string _endpoint = string.Empty;

	public ObservableCollection<ModelItemViewModel> Models { get; } = new();

	public List<SupplierEnum> AvailableSuppliers { get; } = Enum.GetValues(typeof(SupplierEnum)).Cast<SupplierEnum>().ToList();

	public KeyItemViewModel(KeyVM vm, IKeyManageService service, KeyManagementViewModel parent)
	{
		_service = service;
		_parent = parent;
		Id = vm.Id;
		Supplier = vm.Supplier;
		Key = vm.Key ?? "";
		Endpoint = vm.Endpoint ?? "";
		foreach (var model in vm.Models)
		{
			Models.Add(new ModelItemViewModel(model));
		}

		if (Models.Count == 0)
		{
			Models.Add(new ModelItemViewModel(string.Empty));
		}
	}

	public KeyItemViewModel(IKeyManageService service, KeyManagementViewModel parent)
	{
		_service = service;
		_parent = parent;
		Supplier = SupplierEnum.OpenAI; // Default
		Models.Add(new ModelItemViewModel(string.Empty));
	}

	[RelayCommand]
	public void AddModel()
	{
		Models.Add(new ModelItemViewModel(string.Empty));
	}

	[RelayCommand]
	public void RemoveModel(ModelItemViewModel item)
	{
		if (Models.Contains(item))
		{
			Models.Remove(item);
		}
	}

	[RelayCommand]
	public async Task SaveAsync()
	{
		var vm = new KeyVM
		{
			Id = Id,
			Supplier = Supplier,
			Key = Key,
			Endpoint = Endpoint,
			Models = Models.Select(m => m.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
		};
		await _service.SaveKeyAsync(vm);
		await _parent.LoadAllConfigsAsync();
	}

	[RelayCommand]
	public async Task DeleteAsync()
	{
		if (Id != Guid.Empty)
		{
			await _service.DeleteKeyAsync(Id);
		}
		_parent.RemoveKey(this);
	}
}

public sealed partial class KeyManagementViewModel : ViewModelBase
{
	private readonly IKeyManageService _keyManageService;

	public ObservableCollection<KeyItemViewModel> Keys { get; } = new();

	public KeyManagementViewModel(IKeyManageService keyManageService)
	{
		_keyManageService = keyManageService;
		_ = LoadAllConfigsAsync();
	}

	public async Task LoadAllConfigsAsync()
	{
		var keyVms = await _keyManageService.GetAllKeysAsync();
		Keys.Clear();
		foreach (var key in keyVms)
		{
			Keys.Add(new KeyItemViewModel(key, _keyManageService, this));
		}
	}

	[RelayCommand]
	public void AddKey()
	{
		Keys.Insert(0, new KeyItemViewModel(_keyManageService, this));
	}

	public void RemoveKey(KeyItemViewModel item)
	{
		Keys.Remove(item);
	}
}