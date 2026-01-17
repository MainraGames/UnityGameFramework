using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEditor;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class VersionInfo
    {
        public static VersionInfo current = new VersionInfo()
        {
            major = 1,
            minor = 0,
            patch = 0
        };

        public int major;
        public int minor;
        public int patch;
    }
}
