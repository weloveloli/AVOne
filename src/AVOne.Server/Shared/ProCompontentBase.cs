// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Shared
{
    public abstract class ProCompontentBase : ComponentBase
    {
        private I18n? _languageProvider;
        private bool _snackbar;
        private string? message;

        public void ShowSnackbar(string msg)
        {
            message = msg;
            _snackbar = true;
        }
        public void ShowSnackbarLocal(string msgKey, params object[] args)
        {
            message = string.Format(T(msgKey), args);
            _snackbar = true;
        }

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
    }
}
