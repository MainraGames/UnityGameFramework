using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Memo
{
    public static class Styles
    {
        private static GUIStyle m_h1;
        public static GUIStyle h1
        {
            get
            {
                if (m_h1 == null)
                {
                    m_h1 = new GUIStyle(EditorStyles.label);
                    m_h1.fontStyle = FontStyle.Bold;
                    m_h1.fontSize = 18;
                    m_h1.richText = true;
                    m_h1.margin = new RectOffset(0, 0, 8, 8);
                }
                return m_h1;
            }
        }

        private static GUIStyle m_h2;
        public static GUIStyle h2
        {
            get
            {
                if (m_h2 == null)
                {
                    m_h2 = new GUIStyle(EditorStyles.label);
                    m_h2.fontStyle = FontStyle.Bold;
                    m_h2.fontSize = 15;
                    m_h2.richText = true;
                    m_h2.margin = new RectOffset(0, 0, 4, 4);
                }
                return m_h2;
            }
        }

        private static GUIStyle m_h3;
        public static GUIStyle h3
        {
            get
            {
                if (m_h3 == null)
                {
                    m_h3 = new GUIStyle(EditorStyles.label);
                    m_h3.fontStyle = FontStyle.Bold;
                    m_h3.fontSize = 12;
                    m_h3.richText = true;
                }
                return m_h3;
            }
        }

        private static GUIStyle m_p1;
        public static GUIStyle p1
        {
            get
            {
                if (m_p1 == null)
                {
                    m_p1 = new GUIStyle(EditorStyles.label);
                    m_p1.alignment = TextAnchor.UpperLeft;
                    m_p1.wordWrap = true;
                    m_p1.richText = true;
                }
                return m_p1;
            }
        }

        private static GUIStyle m_p2;
        public static GUIStyle p2
        {
            get
            {
                if (m_p2 == null)
                {
                    m_p2 = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                    m_p2.alignment = TextAnchor.UpperLeft;
                    m_p2.wordWrap = true;
                    m_p2.richText = true;
                }
                return m_p2;
            }
        }

        private static GUIStyle m_headerContainer;
        public static GUIStyle headerContainer
        {
            get
            {
                if (m_headerContainer == null)
                {
                    m_headerContainer = new GUIStyle(EditorStyles.toolbar);
                }
                return m_headerContainer;
            }
        }
    }
}