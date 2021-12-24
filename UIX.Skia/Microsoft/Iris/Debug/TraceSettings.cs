// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Debug.TraceSettings
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using Microsoft.Iris.OS;
using System;
using System.Collections.Generic;
using System.Security;

namespace Microsoft.Iris.Debug
{
    [SuppressUnmanagedCodeSecurity]
    public class TraceSettings
    {
        private static TraceSettings s_instance;
        private Dictionary<TraceCategory, byte> CategoryLevels = new Dictionary<TraceCategory, byte>();

        public TraceSettings()
        {
            if (s_instance != null)
                throw new InvalidOperationException("TraceSettings was already initialized. Use TraceSettings.Current.");
            s_instance = this;
            
            if (DebugTraceToFile && System.IO.File.Exists(DebugTraceFile))
                System.IO.File.Delete(DebugTraceFile);
        }

        public static TraceSettings Current
        {
            get
            {
                if (s_instance == null)
                    new TraceSettings();
                return s_instance;
            }
        }

        public void ListenForRegistryUpdates()
        {
        }

        public void StopListeningForRegistryUpdates()
        {
        }

        public byte GetCategoryLevel(TraceCategory cat)
        {
            if (CategoryLevels.TryGetValue(cat, out byte level))
                return level;
            else
                return 0;
        }

        public void SetCategoryLevel(TraceCategory cat, byte level)
        {
            if (CategoryLevels.ContainsKey(cat))
                CategoryLevels[cat] = level;
            else
                CategoryLevels.Add(cat, level);
        }

        public bool IsFlagsCategory(TraceCategory cat) => false;

        private bool IsExternalCategory(TraceCategory cat) => (uint)cat < 25U;

        public bool SendOutputToDebugger { get; set; } = true;

        public bool TimedWriteLines { get; set; } = false;

        public bool ShowCategories { get; set; } = false;

        public bool AlwaysShowBraces { get; set; }

        public string WriteLinePrefix { get; set; } = string.Empty;

        public string RendererWriteLinePrefix { get; set; }

        public bool DebugTraceToFile => !string.IsNullOrEmpty(DebugTraceFile);

        public string DebugTraceFile { get; set; }
    }
}
