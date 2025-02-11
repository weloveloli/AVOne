// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Global
{
    using Microsoft.JSInterop;

    public class CookieStorage
    {
        private readonly IJSRuntime _jsRuntime;

        public CookieStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetAsync(string key)
        {
            return await _jsRuntime.InvokeAsync<string>(JsInteropConstants.GetCookie, key);
        }

        public async void SetAsync<T>(string key, T? value)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(JsInteropConstants.SetCookie, key, value?.ToString());
            }
            catch
            {
                // ignored
            }
        }
    }
}
