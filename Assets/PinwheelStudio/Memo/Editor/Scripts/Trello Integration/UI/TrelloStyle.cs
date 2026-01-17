using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo.Trello.UI
{
    public static class TrelloStyle
    {
        public static Color listBodyBackgroundColor => new Color32(45, 45, 45, 255);

        private static GUIStyle m_listBody;
        public static GUIStyle listBody
        {
            get
            {
                if (m_listBody == null)
                {
                    m_listBody = new GUIStyle();
                    m_listBody.margin = new RectOffset(0, 8, 0, 0);
                    m_listBody.padding = new RectOffset(8, 8, 0, 8);
                    m_listBody.clipping = TextClipping.Clip;
                }
                return m_listBody;
            }
        }

        private static GUIStyle m_listNameContainer;
        public static GUIStyle listNameContainer
        {
            get
            {
                if (m_listNameContainer == null)
                {
                    m_listNameContainer = new GUIStyle();
                    m_listNameContainer.margin = new RectOffset(0, 0, 0, 0);
                    m_listNameContainer.padding = new RectOffset(0, 0, 12, 12);
                }
                return m_listNameContainer;
            }
        }

        private static GUIStyle m_listName;
        public static GUIStyle listName
        {
            get
            {
                if (m_listName == null)
                {
                    m_listName = new GUIStyle(Styles.h3);
                    m_listName.margin = new RectOffset(0, 0, 0, 0);
                    m_listName.wordWrap = false;
                    m_listName.clipping = TextClipping.Clip;
                }
                return m_listName;
            }
        }

        public static Color cardBodyBackgroundColor => new Color32(64, 64, 64, 255);

        private static GUIStyle m_cardBody;
        public static GUIStyle cardBody
        {
            get
            {
                if (m_cardBody == null)
                {
                    m_cardBody = new GUIStyle();
                    m_cardBody.margin = new RectOffset(0, 0, 0, 8);
                    m_cardBody.padding = new RectOffset(8, 8, 8, 8);
                    m_cardBody.wordWrap = true;
                    m_cardBody.normal.background = Texture2D.whiteTexture;
                    m_cardBody.hover.background = Texture2D.redTexture;
                }
                return m_cardBody;
            }
        }

        public static Color cardHoverOutlineColor => new Color32(128, 128, 128, 255);

        private static GUIStyle m_p1Centered;
        public static GUIStyle p1Centered
        {
            get
            {
                if (m_p1Centered == null)
                {
                    m_p1Centered = new GUIStyle(Styles.p1);
                    m_p1Centered.alignment = TextAnchor.MiddleCenter;
                }
                return m_p1Centered;
            }
        }
    }
}
