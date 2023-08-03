using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace PhoenixAdult.ScheduledTasks
{
    public class AddCollection : IScheduledTask
    {
        private readonly ILibraryManager libraryManager;

        private readonly ICollectionManager collectionManager;
        private readonly ILogger<AddCollection> log;

        public AddCollection(ILibraryManager libraryManager, ICollectionManager collectionManager, ILogger<AddCollection> log)
        {
            this.libraryManager = libraryManager;
            this.collectionManager = collectionManager;
            this.log = log;
        }

        public string Key => Plugin.Instance.Name + "AddCollection";

        public string Name => "Add Collection";

        public string Description => "Creates Collection for every scene";

        public string Category => Plugin.Instance.Name;

#if __EMBY__
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
#else
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
#endif
        {
            await Task.Yield();
            progress?.Report(0);

            var items = this.libraryManager.GetItemList(new InternalItemsQuery()).Where(o => o.ProviderIds.ContainsKey(Plugin.Instance.Name));

            var replacements = new Dictionary<string, string>
                                {
                                    { Regex.Escape("&nbsp;"), string.Empty }
                                };

            var studios = items.SelectMany(o => o.Studios.Select(studio => ReplaceStrings(studio, replacements))).Distinct().ToList();

            foreach (var (idx, studio) in studios.WithIndex())
            {
                progress?.Report((double)idx / studios.Count * 100);

                var movies = items.Where(o => o.Studios.Any(studio => ReplaceStrings(studio, replacements).Equals(studio, StringComparison.OrdinalIgnoreCase)))
                  .ToList();
                var option = new CollectionCreationOptions
                {
                    Name = studio,
#if __EMBY__
                    ItemIdList = movies.Select(o => o.InternalId).ToArray(),
#else
                    ItemIdList = movies.Select(o => o.Id.ToString()).ToArray(),
#endif
                };

#if __EMBY__
                var collection = await this.collectionManager.CreateCollection(option).ConfigureAwait(false);
#else
                var collection = await this.collectionManager.CreateCollectionAsync(option).ConfigureAwait(false);
#endif

                var moviesImages = movies.Where(o => o.HasImage(ImageType.Primary));
                if (moviesImages.Any())
                {
                    collection.SetImage(moviesImages.Random().GetImageInfo(ImageType.Primary, 0), 0);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
            string ReplaceStrings(string input, Dictionary<string, string> replacements)
            {
                var regex = new Regex(string.Join("|", replacements.Keys.Select(Regex.Escape)));
                return regex.Replace(input, match => replacements[match.Value]);
            }

            progress?.Report(100);
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerWeekly, DayOfWeek = DayOfWeek.Sunday, TimeOfDayTicks = TimeSpan.FromHours(12).Ticks };
        }
    }
}
