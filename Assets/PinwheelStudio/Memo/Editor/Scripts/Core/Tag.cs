using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class Tag
    {
        [SerializeField]
        protected string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        [SerializeField]
        protected Colors m_color;
        public Colors color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        [SerializeField]
        protected string m_id;
        public string id
        {
            get
            {
                return m_id;
            }
            internal set
            {
                m_id = value;
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

        internal Tag()
        {
            m_id = Utilities.NewId();
            m_color = Colors.Red;
            m_name = string.Empty;
        }
    }
}
