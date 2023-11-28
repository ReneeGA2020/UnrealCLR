using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using UnrealEngine.Plugins.Internal;
using UnrealEngine.Plugins.Loader;

namespace UnrealEngine.Plugins;

public class PluginLoader : IDisposable
{
	private readonly PluginConfig _config;

	private ManagedLoadContext _context;

	private readonly AssemblyLoadContextBuilder _contextBuilder;

	private volatile bool _disposed;

	private FileSystemWatcher? _fileWatcher;

	private Debouncer? _debouncer;

	public bool IsUnloadable => _context.IsCollectible;

	internal AssemblyLoadContext LoadContext => _context;

	public event PluginReloadedEventHandler? Reloaded;

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
	{
		return CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, delegate
		{
		});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes, Action<PluginConfig> configure)
	{
		return CreateFromAssemblyFile(assemblyFile, sharedTypes, delegate(PluginConfig config)
		{
			config.IsUnloadable = isUnloadable;
			configure(config);
		});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
	{
		return CreateFromAssemblyFile(assemblyFile, sharedTypes, delegate
		{
		});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes, Action<PluginConfig> configure)
	{
		return CreateFromAssemblyFile(assemblyFile, delegate(PluginConfig config)
		{
			if (sharedTypes != null)
			{
				HashSet<Assembly> hashSet = new HashSet<Assembly>();
				Type[] array = sharedTypes;
				foreach (Type type in array)
				{
					hashSet.Add(type.Assembly);
				}
				foreach (Assembly current in hashSet)
				{
					config.SharedAssemblies.Add(current.GetName());
				}
			}
			configure(config);
		});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile)
	{
		return CreateFromAssemblyFile(assemblyFile, delegate
		{
		});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Action<PluginConfig> configure)
	{
		if (configure == null)
		{
			throw new ArgumentNullException("configure");
		}
		PluginConfig config = new PluginConfig(assemblyFile);
		configure(config);
		return new PluginLoader(config);
	}

	public PluginLoader(PluginConfig config)
	{
		_config = config ?? throw new ArgumentNullException("config");
		_contextBuilder = CreateLoadContextBuilder(config);
		_context = (ManagedLoadContext)_contextBuilder.Build();
		if (config.EnableHotReload)
		{
			StartFileWatcher();
		}
	}

	public void Reload()
	{
		EnsureNotDisposed();
		if (!IsUnloadable)
		{
			throw new InvalidOperationException("Reload cannot be used because IsUnloadable is false");
		}
		_context.Unload();
		_context = (ManagedLoadContext)_contextBuilder.Build();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		this.Reloaded?.Invoke(this, new PluginReloadedEventArgs(this));
	}

	private void StartFileWatcher()
	{
		_debouncer = new Debouncer(_config.ReloadDelay);
		_fileWatcher = new FileSystemWatcher
		{
			Path = Path.GetDirectoryName(_config.MainAssemblyPath)
		};
		_fileWatcher.Changed += OnFileChanged;
		_fileWatcher.Filter = "*.dll";
		_fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
		_fileWatcher.EnableRaisingEvents = true;
	}

	private void OnFileChanged(object source, FileSystemEventArgs e)
	{
		if (!_disposed)
		{
			_debouncer?.Execute(Reload);
		}
	}

	public Assembly LoadDefaultAssembly()
	{
		EnsureNotDisposed();
		return _context.LoadAssemblyFromFilePath(_config.MainAssemblyPath);
	}

	public Assembly LoadAssembly(AssemblyName assemblyName)
	{
		EnsureNotDisposed();
		return _context.LoadFromAssemblyName(assemblyName);
	}

	public Assembly LoadAssemblyFromPath(string assemblyPath)
	{
		return _context.LoadAssemblyFromFilePath(assemblyPath);
	}

	public Assembly LoadAssembly(string assemblyName)
	{
		EnsureNotDisposed();
		return LoadAssembly(new AssemblyName(assemblyName));
	}

	public AssemblyLoadContext.ContextualReflectionScope EnterContextualReflection()
	{
		return _context.EnterContextualReflection();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (_fileWatcher != null)
			{
				_fileWatcher.EnableRaisingEvents = false;
				_fileWatcher.Changed -= OnFileChanged;
				_fileWatcher.Dispose();
			}
			_debouncer?.Dispose();
			if (_context.IsCollectible)
			{
				_context.Unload();
			}
		}
	}

	private void EnsureNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("PluginLoader");
		}
	}

	private static AssemblyLoadContextBuilder CreateLoadContextBuilder(PluginConfig config)
	{
		AssemblyLoadContextBuilder builder = new AssemblyLoadContextBuilder();
		builder.SetMainAssemblyPath(config.MainAssemblyPath);
		builder.SetDefaultContext(config.DefaultContext);
		foreach (AssemblyName ext in config.PrivateAssemblies)
		{
			builder.PreferLoadContextAssembly(ext);
		}
		if (config.PreferSharedTypes)
		{
			builder.PreferDefaultLoadContext(preferDefaultLoadContext: true);
		}
		if (config.IsUnloadable || config.EnableHotReload)
		{
			builder.EnableUnloading();
		}
		if (config.LoadInMemory)
		{
			builder.PreloadAssembliesIntoMemory();
			builder.ShadowCopyNativeLibraries();
		}
		builder.IsLazyLoaded(config.IsLazyLoaded);
		foreach (AssemblyName assemblyName in config.SharedAssemblies)
		{
			builder.PreferDefaultLoadContextAssembly(assemblyName);
		}
		return builder;
	}
}
