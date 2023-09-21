// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Shared
{
    using Microsoft.JSInterop;

    public abstract class ProComponentBase : ComponentBase
    {

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        [Inject]
        protected I18n I18n { get; set; } = null!;

        [CascadingParameter(Name = "CultureName")]
        protected string? Culture { get; set; }

        protected string T(string? key, params object[] args)
        {
            return I18n.T(key, args: args);
        }

        public void Success(string message, params object[] args)
        {
            var msg = T(message, args);
            _ = (JS?.InvokeVoidAsync("Qmsg.success", msg));
        }
        public void Info(string message, params object[] args)
        {
            var msg = T(message, args);
            _ = (JS?.InvokeVoidAsync("Qmsg.info", msg));
        }

        public void Error(string message, params object[] args)
        {
            var msg = T(message, args);
            _ = (JS?.InvokeVoidAsync("Qmsg.error", msg));
        }

        public void Warning(string message, params object[] args)
        {
            var msg = T(message, args);
            _ = (JS?.InvokeVoidAsync("Qmsg.warning", msg));
        }

        public void ShowLoading(string message, bool autoClose, int? timeout, params object[] args)
        {
            var msg = T(message, args);
            _ = (JS?.InvokeVoidAsync("QmsgShowLoading", msg, autoClose, timeout));
        }

        public void CloseLoading()
        {
            _ = (JS?.InvokeVoidAsync("QmsgCloseLoading"));
        }
    }
}
