using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace UnrealEngine.Plugins.Loader;

public static class RuntimeConfigExtensions
{
    private static readonly JsonSerializerOptions s_serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AssemblyLoadContextBuilder TryAddAdditionalProbingPathFromRuntimeConfig(this AssemblyLoadContextBuilder builder, string runtimeConfigPath, bool includeDevConfig, out Exception? error)
    {
        error = null;
        try
        {
            RuntimeConfig? config = TryReadConfig(runtimeConfigPath);
            if (config == null)
            {
                return builder;
            }
            RuntimeConfig? devConfig = null;
            if (includeDevConfig)
            {
                devConfig = TryReadConfig(string.Concat(runtimeConfigPath.AsSpan(0, runtimeConfigPath.Length - ".json".Length), ".dev.json"));
            }
            string? tfm = config.RuntimeOptions?.Tfm ?? devConfig?.RuntimeOptions?.Tfm;
            if (config.RuntimeOptions != null)
            {
                AddProbingPaths(builder, config.RuntimeOptions, tfm);
            }
            if (devConfig?.RuntimeOptions != null)
            {
                AddProbingPaths(builder, devConfig.RuntimeOptions, tfm);
            }
            if (tfm != null)
            {
                string? dotnet = Process.GetCurrentProcess().MainModule?.FileName;
                if (dotnet is not null && string.Equals(Path.GetFileNameWithoutExtension(dotnet), "dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    string? dotnetHome = Path.GetDirectoryName(dotnet);
                    if (dotnetHome != null)
                    {
                        _ = builder.AddProbingPath(Path.Combine(dotnetHome, "store", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), tfm));
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
            _ = builder.AddProbingPath(path);
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
