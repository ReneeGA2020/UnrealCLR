using System;
using System.Diagnostics;
using System.IO;

namespace UnrealEngine.Plugins.LibraryModel;

[DebuggerDisplay("{Name} = {AdditionalProbingPath}")]
public class NativeLibrary
{
	public string Name { get; private set; }

	public string AppLocalPath { get; private set; }

	public string AdditionalProbingPath { get; private set; }

	private NativeLibrary(string name, string appLocalPath, string additionalProbingPath)
	{
		Name = name ?? throw new ArgumentNullException("name");
		AppLocalPath = appLocalPath ?? throw new ArgumentNullException("appLocalPath");
		AdditionalProbingPath = additionalProbingPath ?? throw new ArgumentNullException("additionalProbingPath");
	}

	public static NativeLibrary CreateFromPackage(string packageId, string packageVersion, string assetPath)
	{
		return new NativeLibrary(Path.GetFileNameWithoutExtension(assetPath), assetPath, Path.Combine(packageId.ToLowerInvariant(), packageVersion, assetPath));
	}
}
