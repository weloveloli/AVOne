// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
namespace AVOne.Server.Pages.App.Downloads.Components
{
    using AVOne.Configuration;
    using AVOne.Impl.Job;
    using AVOne.Models.Download;
    using AVOne.Server.Data.Base;

    public partial class TaskNav : ProComponentBase
    {
        [Inject]
        private NavigationManager? Navigation { get; set; }

        [Inject]
        private IConfigurationManager? ConfigurationManager { get; set; }

        [Inject]
        public IProviderManager? ProviderManager { get; set; }

        [Inject]
        public IJobManager? JobManager { get; set; }

        private ICacheMetaDataProvider<PornMovie>? cacheMetaDataProvider;

        private List<string> DownloadTags = new List<string>();

        bool addTaskDialog;
        bool showAdvancedDownloadOpt;

        bool searchMetaData;

        List<string> MetaDataProviders = new List<string>();
        Dictionary<string, IRemoteMetadataSearchProvider<PornMovieInfo>> MetaDataProvidersMap = new Dictionary<string, IRemoteMetadataSearchProvider<PornMovieInfo>>();

        static int? defaultThreadCount;
        static int? defaultRetryCount;
        static string? defaultOutputDir;

        class AddJobModel
        {
            [Required]
            [Url]
            public string? Url { get; set; }

            [Range(1, 16)]
            public int? ThreadCount { get; set; } = defaultThreadCount;

            [Range(1, 8)]
            public int? RetryCount { get; set; } = defaultRetryCount;

            public string? DownloadDir { get; set; } = defaultOutputDir;

            public List<string> Tags { get; set; } = new List<string>();

            public int Step { get; set; } = 0;

            public string? DownloadItemKey { get; set; }

            public string? PreferName { get; set; }

            public bool SaveMetaData { get; set; } = false;

            public bool UseInternelMetaData { get; set; } = false;

            public bool AllowInternelMetaData { get; set; } = false;

            public string MetaDataSearchName { get; set; } = string.Empty;

            public string MetaDataProvider { get; set; } = string.Empty;

            public Models.Result.RemoteMetadataSearchResult? SearchMetaDataResult { get; set; }
        }
        private List<SelectData> _tagList = new List<SelectData>();

        private List<BaseDownloadableItem> DownloadableItems { get; set; } = new List<BaseDownloadableItem>();
        private void HandleCloseClick(string lable)
        {
            _addJobModel.Tags.Remove(lable);
        }
        protected override void OnInitialized()
        {
            defaultThreadCount = ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultDownloadThreadCount;
            defaultRetryCount = ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultRetryCount;
            defaultOutputDir = ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultDownloadDir;
            _addJobModel = new();
            _tagList = new List<SelectData>{
            new SelectData() { Label = T("Download.Tags.Censored"), Value = "Censored" },
            new SelectData() { Label = T("Download.Tags.Uncensored"), Value = "Uncensored" },
            new SelectData() { Label = T("Download.Tags.Chinese"), Value = "Chinese" },
            new SelectData() { Label = T("Download.Tags.ChineseSub"), Value = "ChineseSub" },
            new SelectData() { Label = T("Download.Tags.Taiwanese"), Value = "Taiwanese" },
            new SelectData() { Label = T("Download.Tags.Japanese"), Value = "Japanese" },
            new SelectData() { Label = T("Download.Tags.Anime"), Value = "Anime" },
            new SelectData() { Label = T("Download.Tags.US"), Value = "US" },
            new SelectData() { Label = T("Download.Tags.Other"), Value = "Other" },
        };
            MetaDataProviders = ProviderManager?.GetMetadataProviders<PornMovie>(null).OfType<IRemoteMetadataSearchProvider<PornMovieInfo>>()?.Select(e => e.Name).ToList() ?? new List<string>();
            MetaDataProvidersMap = ProviderManager?.GetMetadataProviders<PornMovie>(null).OfType<IRemoteMetadataSearchProvider<PornMovieInfo>>()?.ToDictionary(e => e.Name) ?? new Dictionary<string, IRemoteMetadataSearchProvider<PornMovieInfo>>();
            cacheMetaDataProvider = ProviderManager?.GetMetadataProviders<PornMovie>(null).OfType<ICacheMetaDataProvider<PornMovie>>().FirstOrDefault();
            DownloadTags = typeof(DownloadItemTags).GetAllPublicConstantValues<string>();
        }

        private AddJobModel _addJobModel = new();

        private List<Models.Result.RemoteMetadataSearchResult> _searchResults = new List<Models.Result.RemoteMetadataSearchResult>();
        private async Task HandleSearchMetaData()
        {
            await InvokeAsync(async () =>
            {
                if (string.IsNullOrEmpty(_addJobModel.MetaDataProvider))
                {
                    return;
                }
                searchMetaData = true;
                this.ShowLoading("Download.Tasks.SearchingMetaData", false, null, _addJobModel.MetaDataSearchName!);

                var provider = MetaDataProvidersMap[_addJobModel.MetaDataProvider];
                try
                {
                    var searchResults = await provider.GetSearchResults(new PornMovieInfo { Name = _addJobModel.MetaDataSearchName, Valid = false }, default);
                    _searchResults = searchResults?.ToList() ?? new List<Models.Result.RemoteMetadataSearchResult>();
                }

                catch (Exception)
                {
                    this.Error("Download.Tasks.SearchingMetaDataFailed");
                }

                finally
                {
                    this.CloseLoading();
                    searchMetaData = false;
                    StateHasChanged();
                }

            });
        }

        private void autoDetect()
        {
            // TODO: ADD Script to allow user custom detect
            var pornMovieInfo = PornMovieInfo.Parse(new PornMovie { Name = _addJobModel.PreferName! }, ConfigurationManager!.CommonConfiguration);
            if (pornMovieInfo is not null && pornMovieInfo.Valid)
            {
                var newName = pornMovieInfo.Id;
                _addJobModel.PreferName = newName;
                _addJobModel.MetaDataSearchName = newName;
                _addJobModel.DownloadDir = Path.Combine(_addJobModel.DownloadDir!, newName);
            }
        }

        private async Task HandleOnValidSubmit()
        {
            await InvokeAsync(async () =>
            {
                var step = _addJobModel.Step;
                await this.AddDownloadJob();
                StateHasChanged();
                if (step == 3)
                {
                    addTaskDialog = false;
                    showAdvancedDownloadOpt = false;
                    _addJobModel = new AddJobModel();
                }
            });
        }

        private async Task AddDownloadJob()
        {
            if (_addJobModel.Step == 0)
            {
                this.ShowLoading("Download.Tasks.ExtractingMedia", false, null, _addJobModel.Url!);
                var jobItems = ProviderManager?.GetMediaExtractorProviders(_addJobModel.Url!);
                var extractorProvider = jobItems?.FirstOrDefault();
                if (extractorProvider is null)
                {
                    this.CloseLoading();
                    var failedReason = T("Download.Tasks.FailedReason.NoExtrator", _addJobModel.Url!);
                    this.Error("Download.Tasks.Message.AddTaskFailed", _addJobModel.Url!, failedReason);
                    return;
                }

                var items = await extractorProvider.ExtractAsync(_addJobModel.Url!);
                if (items is null || items.Count() == 0)
                {
                    this.CloseLoading();
                    var failedReason = T("Download.Tasks.FailedReason.GetDownloadItemFailed", _addJobModel.Url!);
                    this.Error("Download.Tasks.Message.AddTaskFailed", _addJobModel.Url!, failedReason);
                    return;
                }
                this.CloseLoading();
                DownloadableItems = items.ToList();
                _addJobModel.DownloadItemKey = DownloadableItems.FirstOrDefault()?.Key;
                _addJobModel.Step = 1;

            }

            else if (_addJobModel.Step == 1)
            {
                var jobItem = DownloadableItems.FirstOrDefault(x => x.Key == _addJobModel.DownloadItemKey);
                if (jobItem is null)
                {
                    var failedReason = T("Download.Tasks.FailedReason.GetDownloadItemFailed", _addJobModel.Url!);
                    this.Error("Download.Tasks.Message.AddTaskFailed", _addJobModel.Url!, failedReason);
                    return;
                }
                if (jobItem.HasMetaData && cacheMetaDataProvider is not null)
                {
                    _addJobModel.AllowInternelMetaData = true;
                    _addJobModel.UseInternelMetaData = true;
                }

                if (jobItem.DownloadTags.IsNotEmpty())
                {
                    _addJobModel.Tags = jobItem.DownloadTags.Intersect(this.DownloadTags).ToList();
                }

                _addJobModel.PreferName = jobItem.SaveName;
                _addJobModel.MetaDataSearchName = jobItem.SaveName;
                _addJobModel.MetaDataProvider = MetaDataProviders.FirstOrDefault() ?? string.Empty;
                _addJobModel.Step = 2;
            }
            else if (_addJobModel.Step == 2)
            {
                var downloadDir = _addJobModel.DownloadDir ?? ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultDownloadDir;
                if (!Path.Exists(downloadDir))
                {
                    try
                    {
                        Directory.CreateDirectory(downloadDir!);
                    }
                    catch (Exception)
                    {
                        this.Error("Download.Tasks.FailedReason.CannotCreateDir", downloadDir);
                        return;
                    }
                }

                var jobItem = DownloadableItems.FirstOrDefault(x => x.Key == _addJobModel.DownloadItemKey);
                var downloadJob = new DownloadAVJob();
                if (_addJobModel.SaveMetaData)
                {
                    if (_addJobModel.UseInternelMetaData && jobItem!.HasMetaData)
                    {
                        this.cacheMetaDataProvider!.StoreCache(jobItem.Key, jobItem);
                        downloadJob.MetaDataProviderId = jobItem.Key;
                        downloadJob.MetaDataProviderName = this.cacheMetaDataProvider!.Name;
                    }
                    else if (_addJobModel.SearchMetaDataResult != null)
                    {
                        var result = _addJobModel.SearchMetaDataResult;
                        downloadJob.MetaDataProviderId = result.GetPid(result.SearchProviderName).ToString();
                        downloadJob.MetaDataProviderName = result.SearchProviderName;
                    }
                }

                downloadJob.Name = jobItem!.DisplayName;
                var perferName = _addJobModel.PreferName;

                var threadCount = _addJobModel.ThreadCount ?? ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultDownloadThreadCount;
                var retryCount = _addJobModel.ThreadCount ?? ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultRetryCount;
                var retryWait = _addJobModel.ThreadCount ?? ConfigurationManager?.CommonConfiguration.DownloadConfig.DefaultRetryWait;
                var opt = new DownloadOpts { ThreadCount = threadCount, OutputDir = downloadDir, RetryCount = retryCount, RetryWait = retryWait, PreferName = perferName };
                downloadJob.DownloadableItem = jobItem;
                downloadJob.DownloadOpts = opt;
                downloadJob.Tags = _addJobModel.Tags;

                JobManager?.AddJob(downloadJob);
                await JobManager!.EnqueueJob(downloadJob);
                Navigation?.NavigateTo(GlobalVariables.DefaultRoute, true);
            }
        }
    }
}
