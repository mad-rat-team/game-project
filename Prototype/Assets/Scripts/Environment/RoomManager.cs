using System;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private static RoomManager rm;

    private GameObject currentRoom;
    private string currentRoomName;
    public static event Action OnRoomChanged;

    public static GameObject GetCurrentRoom()
    {
        return rm.currentRoom;
    }

    public static string GetCurrentRoomName()
    {
        return rm.currentRoomName;
    }

    public static void ChangeRoom(string newRoomName)
    {
        RuntimeSaveManager.SaveRoom(rm.currentRoom, rm.currentRoomName);
        ChangeRoomWithoutSaving(newRoomName);
    }

    public static void ChangeRoomWithoutSaving(string newRoomName)
    {
        // Can't use normal Destroy(), because InteractionManager is getting the list of interactable on the same frame
        if (rm.currentRoom != null) DestroyImmediate(rm.currentRoom);
        rm.currentRoom = RuntimeSaveManager.LoadRoom(newRoomName);
        rm.currentRoomName = newRoomName;
        OnRoomChanged?.Invoke();
    }

    private void Awake()
    {
        if (rm != null)
        {
            Debug.LogWarning("More than 1 RoomManager in the scene");
            return;
        }
        rm = this;
    }

    //private void Start()
    //{
    //    currentRoom = RuntimeSaveManager.LoadRoom(startingRoomName);
    //    currentRoomName = startingRoomName;
    //}
}
