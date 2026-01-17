using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class Link
    {
        [SerializeField]
        protected string m_url;
        public string url
        {
            get
            {
                return m_url;
            }
            set
            {
                m_url = value;
            }
        }

        [SerializeField]
        protected string m_displayText;
        public string displayText
        {
            get
            {
                return m_displayText;
            }
            set
            {
                m_displayText = value;
            }
        }

        [SerializeField]
        protected string m_idRemote;
        public string idRemote
        {
            get
            {
                return m_idRemote;
            }
            set
            {
                m_idRemote = value;
            }
        }

        [SerializeField]
        protected bool m_isDeleted;
        public bool isDeleted
        {
            get
            {
                return m_isDeleted;
            }
            set
            {
                m_isDeleted = value;
            }
        }

        public Link()
        {

        }

        public Link(string url, string displayText = null)
        {
            m_url = url;
            m_displayText = displayText;
        }
    }
}
