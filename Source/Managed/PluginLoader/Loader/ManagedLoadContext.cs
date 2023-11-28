using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using UnrealEngine.Plugins.LibraryModel;

namespace UnrealEngine.Plugins.Loader;

[DebuggerDisplay("'{Name}' ({_mainAssemblyPath})")]
internal class ManagedLoadContext : AssemblyLoadContext
{
	private readonly string _basePath;

	private readonly string _mainAssemblyPath;

	private readonly IReadOnlyDictionary<string, ManagedLibrary> _managedAssemblies;

	private readonly IReadOnlyDictionary<string, NativeLibrary> _nativeLibraries;

	private readonly IReadOnlyCollection<string> _privateAssemblies;

	private readonly ICollection<string> _defaultAssemblies;

	private readonly IReadOnlyCollection<string> _additionalProbingPaths;

	private readonly bool _preferDefaultLoadContext;

	private readonly string[] _resourceRoots;

	private readonly bool _loadInMemory;

	private readonly bool _lazyLoadReferences;

	private readonly AssemblyLoadContext _defaultLoadContext;

	private readonly AssemblyDependencyResolver _dependencyResolver;

	private readonly bool _shadowCopyNativeLibraries;

	private readonly string _unmanagedDllShadowCopyDirectoryPath;

	public ManagedLoadContext(string mainAssemblyPath, IReadOnlyDictionary<string, ManagedLibrary> managedAssemblies, IReadOnlyDictionary<string, NativeLibrary> nativeLibraries, IReadOnlyCollection<string> privateAssemblies, IReadOnlyCollection<string> defaultAssemblies, IReadOnlyCollection<string> additionalProbingPaths, IReadOnlyCollection<string> resourceProbingPaths, AssemblyLoadContext defaultLoadContext, bool preferDefaultLoadContext, bool lazyLoadReferences, bool isCollectible, bool loadInMemory, bool shadowCopyNativeLibraries)
		: base(Path.GetFileNameWithoutExtension(mainAssemblyPath), isCollectible)
	{
		if (resourceProbingPaths == null)
		{
			throw new ArgumentNullException("resourceProbingPaths");
		}
		_mainAssemblyPath = mainAssemblyPath ?? throw new ArgumentNullException("mainAssemblyPath");
		_dependencyResolver = new AssemblyDependencyResolver(mainAssemblyPath);
		_basePath = Path.GetDirectoryName(mainAssemblyPath) ?? throw new ArgumentException("mainAssemblyPath");
		_managedAssemblies = managedAssemblies ?? throw new ArgumentNullException("managedAssemblies");
		_privateAssemblies = privateAssemblies ?? throw new ArgumentNullException("privateAssemblies");
		if (defaultAssemblies == null)
		{
			throw new ArgumentNullException("defaultAssemblies");
		}
		_defaultAssemblies = defaultAssemblies.ToList();
		_nativeLibraries = nativeLibraries ?? throw new ArgumentNullException("nativeLibraries");
		_additionalProbingPaths = additionalProbingPaths ?? throw new ArgumentNullException("additionalProbingPaths");
		_defaultLoadContext = defaultLoadContext;
		_preferDefaultLoadContext = preferDefaultLoadContext;
		_loadInMemory = loadInMemory;
		_lazyLoadReferences = lazyLoadReferences;
		_resourceRoots = new string[1] { _basePath }.Concat(resourceProbingPaths).ToArray();
		_shadowCopyNativeLibraries = shadowCopyNativeLibraries;
		_unmanagedDllShadowCopyDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		if (shadowCopyNativeLibraries)
		{
			base.Unloading += delegate
			{
				OnUnloaded();
			};
		}
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		if (assemblyName.Name == null)
		{
			return null;
		}
		if ((_preferDefaultLoadContext || _defaultAssemblies.Contains(assemblyName.Name)) && !_privateAssemblies.Contains<string>(assemblyName.Name))
		{
			try
			{
				Assembly defaultAssembly = _defaultLoadContext.LoadFromAssemblyName(assemblyName);
				if (defaultAssembly != null)
				{
					if (_lazyLoadReferences)
					{
						AssemblyName[] referencedAssemblies = defaultAssembly.GetReferencedAssemblies();
						foreach (AssemblyName reference in referencedAssemblies)
						{
							if (reference.Name != null && !_defaultAssemblies.Contains(reference.Name))
							{
								_defaultAssemblies.Add(reference.Name);
							}
						}
					}
					return defaultAssembly;
				}
			}
			catch
			{
			}
		}
		string? resolvedPath = _dependencyResolver.ResolveAssemblyToPath(assemblyName);
		if (!string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath))
		{
			return LoadAssemblyFromFilePath(resolvedPath);
		}
		if (!string.IsNullOrEmpty(assemblyName.CultureName) && !string.Equals("neutral", assemblyName.CultureName))
		{
			string[] resourceRoots = _resourceRoots;
			for (int i = 0; i < resourceRoots.Length; i++)
			{
				string resourcePath = Path.Combine(resourceRoots[i], assemblyName.CultureName, assemblyName.Name + ".dll");
				if (File.Exists(resourcePath))
				{
					return LoadAssemblyFromFilePath(resourcePath);
				}
			}
			return null;
		}
		if (_managedAssemblies.TryGetValue(assemblyName.Name, out var library) && library != null)
		{
			if (SearchForLibrary(library, out string? path) && path != null)
			{
				return LoadAssemblyFromFilePath(path);
			}
		}
		else
		{
			string dllName = assemblyName.Name + ".dll";
			foreach (string item in _additionalProbingPaths.Prepend(_basePath))
			{
				string localFile = Path.Combine(item, dllName);
				if (File.Exists(localFile))
				{
					return LoadAssemblyFromFilePath(localFile);
				}
			}
		}
		return null;
	}

	public Assembly LoadAssemblyFromFilePath(string path)
	{
		if (!_loadInMemory)
		{
			return LoadFromAssemblyPath(path);
		}
		using FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		string pdbPath = Path.ChangeExtension(path, ".pdb");
		if (File.Exists(pdbPath))
		{
			using (FileStream pdbFile = File.Open(pdbPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return LoadFromStream(file, pdbFile);
			}
		}
		return LoadFromStream(file);
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		string? resolvedPath = _dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		if (!string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath))
		{
			return LoadUnmanagedDllFromResolvedPath(resolvedPath, normalizePath: false);
		}
		string[] nativeLibraryPrefixes = PlatformInformation.NativeLibraryPrefixes;
		foreach (string prefix in nativeLibraryPrefixes)
		{
			if (_nativeLibraries.TryGetValue(prefix + unmanagedDllName, out var library))
			{
				if (SearchForLibrary(library, prefix, out string? path) && path != null)
				{
					return LoadUnmanagedDllFromResolvedPath(path);
				}
				continue;
			}
			string[] nativeLibraryExtensions = PlatformInformation.NativeLibraryExtensions;
			foreach (string suffix in nativeLibraryExtensions)
			{
				if (!unmanagedDllName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				string trimmedName = unmanagedDllName.Substring(0, unmanagedDllName.Length - suffix.Length);
				if (_nativeLibraries.TryGetValue(prefix + trimmedName, out library))
				{
					if (SearchForLibrary(library, prefix, out string? path2) && path2 != null)
					{
						return LoadUnmanagedDllFromResolvedPath(path2);
					}
					continue;
				}
				string prefixSuffixDllName = prefix + unmanagedDllName + suffix;
				string prefixDllName = prefix + unmanagedDllName;
				foreach (string probingPath in _additionalProbingPaths.Prepend(_basePath))
				{
					string localFile = Path.Combine(probingPath, prefixSuffixDllName);
					if (File.Exists(localFile))
					{
						return LoadUnmanagedDllFromResolvedPath(localFile);
					}
					string localFileWithoutSuffix = Path.Combine(probingPath, prefixDllName);
					if (File.Exists(localFileWithoutSuffix))
					{
						return LoadUnmanagedDllFromResolvedPath(localFileWithoutSuffix);
					}
				}
			}
		}
		return base.LoadUnmanagedDll(unmanagedDllName);
	}

	private bool SearchForLibrary(ManagedLibrary library, out string? path)
	{
		string localFile = Path.Combine(_basePath, library.AppLocalPath);
		if (File.Exists(localFile))
		{
			path = localFile;
			return true;
		}
		foreach (string additionalProbingPath in _additionalProbingPaths)
		{
			string candidate = Path.Combine(additionalProbingPath, library.AdditionalProbingPath);
			if (File.Exists(candidate))
			{
				path = candidate;
				return true;
			}
		}
		string[] managedAssemblyExtensions = PlatformInformation.ManagedAssemblyExtensions;
		foreach (string ext in managedAssemblyExtensions)
		{
			string local = Path.Combine(_basePath, library.Name.Name + ext);
			if (File.Exists(local))
			{
				path = local;
				return true;
			}
		}
		path = null;
		return false;
	}

	private bool SearchForLibrary(NativeLibrary library, string prefix, out string? path)
	{
		string[] nativeLibraryExtensions = PlatformInformation.NativeLibraryExtensions;
		foreach (string ext in nativeLibraryExtensions)
		{
			string candidate2 = Path.Combine(_basePath, prefix + library.Name + ext);
			if (File.Exists(candidate2))
			{
				path = candidate2;
				return true;
			}
		}
		string local = Path.Combine(_basePath, library.AppLocalPath);
		if (File.Exists(local))
		{
			path = local;
			return true;
		}
		foreach (string additionalProbingPath in _additionalProbingPaths)
		{
			string candidate = Path.Combine(additionalProbingPath, library.AdditionalProbingPath);
			if (File.Exists(candidate))
			{
				path = candidate;
				return true;
			}
		}
		path = null;
		return false;
	}

	private IntPtr LoadUnmanagedDllFromResolvedPath(string unmanagedDllPath, bool normalizePath = true)
	{
		if (normalizePath)
		{
			unmanagedDllPath = Path.GetFullPath(unmanagedDllPath);
		}
        return !_shadowCopyNativeLibraries ? LoadUnmanagedDllFromPath(unmanagedDllPath) : LoadUnmanagedDllFromShadowCopy(unmanagedDllPath);
    }

    private IntPtr LoadUnmanagedDllFromShadowCopy(string unmanagedDllPath)
	{
		string shadowCopyDllPath = CreateShadowCopy(unmanagedDllPath);
		return LoadUnmanagedDllFromPath(shadowCopyDllPath);
	}

	private string CreateShadowCopy(string dllPath)
	{
		Directory.CreateDirectory(_unmanagedDllShadowCopyDirectoryPath);
		string dllFileName = Path.GetFileName(dllPath);
		string shadowCopyPath = Path.Combine(_unmanagedDllShadowCopyDirectoryPath, dllFileName);
		if (!File.Exists(shadowCopyPath))
		{
			File.Copy(dllPath, shadowCopyPath);
		}
		return shadowCopyPath;
	}

	private void OnUnloaded()
	{
		if (!_shadowCopyNativeLibraries || !Directory.Exists(_unmanagedDllShadowCopyDirectoryPath))
		{
			return;
		}
		try
		{
			Directory.Delete(_unmanagedDllShadowCopyDirectoryPath, true);
		}
		catch (Exception)
		{
		}
	}
}
