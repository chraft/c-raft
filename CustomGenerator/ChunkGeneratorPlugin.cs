using System;
using System.Reflection;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World;

namespace CustomGenerator
{
    [Plugin]
    public class ChunkGeneratorPlugin : IPlugin
    {
        public string Name { get { return "Custom Chunk Generator"; } }

        public string Author { get { return "C#raft Team"; } }

        public string Description { get { return "C#raft official chunk generator plugin"; } }

        public string Website { get { return "http://www.c-raft.com"; } }

        public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public IServer Server { get; set; }

        public IPluginManager PluginManager { get; set; }

        public bool IsPluginEnabled { get; set; }

        private IChunkGenerator _chunkGenerator;

        public void Initialize()
        {
            _chunkGenerator = new CustomChunkGenerator();
            Server.AddChunkGenerator("Default", _chunkGenerator);
        }

        public void Associate(IServer server, IPluginManager pluginManager)
        {
            Server = server;
            
            PluginManager = pluginManager;
        }

        public void OnEnabled()
        {
            IsPluginEnabled = true;
            Server.GetLogger().Log(LogLevel.Info, "Plugin {0} v{1} Enabled", Name, Version);
        }

        public void OnDisabled()
        {
            IsPluginEnabled = false;
            Server.GetLogger().Log(LogLevel.Info, "Plugin {0} v{1} Disabled", Name, Version);
        }
    }
}
