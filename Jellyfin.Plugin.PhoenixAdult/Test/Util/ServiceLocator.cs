using System.IO;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhoenixAdult.Providers;

namespace PhoenixAdult.Test;

[TestClass]
public class ServiceLocator
{
    private static ServiceProvider provider;

    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddHttpClient();
        serviceCollection.AddSingleton<IXmlSerializer, MockedXmlSerializer>();
        serviceCollection.AddSingleton<IApplicationPaths, MockedApplicationPaths>();
        serviceCollection.AddSingleton<ILibraryManager, MockedLibraryManager>();
        serviceCollection.AddScoped<PhoenixAdult.Plugin>();
        serviceCollection.AddScoped<MovieProvider>();
        serviceCollection.AddScoped<MovieImageProvider>();
        serviceCollection.AddScoped<ActorProvider>();
        serviceCollection.AddScoped<ActorImageProvider>();
        provider = serviceCollection.BuildServiceProvider();

        PrepareDatabaseFiles();
    }

    public static T GetService<T>()
    {
        return provider.GetService<T>();
    }

    private static void PrepareDatabaseFiles()
    {
        var plugin = GetService<PhoenixAdult.Plugin>();
        string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string filesFolderPath = Path.Combine(pluginPath, "..", "..", "..", "..", "data");
        string databasePath = Path.Combine(Plugin.Instance.DataFolderPath, "data");
        Directory.CreateDirectory(databasePath);
        foreach (var file in new[] { "Actors.json", "Genres.json", "SiteList.json" })
        {
            File.Copy(Path.Combine(filesFolderPath, file), Path.Combine(databasePath, file), true);
        }
    }
}