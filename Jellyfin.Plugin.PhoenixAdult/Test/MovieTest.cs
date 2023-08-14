using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhoenixAdult.Providers;

namespace PhoenixAdult.Test
{
    [TestClass]
    public class MovieTest
    {
        private readonly CancellationToken token = new ();

        private readonly MovieProvider provider = ServiceLocator.GetService<MovieProvider>();

        private readonly PhoenixAdult.Plugin plugin = ServiceLocator.GetService<PhoenixAdult.Plugin>();

        [TestMethod]
        public async Task JAVLibrary()
        {
            var result = await this.provider.GetMetadata(
                new MovieInfo
                {
                    ProviderIds = new Dictionary<string, string> { { Plugin.Instance.Name, "42#0#38#0#Sq3N51oWpmK4AFBHC1WXynU" } },
                }, this.token);
            Assert.IsNotNull(result.Item, "movie should not be null");
            Assert.AreEqual("REBDB-267", result.Item.OriginalTitle, "should return correct javID");
            Assert.AreEqual("鈴木心春", result.People[0].Name, "should return correct actor");
        }
    }
}