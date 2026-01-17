using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Pinwheel.Memo.Trello
{
    [System.Serializable]
    public class TrelloUser : TrelloObject
    {
        [SerializeField]
        protected string m_fullName;
        public string fullName
        {
            get
            {
                return m_fullName;
            }
            internal set
            {
                m_fullName = value;
            }
        }

        [SerializeField]
        protected string m_initials;
        public string initials
        {
            get
            {
                return m_initials;
            }
            internal set
            {
                m_initials = value;
            }
        }

        [System.NonSerialized]
        protected bool m_isGettingUserInfo;
        public bool isGettingUserInfo
        {
            get
            {
                return m_isGettingUserInfo;
            }
            protected set
            {
                m_isGettingUserInfo = value;
            }
        }

        public static TrelloMe me => TrelloApi.settings.me;

        public TrelloUser(string userId):base(userId)
        {
            name = null;
            fullName = null;
            initials = null;
        }

        public void UpdateInfoLazy()
        {
            if (isGettingUserInfo)
            {
                Debug.Log("Getting user info in progress, skipping this request.");
            }
            else
            {
                isGettingUserInfo = true;
                TrelloApi.GetUser(id, OnGetUserCompleted);
            }
        }

        protected void OnGetUserCompleted(UnityWebRequest request, TrelloApi.UserDescription response)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                OnGetUserSucceeded(response);
            }
            else
            {
                Debug.Log(request.error);
            }
            request.Dispose();
            isGettingUserInfo = false;
            syncCount += 1;
        }

        protected virtual void OnGetUserSucceeded(TrelloApi.UserDescription response)
        {
            id = response.id;
            name = response.username;
            fullName = response.fullName;
            initials = response.initials;
        }
    }
}
