#pragma warning disable SKEXP0110, SKEXP0001
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class LocalPluginLoaderStrategy : IPluginLoaderStrategy
{
    public bool CanHandle(PluginType type) => type == PluginType.LocalDll;

    public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(plugin.Source)) return;

        var fullPath = Path.GetFullPath(plugin.Source);
        if (!File.Exists(fullPath)) return;

        try
        {
            var assembly = Assembly.LoadFrom(fullPath);
            var pluginName = plugin.Name ?? Path.GetFileNameWithoutExtension(fullPath);

            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.GetMethods().Any(m => m.GetCustomAttributes(typeof(KernelFunctionAttribute), true).Any()))
                {
                    var skPlugin = KernelPluginFactory.CreateFromType(type, pluginName);
                    kernel.Plugins.Add(skPlugin);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load local plugin DLL: {ex.Message}");
        }
    }
}
