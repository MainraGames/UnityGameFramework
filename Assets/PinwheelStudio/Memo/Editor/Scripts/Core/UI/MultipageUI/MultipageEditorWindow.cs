using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Memo.UI;
using System;

namespace Pinwheel.Memo.UI
{
    public class MultipageEditorWindow : EditorWindow, IMultipageWindow
    {
        public EditorWindow window => this;
        public int pageCount => m_pageStack.Count;
        protected Stack<Page> m_pageStack = new Stack<Page>();

        public void PushPage(Page page)
        {
            if (page ==null)
                throw new ArgumentNullException("page");
            if (this != (page.hostWindow as EditorWindow))
                throw new ArgumentException("Invalid page host window");
            m_pageStack.Push(page);
            page.OnPushed();
        }

        public void PopPage()
        {
            Page page = m_pageStack.Pop();
            page.OnPopped();
        }

        protected virtual void OnFocus()
        {
            Page currentPage = null;
            if (m_pageStack.TryPeek(out currentPage))
            {
                currentPage.OnFocus();
            }
        }
    }
}
