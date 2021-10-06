// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.OS.HttpResource
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using Microsoft.Iris.Data;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Microsoft.Iris.OS
{
    internal class HttpResource : Resource
    {
        private IntPtr _handle;
        private NativeApi.DownloadCompleteHandler _pendingCallback;

        public HttpResource(string uri, bool forceSynchronous)
          : base(uri, forceSynchronous)
        {
        }

        public override string Identifier => _uri;

        protected override async void StartAcquisition(bool forceSynchronous)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            HttpResources.Client.GetAsync(_uri).ContinueWith(resp => OnHttpDownloadComplete(resp.Result));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private unsafe void OnHttpDownloadComplete(HttpResponseMessage resp)
        {
            byte[] buffer = null;
            string errorDetails = null;
            int error = resp.IsSuccessStatusCode ? 0 : 3;
            switch (error)
            {
                case 0:
                    buffer = resp.Content.ReadAsByteArrayAsync().Result;
                    break;
                case 1:
                    errorDetails = string.Format("Invalid URI: '{0}'", _uri);
                    break;
                case 2:
                    errorDetails = string.Format("Unable to connect to web host: '{0}'", _uri);
                    break;
                default:
                    errorDetails = string.Format("Failed to complete download from '{0}'", _uri);
                    break;
            }
            _handle = IntPtr.Zero;
            _pendingCallback = null;
            NotifyAcquisitionComplete(buffer, false, errorDetails);
        }

        protected override void CancelAcquisition()
        {
            if (!(_handle != IntPtr.Zero))
                return;
            int num = (int)NativeApi.SpDownloadClose(_handle);
            _handle = IntPtr.Zero;
            _pendingCallback = null;
        }
    }
}
