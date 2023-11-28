namespace UnrealEngine.Plugins;

public class PluginReloadedEventArgs(PluginLoader loader) : EventArgs
{
    public PluginLoader Loader { get; } = loader;
}
