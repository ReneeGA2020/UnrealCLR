using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace UnrealEngine.Plugins.Loader;

public static class RuntimeConfigExtensions
{
	private const string JsonExt = ".json";

	private static readonly JsonSerializerOptions s_serializerOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public static AssemblyLoadContextBuilder TryAddAdditionalProbingPathFromRuntimeConfig(this AssemblyLoadContextBuilder builder, string runtimeConfigPath, bool includeDevConfig, out Exception? error)
	{
		error = null;
		try
		{
			RuntimeConfig config = TryReadConfig(runtimeConfigPath);
			if (config == null)
			{
				return builder;
			}
			RuntimeConfig devConfig = null;
			if (includeDevConfig)
			{
				devConfig = TryReadConfig(runtimeConfigPath.Substring(0, runtimeConfigPath.Length - ".json".Length) + ".dev.json");
			}
			string tfm = config.runtimeOptions?.Tfm ?? devConfig?.runtimeOptions?.Tfm;
			if (config.runtimeOptions != null)
			{
				AddProbingPaths(builder, config.runtimeOptions, tfm);
			}
			if (devConfig?.runtimeOptions != null)
			{
				AddProbingPaths(builder, devConfig.runtimeOptions, tfm);
			}
			if (tfm != null)
			{
				string dotnet = Process.GetCurrentProcess().MainModule.FileName;
				if (string.Equals(Path.GetFileNameWithoutExtension(dotnet), "dotnet", StringComparison.OrdinalIgnoreCase))
				{
					string dotnetHome = Path.GetDirectoryName(dotnet);
					if (dotnetHome != null)
					{
						builder.AddProbingPath(Path.Combine(dotnetHome, "store", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), tfm));
					}
				}
			}
		}
		catch (Exception ex)
		{
			error = ex;
		}
		return builder;
	}

	private static void AddProbingPaths(AssemblyLoadContextBuilder builder, RuntimeOptions options, string? tfm)
	{
		if (options.AdditionalProbingPaths == null)
		{
			return;
		}
		string[] additionalProbingPaths = options.AdditionalProbingPaths;
		for (int i = 0; i < additionalProbingPaths.Length; i++)
		{
			string path = additionalProbingPaths[i];
			if (path.Contains("|arch|"))
			{
				path = path.Replace("|arch|", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant());
			}
			if (path.Contains("|tfm|"))
			{
				if (tfm == null)
				{
					continue;
				}
				path = path.Replace("|tfm|", tfm);
			}
			builder.AddProbingPath(path);
		}
	}

	private static RuntimeConfig? TryReadConfig(string path)
	{
		try
		{
			return JsonSerializer.Deserialize<RuntimeConfig>(File.ReadAllBytes(path), s_serializerOptions);
		}
		catch
		{
			return null;
		}
	}
}
