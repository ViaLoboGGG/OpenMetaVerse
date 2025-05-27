using System;
using UnityEngine;

public class PortalLink : MonoBehaviour
{
    public string destinationUrl;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"🌌 Entered portal. Loading space at: {destinationUrl}");
            // Raise EventBus event
            EventBus<PortalTransitEvent>.Raise(new PortalTransitEvent { DestinationUrl = destinationUrl });

        }
    }

}
public struct PortalTransitEvent
{
    public string DestinationUrl;         // Required: where to go
    public string PortalId;               // Optional: which portal triggered it
    public Vector3 EntryPosition;         // Optional: where the player entered
    public Quaternion EntryRotation;      // Optional: direction they were facing
    public string PlayerId;               // Optional: multiplayer support or logging
    public string TriggeredByScript;      // Optional: name or ID of script/component that triggered it
    public DateTime TriggerTime;          // Optional: useful for logging/timestamped analytics

    public PortalTransitEvent(string destinationUrl, Vector3 entryPosition, Quaternion entryRotation, string portalId = "", string playerId = "", string triggeredByScript = "")
    {
        DestinationUrl = destinationUrl;
        EntryPosition = entryPosition;
        EntryRotation = entryRotation;
        PortalId = portalId;
        PlayerId = playerId;
        TriggeredByScript = triggeredByScript;
        TriggerTime = DateTime.UtcNow;
    }
}