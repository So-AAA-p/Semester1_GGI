using UnityEngine;

namespace BreakOut
{
public class BreakoutDeathZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        // Lebenspunkt abziehen und neuen Ball spawnen
        BreakOutManager.instance.OnDeath();

        // diesen Ball zerstören
        Destroy(other.gameObject);
    }
}

}
