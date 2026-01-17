using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Memo.UI
{
    public interface IMultipageWindow
    {
        EditorWindow window { get; }
        int pageCount { get; }
        void PushPage(Page page);
        void PopPage();
    }

    public static class MultipageWindowUtilities
    {
        public static void PopAllPages(this IMultipageWindow window)
        {
            while (window.pageCount > 0)
            {
                window.PopPage();
            }
        }
    }
}
