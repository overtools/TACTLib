using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using DragonLib;
using Microsoft.Extensions.DependencyInjection;
using TACTLib;
using TACTView.Api.Registry;
using TACTView.Models;

namespace TACTView.ViewModels {
    public class ModuleViewModel : Singleton<ModuleViewModel> {
        public ModuleViewModel() {
            var modulesDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "./", "Modules");
            if (!Directory.Exists(modulesDir)) {
                Logger.Warn("TACTView::Modules", "Can't find modules directory!");
                return;
            }

            foreach (var moduleDir in Directory.EnumerateDirectories(modulesDir)) {
                var manifestPath = Path.Combine(moduleDir, "manifest.json");
                if (!File.Exists(manifestPath)) {
                    Logger.Warn("TACTView::Modules", $"Can't find manifest for module {Path.GetFileName(moduleDir)}");
                    continue;
                }

                try {
                    var manifest = JsonSerializer.Deserialize<ModuleManifest>(File.ReadAllText(manifestPath));
                    if (manifest == null || string.IsNullOrEmpty(manifest.EntryPoint) || string.IsNullOrEmpty(manifest.MainModule)) {
                        Logger.Warn("TACTView::Modules", $"Missing or malformed manifest for module {Path.GetDirectoryName(moduleDir)}");
                        continue;
                    }

                    var assembly = Assembly.LoadFile(Path.Combine(moduleDir, manifest.MainModule));
                    var type = assembly.GetType(manifest.EntryPoint);
                    if (type == null) {
                        Logger.Warn("TACTView::Modules", $"Can't find module entry point {manifest.EntryPoint}");
                        continue;
                    }

                    Logger.Warn("TACTView::Modules", $"Loading module {manifest.MainModule}");
                    var services = CreateServiceProvider(type);
                    manifest.Instance = services.GetRequiredService(type);
                    Modules.Add(manifest);
                } catch (Exception e) {
                    Logger.Error("TACTView::Modules", e.ToString());
                }
            }
        }

        private static RegistryViewModel Registry => RegistryViewModel.Instance;
        internal ICollection<ModuleManifest> Modules => new Collection<ModuleManifest>();

        private static ServiceProvider CreateServiceProvider(Type type) {
            return new ServiceCollection()
                   .AddSingleton<IRegistry<IFileHandler>>(Registry)
                   .AddSingleton<IRegistry<IProductConnector>>(Registry)
                   .AddSingleton<IRegistry<IPlugin>>(Registry)
                   .AddSingleton(type)
                   .BuildServiceProvider();
        }
    }
}
