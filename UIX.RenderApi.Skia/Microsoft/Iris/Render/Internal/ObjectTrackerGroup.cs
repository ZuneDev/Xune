// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Internal.ObjectTrackerGroup
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

namespace Microsoft.Iris.Render.Internal
{
  internal static class ObjectTrackerGroup
  {
    private static ObjectTracker s_objectTrackerGroup = new ObjectTracker((RenderSession) null, ObjectTracker.ThreadMode.Master, (object) null);

    internal static void RegisterChildTracker(ObjectTracker trackerToAdd) => ObjectTrackerGroup.s_objectTrackerGroup.AddObject((object) trackerToAdd);
  }
}
