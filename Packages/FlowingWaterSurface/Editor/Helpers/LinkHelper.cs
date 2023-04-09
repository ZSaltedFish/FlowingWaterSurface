using System.Collections.Generic;
using UnityEngine;

namespace com.ZKnight.FlowingWaterSurface.Editor
{
    public static class LinkHelper
    {
        private static bool TryList(float t, int index, List<Vector3> list, out Vector3 result)
        {
            result = Vector3.zero;
            switch (list.Count)
            {
                case 0: break;
                case 1: result = list[0]; break;
                case 2: result = Vector3.Lerp(list[0], list[1], t); break;
                case 3: result = Vector3.Lerp(list[index], list[index + 1], t); break;
                default: return true;
            }
            return false;
        }

        public static Vector3 GetCatmullRom(float t, int index, List<Vector3> list)
        {
            var result = Vector3.zero;
            if (!TryList(t, index, list, out result))
            {
                return result;
            }



            return result;
        }
    }
}
