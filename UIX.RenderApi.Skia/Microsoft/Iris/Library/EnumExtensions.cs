using Microsoft.Iris.Render;
using Microsoft.Iris.Render.Common;

namespace Microsoft.Iris.Library
{
    public static class EnumExtensions
    {
        public static bool FulfillsRequirement(this GraphicsDeviceType cur, GraphicsDeviceType requirement)
        {
            Debug2.Assert(requirement != GraphicsDeviceType.Unknown, nameof(requirement));

            if (cur == requirement || requirement == GraphicsDeviceType.None)
                return true;
            else if (cur == GraphicsDeviceType.Unknown)
                return false;

            if (requirement == GraphicsDeviceType.Skia)
                return ((int)cur >> 4) == 1;

            return false;
        }
    }
}
