using UnityEngine;

// Collectible conch. The id is remembered in GameState so it doesn't respawn on a later dive.
[RequireComponent(typeof(Collider2D))]
public class Conch : MonoBehaviour
{
    public string id;

    Vector3 basePos;

    void Start()
    {
        if (GameState.Instance.HasCollected(id))
        {
            Destroy(gameObject);
            return;
        }
        basePos = transform.position;
    }

    void Update()
    {
        transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<DiverController>() == null) return;
        GameState.Instance.Collect(id);
        Destroy(gameObject);
    }
}
