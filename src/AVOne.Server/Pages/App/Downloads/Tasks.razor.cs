// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Pages.App.Downloads
{
    using AVOne.Impl.Data;
    using AVOne.Impl.Job;
    using AVOne.Providers;
    using AVOne.Server.Shared;
    using System.Timers;
    using Timer = System.Timers.Timer;

    public partial class Tasks : ProCompontentBase, IDisposable
    {
        [Inject]
        protected IJobManager JobManager { get; set; }
        [Inject]
        protected IProviderManager ProviderManager { get; set; }
        private string? _inputText;
        [Inject]
        protected JobRepository JobRepository { get; set; }
        [Inject]
        protected ILogger<Tasks> Logger { get; set; }

        private void InputTextChanged(string? text)
        {
        }

        [Parameter]
        public string? FilterText { get; set; }

        public List<JobModel> Jobs { get; set; }

        public Tasks()
        {
            Jobs = new List<JobModel>();
        }

        public int PageSize { get; set; } = 20;
        public int PageIndex { get; set; } = 1;
        public int PageCount { get; set; } = 1;

        protected Timer Timer { get; set; }

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
            if (Timer == null)
            {
                Timer = new Timer
                {
                    Interval = 2000
                };
                Timer.Elapsed += OnTimerCallback;
                Timer.Start();
            }
        }

        private async void OnTimerCallback(object? sender, ElapsedEventArgs e)
        {
            await InvokeAsync(() =>
            {
                var pageList = JobRepository.GetPagedList(PageIndex, PageSize, (true, (j) => j.Type == "DownloadAVJob"));
                var list = new List<JobModel> { };
                list.AddRange(pageList.Items);
                this.Jobs = list;
                if (Jobs.Count > 0)
                {
                    var job = Jobs.FirstOrDefault();
                    if (job is not null)
                    {
                        Logger.LogInformation($"Job {job.Id} {job.Name} {job.Progress}");
                    }
                }
                PageIndex = pageList.PageIndex;
                PageSize = pageList.PageSize;
                PageCount = pageList.TotalPages;
                base.StateHasChanged();
            });
        }

        public string? InputText
        {
            get { return _inputText; }
            set
            {
                _inputText = value;
                InputTextChanged(_inputText);
            }
        }

        private void RemoveJob(JobModel removeJobModel)
        {

        }
        private void ResetSort()
        {

        }

        private void SortbyName()
        {

        }

        private void SortbyCreatedDate()
        {

        }

        public void Dispose()
        {
            Timer?.Dispose();
        }
    }
}
