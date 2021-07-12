// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.VideoStream
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Graphics;
using Microsoft.Iris.Render.Internal;
using Microsoft.Iris.Render.Protocols.Splash.Rendering;
using System;
using System.Threading;

namespace Microsoft.Iris.Render
{
  internal sealed class VideoStream : 
    SharedResource,
    IVideoPoolNotification,
    IVideoStream,
    ISharedRenderObject,
    ISurfaceContentOwner
  {
    private static int s_idxNextUnique = 100;
    private GraphicsDevice m_device;
    private Surface m_surfaceScene;
    private VideoPool m_poolScene;
    private int m_nUniqueID;
    private Size m_sizeTargetPxl;
    private Size m_sizeAspectRatio;

    public VideoStream(RenderSession session, GraphicsDevice device)
      : base(session)
    {
      Debug2.Validate(device != null, typeof (ArgumentNullException), "must pass a valid device");
      this.m_device = device;
      this.Initialize();
    }

    private void Initialize()
    {
      this.m_nUniqueID = Interlocked.Increment(ref VideoStream.s_idxNextUnique);
      this.m_poolScene = new VideoPool(this.m_device, (IVideoPoolNotification) this);
      this.m_surfaceScene = new Surface((SurfacePool) this.m_poolScene, (ISurfaceContentOwner) this, Size.Zero);
      this.m_surfaceScene.RegisterUsage((object) this);
      IEffectTemplate effectTemplate = this.m_session.CreateEffectTemplate((object) this, "EmptyVideo");
      ColorElement colorElement = new ColorElement("ColorFill", ColorF.FromArgb((int) byte.MaxValue, 0, 0, 0));
      effectTemplate.Build((EffectInput) colorElement);
      IEffect instance = effectTemplate.CreateInstance((object) this);
      this.m_poolScene.Effect = (Effect) instance;
      instance.UnregisterUsage((object) this);
      effectTemplate.UnregisterUsage((object) this);
      RemoteDynamicSurfaceFactory.SendCreateVideoInstance(this.m_session.RenderingProtocol, this.m_nUniqueID, this.m_session.RenderingPort.MessagingProtocol.Context_ClassHandle, RemoteDevice.CreateFromHandle(this.m_device.RemoteDevice.Port, this.m_device.RemoteDevice.RenderHandle), this.m_surfaceScene.RemoteStub, this.m_poolScene.RemoteVideoStub);
    }

    protected override void Dispose(bool fInDispose)
    {
      if (fInDispose)
      {
        if (this.m_poolScene != null)
        {
          this.m_surfaceScene.UnregisterUsage((object) this);
          this.m_surfaceScene = (Surface) null;
          this.m_poolScene.Dispose();
          this.m_poolScene = (VideoPool) null;
        }
        RemoteDynamicSurfaceFactory.SendCloseInstance(this.m_session.RenderingProtocol, this.m_nUniqueID);
      }
      this.m_device = (GraphicsDevice) null;
      base.Dispose(fInDispose);
    }

    public void InvalidateClients()
    {
      if (this.InvalidateContentEvent == null)
        return;
      this.InvalidateContentEvent();
    }

    public void NotifySourceChanged(Size sizeTargetPxl, Size sizeAspectRatio)
    {
      bool flag = false;
      if (this.m_sizeTargetPxl != sizeTargetPxl)
      {
        this.m_sizeTargetPxl = sizeTargetPxl;
        flag = true;
      }
      if (this.m_sizeAspectRatio != sizeAspectRatio)
      {
        this.m_sizeAspectRatio = sizeAspectRatio;
        flag = true;
      }
      if (!flag)
        return;
      this.InvalidateClients();
    }

    public void OnSurfaceDisposed(Surface surface)
    {
      if (this.m_surfaceScene != surface)
        return;
      this.m_surfaceScene = (Surface) null;
    }

    public void OnRestoreContent(Surface surface)
    {
    }

    public int StreamID => this.m_nUniqueID;

    public float ContentOverscan
    {
      get => this.m_poolScene.ContentOverscan;
      set
      {
        Debug2.Validate((double) value >= 0.0 && (double) value <= 0.5, typeof (ArgumentException), "Valid range for content overscan is [0, .5]");
        if (Math2.WithinEpsilon(this.m_poolScene.ContentOverscan, value))
          return;
        this.m_poolScene.ContentOverscan = value;
        this.InvalidateClients();
      }
    }

    public int ContentAspectWidth => this.m_sizeAspectRatio.Width;

    public int ContentAspectHeight => this.m_sizeAspectRatio.Height;

    public int ContentHeight => this.m_sizeTargetPxl.Height;

    public int ContentWidth => this.m_sizeTargetPxl.Width;

    public event InvalidateContentHandler InvalidateContentEvent;

    internal Surface Surface => this.m_surfaceScene;

    protected override void OnUsageChange(bool fUsed) => this.InvalidateClients();
  }
}
