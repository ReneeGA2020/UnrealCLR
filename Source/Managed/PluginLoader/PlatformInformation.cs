using System.Runtime.InteropServices;

namespace UnrealEngine.Plugins;

internal class PlatformInformation
{
    public static readonly string[] NativeLibraryExtensions;

    public static readonly string[] NativeLibraryPrefixes;

    public static readonly string[] ManagedAssemblyExtensions;

    static PlatformInformation()
    {
        ManagedAssemblyExtensions = [".dll", ".ni.dll", ".exe", ".ni.exe"];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            NativeLibraryPrefixes = [""];
            NativeLibraryExtensions = [".dll"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            NativeLibraryPrefixes = ["", "lib"];
            NativeLibraryExtensions = [".dylib"];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            NativeLibraryPrefixes = ["", "lib"];
            NativeLibraryExtensions = [".so", ".so.1"];
        }
        else
        {
            NativeLibraryPrefixes = [];
            NativeLibraryExtensions = [];
        }
    }
}
