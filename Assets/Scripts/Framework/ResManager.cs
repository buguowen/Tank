using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public static class ResManager
    {
        public static GameObject LoadPrefab(string path)
        {
            return Resources.Load<GameObject>(path);
        }
    }
}