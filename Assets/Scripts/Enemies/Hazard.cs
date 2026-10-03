using UnityEngine;

// Damages the diver on contact. Put on any trigger collider (jellyfish, ray, shark).
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerStay2D(Collider2D other)
    {
        var diver = other.GetComponentInParent<DiverController>();
        if (diver == null) return;

        if (GameState.Instance.Damage(damage))
        {
            var away = (Vector2)(diver.transform.position - transform.position);
            diver.Knock(away.sqrMagnitude > 0.001f ? away.normalized : Vector2.up);
        }
    }
}
