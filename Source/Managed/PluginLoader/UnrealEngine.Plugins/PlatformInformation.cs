using System;
using System.Runtime.InteropServices;

namespace UnrealEngine.Plugins;

internal class PlatformInformation
{
	public static readonly string[] NativeLibraryExtensions;

	public static readonly string[] NativeLibraryPrefixes;

	public static readonly string[] ManagedAssemblyExtensions;

	static PlatformInformation()
	{
		ManagedAssemblyExtensions = new string[4] { ".dll", ".ni.dll", ".exe", ".ni.exe" };
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			NativeLibraryPrefixes = new string[1] { "" };
			NativeLibraryExtensions = new string[1] { ".dll" };
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			NativeLibraryPrefixes = new string[2] { "", "lib" };
			NativeLibraryExtensions = new string[1] { ".dylib" };
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			NativeLibraryPrefixes = new string[2] { "", "lib" };
			NativeLibraryExtensions = new string[2] { ".so", ".so.1" };
		}
		else
		{
			NativeLibraryPrefixes = Array.Empty<string>();
			NativeLibraryExtensions = Array.Empty<string>();
		}
	}
}
