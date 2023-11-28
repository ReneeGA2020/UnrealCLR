using System;

namespace UnrealEngine.Plugins;

public class PluginReloadedEventArgs : EventArgs
{
	public PluginLoader Loader { get; }

	public PluginReloadedEventArgs(PluginLoader loader)
	{
		Loader = loader;
	}
}
