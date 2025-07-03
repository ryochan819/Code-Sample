using System;
using UnityEngine;

public static class ModsEvent
{
    public static event Action<bool> OnModsListUpdated;
    public static void ModsListUpdated(bool subscribed)
    {
        OnModsListUpdated?.Invoke(subscribed);
    }

    public static event Action<bool> syncingWithHost;
    public static void SyncingWithHost(bool syncing)
    {
        syncingWithHost?.Invoke(syncing);
    }
}
