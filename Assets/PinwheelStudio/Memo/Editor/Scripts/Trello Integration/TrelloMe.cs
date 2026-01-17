using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.Trello;
using UnityEngine.Networking;
using System;
using UnityEditor;
using System.Linq;

namespace Pinwheel.Memo.Trello
{
    [System.Serializable]
    public class TrelloMe : TrelloUser
    {
        [SerializeField]
        protected List<TrelloApi.Organization> m_organizations = new List<TrelloApi.Organization>();
        [SerializeField]
        protected List<TrelloApi.Board> m_boards = new List<TrelloApi.Board>();
        [SerializeField]
        protected List<TrelloApi.List> m_lists = new List<TrelloApi.List>();
        [SerializeField]
        protected List<TrelloApi.Card> m_cards = new List<TrelloApi.Card>();

        public TrelloMe() : base("me")
        {
        }

        protected override void OnGetUserSucceeded(TrelloApi.UserDescription trelloUser)
        {
            base.OnGetUserSucceeded(trelloUser);
            m_organizations.Clear();
            m_boards.Clear();

            TrelloApi.GetOrganizations(trelloUser.id, (request, response) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    m_organizations.AddRange(response);
                }
                else
                {
                    Debug.LogWarning(request.error);
                }
                request.Dispose();
            });

            TrelloApi.GetBoards(trelloUser.id, (request, response) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    m_boards.AddRange(response);
                }
                else
                {
                    Debug.LogWarning(request.error);
                }
                request.Dispose();
            });
        }

        public List<TrelloApi.Organization> GetOrganizations()
        {
            return m_organizations;
        }

        public TrelloApi.Organization GetOrganizationById(string orgId)
        {
            return m_organizations.Find(o => string.Equals(o.id, orgId));
        }

        public List<TrelloApi.Board> GetOpenBoards(string orgId)
        {
            return m_boards.FindAll(b => !b.closed && string.Equals(orgId, b.idOrganization));
        }

        public TrelloApi.Board GetOpenBoardById(string boardId)
        {
            return m_boards.Find(b => !b.closed && string.Equals(b.id, boardId));
        }

        public void PullListsInBoardLazy(string boardId)
        {
            TrelloApi.GetListsInABoard(boardId, OnGetListsInBoardCompleted);
        }

        private void OnGetListsInBoardCompleted(UnityWebRequest request, TrelloApi.List[] response)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                for (int i = 0; i < response.Length; ++i)
                {
                    //Remove local list with the same id as new list, since list can be updated elsewhere (in browser, other machine)
                    //List can also be moved to other board
                    m_lists.RemoveAll(oldList => string.Equals(oldList.id, response[i].id));
                    m_lists.Add(response[i]);
                }
            }
            else
            {
                Debug.Log("Error getting lists in board: " + request.error);
            }
            request.Dispose();
        }

        public List<TrelloApi.List> GetOpenLists(string boardId, bool sortByPosition = true)
        {
            List<TrelloApi.List> result = m_lists.FindAll(l => !l.closed && string.Equals(l.idBoard, boardId));
            if (sortByPosition)
            {
                result.Sort((l0, l1) => { return l0.pos.CompareTo(l1.pos); });
            }
            return result;
        }

        public TrelloApi.List GetListById(string listId)
        {
            return m_lists.Find(l => string.Equals(l.id, listId));
        }

        public void PullCardsInBoardLazy(string boardId)
        {
            TrelloApi.GetCardsInABoard(boardId, OnGetCardsInABoardCompleted);
        }

        private void OnGetCardsInABoardCompleted(UnityWebRequest request, TrelloApi.Card[] response)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                for (int i = 0; i < response.Length; ++i)
                {
                    HandleIncomingCard(response[i]);
                }
            }
            else
            {
                Debug.Log("Error getting cards in board: " + request.error);
            }
            request.Dispose();
        }

        public List<TrelloApi.Card> GetOpenCardsInList(string listId, bool sortByPosition = true)
        {
            List<TrelloApi.Card> result = m_cards.FindAll(c => !c.closed && string.Equals(c.idList, listId));
            if (sortByPosition)
            {
                result.Sort((c0, c1) => { return c0.pos.CompareTo(c1.pos); });
            }
            return result;
        }

        public TrelloApi.Card GetCardById(string cardId)
        {
            return m_cards.Find(c => string.Equals(c.id, cardId));
        }

        public void HandleIncomingCard(TrelloApi.Card card)
        {
            //Remove old card with the same id
            //Card can be edit/move by other clients
            m_cards.RemoveAll(oldCard => string.Equals(oldCard.id, card.id));
            m_cards.Add(card);
        }

        public void SetDirty()
        {
            EditorUtility.SetDirty(TrelloApi.settings);
        }

        public void RecordUndo(string undoName)
        {
            Undo.RecordObject(TrelloApi.settings, undoName);
        }
    }
}
