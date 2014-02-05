using UnityEngine;
using System.Collections;

namespace JC
{
    public class LightMapAsset : ScriptableObject
    {
        public Texture2D[] lightmapFar = null;

        public Texture2D[] lightmapNear = null;
    }
}
