using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using UnrealEngine.Plugins.LibraryModel;

namespace UnrealEngine.Plugins.Loader;

public class AssemblyLoadContextBuilder
{
	private readonly List<string> _additionalProbingPaths = new List<string>();

	private readonly List<string> _resourceProbingPaths = new List<string>();

	private readonly List<string> _resourceProbingSubpaths = new List<string>();

	private readonly Dictionary<string, ManagedLibrary> _managedLibraries = new Dictionary<string, ManagedLibrary>(StringComparer.Ordinal);

	private readonly Dictionary<string, NativeLibrary> _nativeLibraries = new Dictionary<string, NativeLibrary>(StringComparer.Ordinal);

	private readonly HashSet<string> _privateAssemblies = new HashSet<string>(StringComparer.Ordinal);

	private readonly HashSet<string> _defaultAssemblies = new HashSet<string>(StringComparer.Ordinal);

	private AssemblyLoadContext _defaultLoadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()) ?? AssemblyLoadContext.Default;

	private string? _mainAssemblyPath;

	private bool _preferDefaultLoadContext;

	private bool _lazyLoadReferences;

	private bool _isCollectible;

	private bool _loadInMemory;

	private bool _shadowCopyNativeLibraries;

	public AssemblyLoadContext Build()
	{
		List<string> resourceProbingPaths = new List<string>(_resourceProbingPaths);
		foreach (string additionalPath in _additionalProbingPaths)
		{
			foreach (string subPath in _resourceProbingSubpaths)
			{
				resourceProbingPaths.Add(Path.Combine(additionalPath, subPath));
			}
		}
		if (_mainAssemblyPath == null)
		{
			throw new InvalidOperationException("Missing required property. You must call 'SetMainAssemblyPath' to configure the default assembly.");
		}
		return new ManagedLoadContext(_mainAssemblyPath, _managedLibraries, _nativeLibraries, _privateAssemblies, _defaultAssemblies, _additionalProbingPaths, resourceProbingPaths, _defaultLoadContext, _preferDefaultLoadContext, _lazyLoadReferences, _isCollectible, _loadInMemory, _shadowCopyNativeLibraries);
	}

	public AssemblyLoadContextBuilder SetMainAssemblyPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Argument must not be null or empty.", "path");
		}
		if (!Path.IsPathRooted(path))
		{
			throw new ArgumentException("Argument must be a full path.", "path");
		}
		_mainAssemblyPath = path;
		return this;
	}

	public AssemblyLoadContextBuilder SetDefaultContext(AssemblyLoadContext context)
	{
		_defaultLoadContext = context ?? throw new ArgumentException("Bad Argument: AssemblyLoadContext in AssemblyLoadContextBuilder.SetDefaultContext is null.");
		return this;
	}

	public AssemblyLoadContextBuilder PreferLoadContextAssembly(AssemblyName assemblyName)
	{
		if (assemblyName.Name != null)
		{
			_privateAssemblies.Add(assemblyName.Name);
		}
		return this;
	}

	public AssemblyLoadContextBuilder PreferDefaultLoadContextAssembly(AssemblyName assemblyName)
	{
		if (_lazyLoadReferences)
		{
			if (assemblyName.Name != null && !_defaultAssemblies.Contains(assemblyName.Name))
			{
				_defaultAssemblies.Add(assemblyName.Name);
				AssemblyName[] referencedAssemblies = _defaultLoadContext.LoadFromAssemblyName(assemblyName).GetReferencedAssemblies();
				foreach (AssemblyName reference in referencedAssemblies)
				{
					if (reference.Name != null)
					{
						_defaultAssemblies.Add(reference.Name);
					}
				}
			}
			return this;
		}
		Queue<AssemblyName> names = new Queue<AssemblyName>();
		names.Enqueue(assemblyName);
		AssemblyName name;
		while (names.TryDequeue(out name))
		{
			if (name.Name != null && !_defaultAssemblies.Contains(name.Name))
			{
				_defaultAssemblies.Add(name.Name);
				AssemblyName[] referencedAssemblies = _defaultLoadContext.LoadFromAssemblyName(name).GetReferencedAssemblies();
				foreach (AssemblyName reference2 in referencedAssemblies)
				{
					names.Enqueue(reference2);
				}
			}
		}
		return this;
	}

	public AssemblyLoadContextBuilder PreferDefaultLoadContext(bool preferDefaultLoadContext)
	{
		_preferDefaultLoadContext = preferDefaultLoadContext;
		return this;
	}

	public AssemblyLoadContextBuilder IsLazyLoaded(bool isLazyLoaded)
	{
		_lazyLoadReferences = isLazyLoaded;
		return this;
	}

	public AssemblyLoadContextBuilder AddManagedLibrary(ManagedLibrary library)
	{
		ValidateRelativePath(library.AdditionalProbingPath);
		if (library.Name.Name != null)
		{
			_managedLibraries.Add(library.Name.Name, library);
		}
		return this;
	}

	public AssemblyLoadContextBuilder AddNativeLibrary(NativeLibrary library)
	{
		ValidateRelativePath(library.AppLocalPath);
		ValidateRelativePath(library.AdditionalProbingPath);
		_nativeLibraries.Add(library.Name, library);
		return this;
	}

	public AssemblyLoadContextBuilder AddProbingPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Value must not be null or empty.", "path");
		}
		if (!Path.IsPathRooted(path))
		{
			throw new ArgumentException("Argument must be a full path.", "path");
		}
		_additionalProbingPaths.Add(path);
		return this;
	}

	public AssemblyLoadContextBuilder AddResourceProbingPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Value must not be null or empty.", "path");
		}
		if (!Path.IsPathRooted(path))
		{
			throw new ArgumentException("Argument must be a full path.", "path");
		}
		_resourceProbingPaths.Add(path);
		return this;
	}

	public AssemblyLoadContextBuilder EnableUnloading()
	{
		_isCollectible = true;
		return this;
	}

	public AssemblyLoadContextBuilder PreloadAssembliesIntoMemory()
	{
		_loadInMemory = true;
		return this;
	}

	public AssemblyLoadContextBuilder ShadowCopyNativeLibraries()
	{
		_shadowCopyNativeLibraries = true;
		return this;
	}

	internal AssemblyLoadContextBuilder AddResourceProbingSubpath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Value must not be null or empty.", "path");
		}
		if (Path.IsPathRooted(path))
		{
			throw new ArgumentException("Argument must be not a full path.", "path");
		}
		_resourceProbingSubpaths.Add(path);
		return this;
	}

	private static void ValidateRelativePath(string probingPath)
	{
		if (string.IsNullOrEmpty(probingPath))
		{
			throw new ArgumentException("Value must not be null or empty.", "probingPath");
		}
		if (Path.IsPathRooted(probingPath))
		{
			throw new ArgumentException("Argument must be a relative path.", "probingPath");
		}
	}
}
