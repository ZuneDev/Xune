﻿// Decompiled with JetBrains decompiler
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
    internal static class TraceSettings
    {
        private static Dictionary<TraceCategory, byte> CategoryLevels = new();

        private static string s_debugTraceFile;

        public static void ListenForRegistryUpdates()
        {
        }

        public static void StopListeningForRegistryUpdates()
        {
        }

        public static void Refresh()
        {
            s_debugTraceFile = Environment.GetEnvironmentVariable("SPLASH_TRACE_FILE");
            WriteLinePrefix = string.Empty;
            SendOutputToDebugger = true;
            ShowCategories = false;
            TimedWriteLines = false;
        }

        public static byte GetCategoryLevel(TraceCategory cat)
        {
            if (CategoryLevels.TryGetValue(cat, out byte level))
            {
                return level;
            }
            else
            {
                CategoryLevels.Add(cat, 0);
                return GetCategoryLevel(cat);
            }
        }

        public static void SetCategoryLevel(TraceCategory cat, byte level)
        {
            if (CategoryLevels.ContainsKey(cat))
            {
                CategoryLevels[cat] = level;
            }
            else
            {
                CategoryLevels.Add(cat, level);
            }
        }

        public static bool IsFlagsCategory(TraceCategory cat) => false;

        private static bool IsExternalCategory(TraceCategory cat) => (uint)cat < 25U;

        public static bool SendOutputToDebugger { get; set; } = true;

        public static bool TimedWriteLines { get; set; } = false;

        public static bool ShowCategories { get; set; } = false;

        public static bool AlwaysShowBraces { get; set; } = false;

        public static string WriteLinePrefix { get; set; } = "";

        public static string RendererWriteLinePrefix { get; set; } = "";

        public static bool DebugTraceToFile => !string.IsNullOrEmpty(s_debugTraceFile);

        public static string DebugTraceFile => s_debugTraceFile;
    }
}
