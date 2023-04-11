// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Pages.App.Downloads
{
    using AVOne.Impl.Data;
    using AVOne.Impl.Job;
    using AVOne.Providers;
    using AVOne.Server.Shared;
    using System.Linq.Expressions;
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
        public string? Status { get; set; }

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
                    Interval = 1000
                };
                Timer.Elapsed += OnTimerCallback;
                Timer.Start();
            }
        }

        protected override void OnParametersSet()
        {
            this.UpdateJobs();
        }

        private async void OnTimerCallback(object? sender, ElapsedEventArgs e)
        {
            await InvokeAsync(() =>
            {
                UpdateJobs();
                base.StateHasChanged();
            });
        }

        private void UpdateJobs()
        {
            Expression<Func<JobModel, bool>> statusPrecidate = (e) => true;
            if (Status == "downloading")
            {
                statusPrecidate = (e) => e.Status == (int)JobStatus.Pending || e.Status == (int)JobStatus.Running || e.Status == (int)JobStatus.Failed;
            }
            else if (Status == "completed")
            {
                statusPrecidate = (e) => e.Status == (int)JobStatus.Completed;
            }
            else
            {
                statusPrecidate = (e) => e.Status == (int)JobStatus.Canceled;
            }
            var pageList = JobRepository.GetJobs(0, 100,
                (true, (e) => e.Type == "DownloadAVJob"),
                (true, statusPrecidate),
                (!string.IsNullOrEmpty(Tag), (e) => e.Tags.Contains(Tag)),
                (!string.IsNullOrEmpty(InputText), (e) => e.Name.Contains(InputText!)));
            var list = new List<JobModel> { };
            list.AddRange(pageList);
            this.Jobs = list;
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

        private void StopJob(JobModel removeJobModel)
        {
            JobManager.CancelJobByKey(removeJobModel.Key);
        }
        private void DeleteJob(JobModel removeJobModel)
        {
            JobManager.DeleteJob(removeJobModel.Key);
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
