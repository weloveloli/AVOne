// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Shared
{
    using Microsoft.JSInterop;

    public abstract class ProCompontentBase : ComponentBase
    {
        private I18n? _languageProvider;

        [Inject]
        public IJSRuntime JS { get; set; }

        [CascadingParameter]
        public I18n LanguageProvider
        {
            get
            {
                return _languageProvider ?? throw new Exception("please Inject I18n!");
            }
            set
            {
                _languageProvider = value;
            }
        }

        public string T(string key)
        {
            return LanguageProvider.T(key) ?? key;
        }
        public string T(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }

        public void Success(string message, params object[] args)
        {
            var msg = T(message, args);
            JS?.InvokeVoidAsync("Qmsg.success", msg);
        }
        public void Info(string message, params object[] args)
        {
            var msg = T(message, args);
            JS?.InvokeVoidAsync("Qmsg.info", msg);
        }

        public void Error(string message, params object[] args)
        {
            var msg = T(message, args);
            JS?.InvokeVoidAsync("Qmsg.error", msg);
        }

        public void Warning(string message, params object[] args)
        {
            var msg = T(message, args);
            JS?.InvokeVoidAsync("Qmsg.warning", msg);
        }

        public void ShowLoading(string message, bool autoClose, int timeout, params object[] args)
        {
            var msg = T(message, args);
            JS?.InvokeVoidAsync("Qmsg.loading", msg, autoClose, timeout);
        }

        public void CloseLoading()
        {
            JS?.InvokeVoidAsync("Qmsg.QmsgCloseLoading");
        }
    }
}
