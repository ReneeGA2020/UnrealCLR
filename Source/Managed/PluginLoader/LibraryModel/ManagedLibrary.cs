using System.Diagnostics;
using System.Reflection;

namespace UnrealEngine.Plugins.LibraryModel;

[DebuggerDisplay("{Name} = {AdditionalProbingPath}")]
public class ManagedLibrary
{
    public AssemblyName Name { get; private set; }

    public string AdditionalProbingPath { get; private set; }

    public string AppLocalPath { get; private set; }

    private ManagedLibrary(AssemblyName name, string additionalProbingPath, string appLocalPath)
    {
        Name = name ?? throw new ArgumentNullException("name");
        AdditionalProbingPath = additionalProbingPath ?? throw new ArgumentNullException("additionalProbingPath");
        AppLocalPath = appLocalPath ?? throw new ArgumentNullException("appLocalPath");
    }

    public static ManagedLibrary CreateFromPackage(string packageId, string packageVersion, string assetPath)
    {
        string appLocalPath = assetPath.StartsWith("lib/") ? Path.GetFileName(assetPath) : assetPath;
        return new ManagedLibrary(new AssemblyName(Path.GetFileNameWithoutExtension(assetPath)), Path.Combine(packageId.ToLowerInvariant(), packageVersion, assetPath), appLocalPath);
    }
}
