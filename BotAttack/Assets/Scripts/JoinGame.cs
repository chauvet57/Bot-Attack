using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;


public class JoinGame : MonoBehaviour
{
    private NetworkManager networkManager;

    List<GameObject> roomList = new List<GameObject>();

    [SerializeField]
    private GameObject roomListItemPrefab;

    [SerializeField]
    private Transform roomListParent;

    [SerializeField]
    private Text statut;
    
    void Start()
    {
        networkManager = NetworkManager.singleton;
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        RefreshRoomList();
    } 

    public void RefreshRoomList()
    {
        ClearRoomList();

        networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        statut.text = "Chargement ...";
    }

    public void OnMatchList(bool success, string extentedInfo, List<MatchInfoSnapshot> matchList)
    {
        statut.text = "";
        if(matchList == null)
        {
            statut.text = "Problème de lors de la récupérationde la liste des serveurs ";
            return;
        }

        foreach(MatchInfoSnapshot match in matchList)
        {
            GameObject _roomListItemGo = Instantiate(roomListItemPrefab);
            _roomListItemGo.transform.SetParent(roomListParent); 

            RoomListItems _roomListItems = _roomListItemGo.GetComponent<RoomListItems>();
            if (_roomListItems != null)
            {
                _roomListItems.Setup(match, JoinRoom);
            }

            roomList.Add(_roomListItemGo);
        }

        if(roomList.Count == 0)
        {
            statut.text = "Aucun serveur en ligne actuellement";
        }

    }

    private void ClearRoomList()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Destroy(roomList[i]);
        }
        roomList.Clear();
    }

    public void JoinRoom(MatchInfoSnapshot _match)
    {
        networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        ClearRoomList();
        statut.text = "Connexion ...";
    }
}
