using System.Reflection;
using System.Runtime.Loader;

namespace UnrealEngine.Plugins;

public class PluginConfig
{
    private bool _isUnloadable;

    private bool _loadInMemory;

    public string MainAssemblyPath { get; }

    public ICollection<AssemblyName> PrivateAssemblies { get; protected set; } = new List<AssemblyName>();


    public ICollection<AssemblyName> SharedAssemblies { get; protected set; } = new List<AssemblyName>();


    public bool PreferSharedTypes { get; set; }

    public bool IsLazyLoaded { get; set; }

    public AssemblyLoadContext DefaultContext { get; set; } = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()) ?? AssemblyLoadContext.Default;


    public bool IsUnloadable
    {
        get => _isUnloadable || EnableHotReload;
        set => _isUnloadable = value;
    }

    public bool LoadInMemory
    {
        get => _loadInMemory || EnableHotReload;
        set => _loadInMemory = value;
    }

    public bool EnableHotReload { get; set; }

    public TimeSpan ReloadDelay { get; set; } = TimeSpan.FromMilliseconds(200.0);


    public PluginConfig(string mainAssemblyPath)
    {
        if (string.IsNullOrEmpty(mainAssemblyPath))
        {
            throw new ArgumentException("Value must be null or not empty", nameof(mainAssemblyPath));
        }
        if (!Path.IsPathRooted(mainAssemblyPath))
        {
            throw new ArgumentException("Value must be an absolute file path", nameof(mainAssemblyPath));
        }
        MainAssemblyPath = mainAssemblyPath;
    }
}
