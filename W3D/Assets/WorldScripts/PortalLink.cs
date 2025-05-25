using UnityEngine;

public class PortalLink : MonoBehaviour
{
    public string destinationUrl;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"🌌 Entered portal. Loading space at: {destinationUrl}");
            // You can call your network manager or scene loader here.
            EventBus<string>.Raise(@"D:\Development\W3D_Git\W3D\SceneJson\scene.json");

        }
    }
}
