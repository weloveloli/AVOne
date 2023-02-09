// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Jellyfin.Base
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using AVOne.Configuration;
    using AVOne.Extensions;
    using AVOne.IO;
    using AVOne.Enum;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;
    using AVOne.Constants;
    using AVOne.Models.Info;

    public abstract class BaseNfoSaver : IMetadataFileSaverProvider
    {
        public const string DateAddedFormat = "yyyy-MM-dd HH:mm:ss";

        public const string YouTubeWatchUrl = "https://www.youtube.com/watch?v=";

        // filters control characters but allows only properly-formed surrogate sequences
        private const string _invalidXMLCharsRegex = @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]";

        private static readonly HashSet<string> _commonTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "plot",
            "customrating",
            "lockdata",
            "dateadded",
            "title",
            "rating",
            "year",
            "sorttitle",
            "mpaa",
            "aspectratio",
            "collectionnumber",
            "tmdbid",
            "rottentomatoesid",
            "language",
            "tvcomid",
            "tagline",
            "studio",
            "genre",
            "tag",
            "runtime",
            "actor",
            "criticrating",
            "fileinfo",
            "director",
            "writer",
            "trailer",
            "premiered",
            "releasedate",
            "outline",
            "id",
            "credits",
            "originaltitle",
            "watched",
            "playcount",
            "lastplayed",
            "art",
            "resume",
            "biography",
            "formed",
            "review",
            "style",
            "imdbid",
            "imdb_id",
            "country",
            "audiodbalbumid",
            "audiodbartistid",
            "enddate",
            "lockedfields",
            "zap2itid",
            "tvrageid",

            "musicbrainzartistid",
            "musicbrainzalbumartistid",
            "musicbrainzalbumid",
            "musicbrainzreleasegroupid",
            "tvdbid",
            "collectionitem",

            "isuserfavorite",
            "userrating",

            "countrycode"
        };

        protected BaseNfoSaver(
            IFileSystem fileSystem,
            IConfigurationManager configurationManager,
            ILogger<BaseNfoSaver> logger)
        {
            Logger = logger;
            ConfigurationManager = configurationManager;
            FileSystem = fileSystem;
        }

        protected IFileSystem FileSystem { get; }

        protected IConfigurationManager ConfigurationManager { get; }

        protected ILogger<BaseNfoSaver> Logger { get; }

        /// <inheritdoc />
        public string Name => SaverName;

        public static string SaverName => "Nfo";

        /// <inheritdoc />
        public string GetSavePath(BaseItem item)
            => GetLocalSavePath(item);

        /// <summary>
        /// Gets the save path.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><see cref="string" />.</returns>
        protected abstract string GetLocalSavePath(BaseItem item);

        /// <summary>
        /// Gets the name of the root element.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><see cref="string" />.</returns>
        protected abstract string GetRootElementName(BaseItem item);

        /// <inheritdoc />
        public abstract bool IsEnabledFor(BaseItem item, ItemUpdateType updateType);

        protected virtual IEnumerable<string> GetTagsUsed(BaseItem item)
        {
            foreach (var providerKey in item.ProviderIds.Keys)
            {
                var providerIdTagName = GetTagForProviderKey(providerKey);
                if (!_commonTags.Contains(providerIdTagName))
                {
                    yield return providerIdTagName;
                }
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync(BaseItem item, CancellationToken cancellationToken)
        {
            var path = GetSavePath(item);

            using (var memoryStream = new MemoryStream())
            {
                Save(item, memoryStream, path);

                memoryStream.Position = 0;

                cancellationToken.ThrowIfCancellationRequested();

                await SaveToFileAsync(memoryStream, path).ConfigureAwait(false);
            }
        }

        private async Task SaveToFileAsync(Stream stream, string path)
        {
            var directory = Path.GetDirectoryName(path) ?? throw new ArgumentException($"Provided path ({path}) is not valid.", nameof(path));
            Directory.CreateDirectory(directory);

            // On Windows, saving the file will fail if the file is hidden or readonly
            FileSystem.SetAttributes(path, false, false);

            var fileStreamOptions = new FileStreamOptions()
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.None,
                PreallocationSize = stream.Length,
                Options = FileOptions.Asynchronous
            };

            var filestream = new FileStream(path, fileStreamOptions);
            await using (filestream.ConfigureAwait(false))
            {
                await stream.CopyToAsync(filestream).ConfigureAwait(false);
            }
        }

        private void SetHidden(string path, bool hidden)
        {
            try
            {
                FileSystem.SetHidden(path, hidden);
            }
            catch (IOException ex)
            {
                Logger.LogError(ex, "Error setting hidden attribute on {Path}", path);
            }
        }

        private void Save(BaseItem item, Stream stream, string xmlPath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                CloseOutput = false
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                var root = GetRootElementName(item);

                writer.WriteStartDocument(true);

                writer.WriteStartElement(root);

                var baseItem = item;

                if (baseItem != null)
                {
                    AddCommonNodes(baseItem, writer, ConfigurationManager);
                }

                WriteCustomElements(item, writer);

                var tagsUsed = GetTagsUsed(item).ToList();

                try
                {
                    AddCustomTags(xmlPath, tagsUsed, writer, Logger);
                }
                catch (FileNotFoundException)
                {
                }
                catch (IOException)
                {
                }
                catch (XmlException ex)
                {
                    Logger.LogError(ex, "Error reading existing nfo");
                }

                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
        }

        protected abstract void WriteCustomElements(BaseItem item, XmlWriter writer);

        /// <summary>
        /// Adds the common nodes.
        /// </summary>
        private void AddCommonNodes(
            BaseItem item,
            XmlWriter writer,
            IConfigurationManager config)
        {
            var writtenProviderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var overview = (item.Overview ?? string.Empty)
                .StripHtml()
                .Replace("&quot;", "'", StringComparison.Ordinal);

            var options = config.GetNfoConfiguration();


            writer.WriteElementString("plot", overview);        

            if (item is not Video)
            {
                writer.WriteElementString("outline", overview);
            }

            if (!string.IsNullOrWhiteSpace(item.CustomRating))
            {
                writer.WriteElementString("customrating", item.CustomRating);
            }


            writer.WriteElementString("dateadded", item.DateCreated.ToString(DateAddedFormat, CultureInfo.InvariantCulture));

            writer.WriteElementString("title", item.Name ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(item.OriginalTitle))
            {
                writer.WriteElementString("originaltitle", item.OriginalTitle);
            }

            var people = item.People;

            var directors = people
                .Where(i => IsPersonType(i, PersonType.Director))
                .Select(i => i.Name)
                .ToList();

            foreach (var person in directors)
            {
                writer.WriteElementString("director", person);
            }

            var writers = people
                .Where(i => IsPersonType(i, PersonType.Writer))
                .Select(i => i.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var person in writers)
            {
                writer.WriteElementString("writer", person);
            }

            foreach (var person in writers)
            {
                writer.WriteElementString("credits", person);
            }

            foreach (var trailer in item.RemoteTrailers)
            {
                writer.WriteElementString("trailer", GetOutputTrailerUrl(trailer.Url));
            }

            if (item.CommunityRating.HasValue)
            {
                writer.WriteElementString("rating", item.CommunityRating.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (item.ProductionYear.HasValue)
            {
                writer.WriteElementString("year", item.ProductionYear.Value.ToString(CultureInfo.InvariantCulture));
            }

            var forcedSortName = item.ForcedSortName;
            if (!string.IsNullOrEmpty(forcedSortName))
            {
                writer.WriteElementString("sorttitle", forcedSortName);
            }

            if (!string.IsNullOrEmpty(item.OfficialRating))
            {
                writer.WriteElementString("mpaa", item.OfficialRating);
            }

            if (!string.IsNullOrEmpty(item.PreferredMetadataLanguage))
            {
                writer.WriteElementString("language", item.PreferredMetadataLanguage);
            }

            if (!string.IsNullOrEmpty(item.PreferredMetadataCountryCode))
            {
                writer.WriteElementString("countrycode", item.PreferredMetadataCountryCode);
            }           

            if (item.CriticRating.HasValue)
            {
                writer.WriteElementString(
                    "criticrating",
                    item.CriticRating.Value.ToString(CultureInfo.InvariantCulture));
            }

            // Use original runtime here, actual file runtime later in MediaInfo
            var runTimeTicks = item.RunTimeTicks;

            if (runTimeTicks.HasValue)
            {
                var timespan = TimeSpan.FromTicks(runTimeTicks.Value);

                writer.WriteElementString(
                    "runtime",
                    Convert.ToInt64(timespan.TotalMinutes).ToString(CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrWhiteSpace(item.Tagline))
            {
                writer.WriteElementString("tagline", item.Tagline);
            }

            foreach (var country in item.ProductionLocations)
            {
                writer.WriteElementString("country", country);
            }

            foreach (var genre in item.Genres)
            {
                writer.WriteElementString("genre", genre);
            }

            foreach (var studio in item.Studios)
            {
                writer.WriteElementString("studio", studio);
            }

            foreach (var tag in item.Tags)
            {

               writer.WriteElementString("tag", tag);

            }

            if (item.ProviderIds != null)
            {
                foreach (var providerKey in item.ProviderIds.Keys)
                {
                    var providerId = item.ProviderIds[providerKey];
                    if (!string.IsNullOrEmpty(providerId) && !writtenProviderIds.Contains(providerKey))
                    {
                        try
                        {
                            var tagName = GetTagForProviderKey(providerKey);
                            Logger.LogDebug("Verifying custom provider tagname {0}", tagName);
                            XmlConvert.VerifyName(tagName);
                            Logger.LogDebug("Saving custom provider tagname {0}", tagName);

                            writer.WriteElementString(GetTagForProviderKey(providerKey), providerId);
                        }
                        catch (ArgumentException)
                        {
                            // catch invalid names without failing the entire operation
                        }
                        catch (XmlException)
                        {
                            // catch invalid names without failing the entire operation
                        }
                    }
                }
            }

            if (options.SaveImagePathsInNfo)
            {
                AddImages(item, writer);
            }

            AddActors(people, writer, options.SaveImagePathsInNfo);
        }

        /// <summary>
        /// Gets the output trailer URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>System.String.</returns>
        private string GetOutputTrailerUrl(string url)
        {
            // This is what xbmc expects
            return url.Replace(YouTubeWatchUrl, "plugin://plugin.video.youtube/?action=play_video&videoid=", StringComparison.OrdinalIgnoreCase);
        }

        private void AddImages(BaseItem item, XmlWriter writer)
        {
            writer.WriteStartElement("art");

            var image = item.GetImageInfo(ImageType.Primary, 0);

            if (image != null)
            {
                writer.WriteElementString("poster", GetImagePathToSave(image));
            }

            foreach (var backdrop in item.GetImages(ImageType.Backdrop))
            {
                writer.WriteElementString("fanart", GetImagePathToSave(backdrop));
            }

            writer.WriteEndElement();
        }

        private void AddActors(List<PersonInfo> people, XmlWriter writer, ILibraryManager libraryManager, bool saveImagePath)
        {
            foreach (var person in people)
            {
                if (IsPersonType(person, PersonType.Director) || IsPersonType(person, PersonType.Writer))
                {
                    continue;
                }

                writer.WriteStartElement("actor");

                if (!string.IsNullOrWhiteSpace(person.Name))
                {
                    writer.WriteElementString("name", person.Name);
                }

                if (!string.IsNullOrWhiteSpace(person.Role))
                {
                    writer.WriteElementString("role", person.Role);
                }

                if (!string.IsNullOrWhiteSpace(person.Type))
                {
                    writer.WriteElementString("type", person.Type);
                }

                if (person.SortOrder.HasValue)
                {
                    writer.WriteElementString(
                        "sortorder",
                        person.SortOrder.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (saveImagePath)
                {
                    var personEntity = libraryManager.GetPerson(person.Name);
                    var image = personEntity.GetImageInfo(ImageType.Primary, 0);

                    if (image != null)
                    {
                        writer.WriteElementString(
                            "thumb",
                            GetImagePathToSave(image, libraryManager));
                    }
                }

                writer.WriteEndElement();
            }
        }

        private string GetImagePathToSave(ItemImageInfo image)
        {
             return image.Path;
        }

        private bool IsPersonType(PersonInfo person, string type)
            => string.Equals(person.Type, type, StringComparison.OrdinalIgnoreCase)
                || string.Equals(person.Role, type, StringComparison.OrdinalIgnoreCase);

        private void AddCustomTags(string path, IReadOnlyCollection<string> xmlTagsUsed, XmlWriter writer, ILogger<BaseNfoSaver> logger)
        {
            var settings = new XmlReaderSettings()
            {
                ValidationType = ValidationType.None,
                CheckCharacters = false,
                IgnoreProcessingInstructions = true,
                IgnoreComments = true
            };

            using (var fileStream = File.OpenRead(path))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            using (var reader = XmlReader.Create(streamReader, settings))
            {
                try
                {
                    reader.MoveToContent();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error reading existing xml tags from {Path}.", path);
                    return;
                }

                reader.Read();

                // Loop through each element
                while (!reader.EOF && reader.ReadState == ReadState.Interactive)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        var name = reader.Name;

                        if (!_commonTags.Contains(name)
                            && !xmlTagsUsed.Contains(name, StringComparison.OrdinalIgnoreCase))
                        {
                            writer.WriteNode(reader, false);
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        private string GetTagForProviderKey(string providerKey)
            => providerKey.ToLowerInvariant() + "id";
    }
}
