// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Pages.App.Downloads
{
    using AVOne.Impl.Data;
    using AVOne.Impl.Job;
    using AVOne.Models.Download;
    using AVOne.Providers;
    using AVOne.Server.Shared;
    using System.Timers;

    public partial class Tasks : ProCompontentBase
    {
        [Inject]
        protected IJobManager JobManager { get; set; }
        [Inject]
        protected IProviderManager ProviderManager { get; set; }

        [Inject]
        protected JobRepository JobRepository { get; set; }

        public IEnumerable<JobModel> Jobs { get; set; }

        public Tasks()
        {
            Jobs = new List<JobModel>();
        }

        public int PageSize { get; set; } = 20;
        public int PageIndex { get; set; } = 1;
        public int PageCount { get; set; } = 1;

        private readonly Timer timer = new Timer(1000);

        public void OnPageIndexChanged(int index)
        {
            PageIndex = index;
            StateHasChanged();
        }

        public void OnPageSizeChanged(int size)
        {
            PageSize = size;
            StateHasChanged();
        }

        public void OnPageCountChanged(int count)
        {
            PageCount = count;
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            PageIndex = 1;
            PageSize = 10;
            PageCount = 1;
            var pageList = JobRepository.GetPagedList(PageIndex, PageSize, (true, (j) => j.Type == "DownloadAVJob"));
            this.Jobs = pageList.Items;
            timer.Elapsed += (sender, eventArgs) => OnTimerCallback();
            timer.Start();
        }
        private void OnTimerCallback()
        {
            _ = InvokeAsync(() =>
            {
                var pageList = JobRepository.GetPagedList(PageIndex, PageSize, (true, (j) => j.Type == "DownloadAVJob"));
                this.Jobs = pageList.Items;
                this.OnPageCountChanged(pageList.TotalPages);
                StateHasChanged();
            });
        }

        private async Task AddDownloadJob(AddJobModel addJobModel)
        {
            var jobItems = ProviderManager.GetMediaExtractorProviders(addJobModel.RepoUrl);
            var extractorProvider = jobItems.FirstOrDefault();
            if (extractorProvider is null)
            {
                return;
            }

            var item = await extractorProvider.ExtractAsync(addJobModel.RepoUrl);
            var jobItem = item.FirstOrDefault();
            if (jobItem is null)
            {
                return;
            }
            var opt = new DownloadOpts { ThreadCount = 4, OutputDir = "D:\\tmp", RetryCount = 1, RetryWait = 500 };
            var downloadJob = new DownloadAVJob();
            downloadJob.DownloadableItem = jobItem;
            downloadJob.DownloadOpts = opt;
            JobManager.AddJob(downloadJob);
            JobManager.ExecuteJob(downloadJob);
        }

        private void RemoveJob(JobModel removeJobModel)
        {

        }
    }
}
