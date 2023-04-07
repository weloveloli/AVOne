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
        [SupplyParameterFromQuery(Name = "Status")]
        public int? Status { get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "Tag")]
        public string? Tag { get; set; }

        public List<JobModel> Jobs { get; set; }

        public Tasks()
        {
            Jobs = new List<JobModel>();
        }

        protected Timer Timer { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Timer == null)
            {
                Timer = new Timer
                {
                    Interval = 2000
                };
                Timer.Elapsed += OnTimerCallback;
                Timer.Start();
            }
            OnTimerCallback(null, null);
        }

        protected override void OnParametersSet()
        {
            var pageList = JobRepository.GetJobs(0, 100,
                (true, (e) => e.Type == "DownloadAVJob"),
                (Status != null && Status.HasValue, (e) => e.Status == Status!.Value),
                (!string.IsNullOrEmpty(Tag), (e) => e.Tags.Contains(Tag)),
                (!string.IsNullOrEmpty(InputText), (e) => e.Name.Contains(InputText!)));
            var list = new List<JobModel> { };
            list.AddRange(pageList);
            this.Jobs = list;
        }

        private async void OnTimerCallback(object? sender, ElapsedEventArgs e)
        {
            await InvokeAsync(() =>
            {
                var pageList = JobRepository.GetJobs(0, 100,
                    (true, (e) => e.Type == "DownloadAVJob"),
                    (Status != null && Status.HasValue, (e) => e.Status == Status!.Value),
                    (!string.IsNullOrEmpty(Tag), (e) => e.Tags.Contains(Tag)),
                    (!string.IsNullOrEmpty(InputText), (e) => e.Name.Contains(InputText!)));
                var list = new List<JobModel> { };
                list.AddRange(pageList);
                this.Jobs = list;
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
