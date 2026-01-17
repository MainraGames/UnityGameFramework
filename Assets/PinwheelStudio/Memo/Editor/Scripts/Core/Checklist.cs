using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class Checklist
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

        [SerializeField]
        protected List<Item> m_items = new List<Item>();
        public List<Item> items
        {
            get
            {
                return m_items;
            }
        }

        public Checklist()
        {

        }

        [System.Serializable]
        public class Item
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
            protected bool m_isChecked;
            public bool isChecked
            {
                get
                {
                    return m_isChecked;
                }
                set
                {
                    m_isChecked = value;
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

            public Item()
            {
                m_name = null;
                m_isChecked = false;
            }
        }
    }
}
