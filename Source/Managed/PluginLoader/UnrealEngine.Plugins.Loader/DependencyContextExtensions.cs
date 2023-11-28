using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using UnrealEngine.Plugins.LibraryModel;

namespace UnrealEngine.Plugins.Loader;

public static class DependencyContextExtensions
{
	public static AssemblyLoadContextBuilder TryAddDependencyContext(this AssemblyLoadContextBuilder builder, string depsFilePath, out Exception? error)
	{
		error = null;
		try
		{
			builder.AddDependencyContext(depsFilePath);
		}
		catch (Exception ex)
		{
			error = ex;
		}
		return builder;
	}

	public static AssemblyLoadContextBuilder AddDependencyContext(this AssemblyLoadContextBuilder builder, string depsFilePath)
	{
		DependencyContextJsonReader reader = new DependencyContextJsonReader();
		using FileStream file = File.OpenRead(depsFilePath);
		DependencyContext deps = reader.Read(file);
		builder.AddDependencyContext(deps);
		return builder;
	}

	private static string GetFallbackRid()
	{
		string ridBase;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			ridBase = "win10";
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			ridBase = "linux";
		}
		else
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return "any";
			}
			ridBase = "osx.10.12";
		}
		return RuntimeInformation.OSArchitecture switch
		{
			Architecture.X86 => ridBase + "-x86", 
			Architecture.X64 => ridBase + "-x64", 
			Architecture.Arm => ridBase + "-arm", 
			Architecture.Arm64 => ridBase + "-arm64", 
			_ => ridBase, 
		};
	}

	public static AssemblyLoadContextBuilder AddDependencyContext(this AssemblyLoadContextBuilder builder, DependencyContext dependencyContext)
	{
		IReadOnlyList<RuntimeFallbacks> ridGraph = ((dependencyContext.RuntimeGraph.Any() || DependencyContext.Default == null) ? dependencyContext.RuntimeGraph : DependencyContext.Default.RuntimeGraph);
		string rid = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
		string fallbackRid = GetFallbackRid();
		RuntimeFallbacks fallbackGraph = ridGraph.FirstOrDefault((RuntimeFallbacks g) => g.Runtime == rid) ?? ridGraph.FirstOrDefault((RuntimeFallbacks g) => g.Runtime == fallbackRid) ?? new RuntimeFallbacks("any");
		foreach (ManagedLibrary managed in dependencyContext.ResolveRuntimeAssemblies(fallbackGraph))
		{
			builder.AddManagedLibrary(managed);
		}
		foreach (RuntimeLibrary library in dependencyContext.ResolveResourceAssemblies())
		{
			foreach (ResourceAssembly resourceAssembly in library.ResourceAssemblies)
			{
				string resourceDir = Path.GetDirectoryName(Path.GetDirectoryName(resourceAssembly.Path));
				if (resourceDir != null)
				{
					string path = Path.Combine(library.Name.ToLowerInvariant(), library.Version, resourceDir);
					builder.AddResourceProbingSubpath(path);
				}
			}
		}
		foreach (UnrealEngine.Plugins.LibraryModel.NativeLibrary native in dependencyContext.ResolveNativeAssets(fallbackGraph))
		{
			builder.AddNativeLibrary(native);
		}
		return builder;
	}

	private static IEnumerable<ManagedLibrary> ResolveRuntimeAssemblies(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
	{
		IEnumerable<string> rids = GetRids(runtimeGraph);
		return from library in depContext.RuntimeLibraries
			from assetPath in SelectAssets(rids, library.RuntimeAssemblyGroups)
			select ManagedLibrary.CreateFromPackage(library.Name, library.Version, assetPath);
	}

	private static IEnumerable<RuntimeLibrary> ResolveResourceAssemblies(this DependencyContext depContext)
	{
		return depContext.RuntimeLibraries.Where((RuntimeLibrary library) => library.ResourceAssemblies != null && library.ResourceAssemblies.Count > 0);
	}

	private static IEnumerable<UnrealEngine.Plugins.LibraryModel.NativeLibrary> ResolveNativeAssets(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
	{
		IEnumerable<string> rids = GetRids(runtimeGraph);
		return from library in depContext.RuntimeLibraries
			from assetPath in SelectAssets(rids, library.NativeLibraryGroups)
			where PlatformInformation.NativeLibraryExtensions.Contains<string>(Path.GetExtension(assetPath), StringComparer.OrdinalIgnoreCase)
			select UnrealEngine.Plugins.LibraryModel.NativeLibrary.CreateFromPackage(library.Name, library.Version, assetPath);
	}

	private static IEnumerable<string> GetRids(RuntimeFallbacks runtimeGraph)
	{
		string[] first = new string[1] { runtimeGraph.Runtime };
		IEnumerable<string> enumerable = runtimeGraph?.Fallbacks;
		return first.Concat(enumerable ?? Enumerable.Empty<string>());
	}

	private static IEnumerable<string> SelectAssets(IEnumerable<string> rids, IEnumerable<RuntimeAssetGroup> groups)
	{
		foreach (string rid in rids)
		{
			RuntimeAssetGroup group = groups.FirstOrDefault((RuntimeAssetGroup g) => g.Runtime == rid);
			if (group != null)
			{
				return group.AssetPaths;
			}
		}
		return groups.GetDefaultAssets();
	}
}
