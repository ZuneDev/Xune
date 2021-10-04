// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.OS.FileResource
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using Microsoft.Iris.Data;
using System;
using System.IO;

namespace Microsoft.Iris.OS
{
    internal class FileResource : Resource
    {
        private string _filePath;
        private IntPtr _handle;
        private NativeApi.DownloadCompleteHandler _pendingCallback;

        public FileResource(string uri, string filePath, bool forceSynchronous)
          : base(uri, forceSynchronous)
          => _filePath = filePath;

        public override string Identifier => _filePath;

        protected override void StartAcquisition(bool forceSynchronous)
        {
            if (forceSynchronous)
                SynchronousDownload();
            else
                AsynchronousDownload();
        }

        private void AsynchronousDownload()
        {
            _pendingCallback = new NativeApi.DownloadCompleteHandler(OnFileDownloadComplete);
            int num = (int)NativeApi.SpFileDownload(_filePath, _pendingCallback, IntPtr.Zero, out _handle);
        }

        private unsafe void OnFileDownloadComplete(IntPtr handle, int error, uint length, IntPtr context)
        {
            byte[] buffer = null;
            string errorDetails = null;
            if (error == 0)
                buffer = new Span<byte>(NativeApi.DownloadGetBuffer(_handle).ToPointer(), (int)length).ToArray();
            else
                errorDetails = string.Format("Failed to complete download from '{0}'", _filePath);
            int num = (int)NativeApi.SpDownloadClose(_handle);
            _handle = IntPtr.Zero;
            _pendingCallback = null;
            NotifyAcquisitionComplete(buffer, true, errorDetails);
        }

        private void SynchronousDownload()
        {
            MemoryStream outStream = new MemoryStream();
            long fileSize = 0;
            string errorDetails = null;
            FileInfo file = new(_filePath);

            if (!file.Exists)
            {
                errorDetails = string.Format("File not found: '{0}'", _filePath);
            }
            else
            {
                using FileStream fstream = file.OpenRead();
                fstream.CopyTo(outStream);
            }
            byte[] bytes = outStream.ToArray();
            NotifyAcquisitionComplete(bytes, true, errorDetails);
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
