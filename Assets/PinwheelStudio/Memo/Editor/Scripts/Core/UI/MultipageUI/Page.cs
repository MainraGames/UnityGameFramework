using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace Pinwheel.Memo.UI
{
    public class Page
    {
        public IMultipageWindow hostWindow { get; protected set; }

        public Page(IMultipageWindow window)
        {
            hostWindow = window;
        }

        public virtual void OnPushed()
        {

        }

        public virtual void OnPopped()
        {

        }

        public virtual void DrawHeader()
        {
            
        }
        
        public virtual void DrawBody() 
        {
        }        

        public virtual void OnFocus()
        {

        }
    }
}
