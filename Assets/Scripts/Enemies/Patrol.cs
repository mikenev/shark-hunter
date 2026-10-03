using UnityEngine;

// Moves back and forth along an axis around its starting position.
public class Patrol : MonoBehaviour
{
    public Vector2 axis = Vector2.right;
    public float distance = 3f;
    public float speed = 1.5f;
    public SpriteRenderer spriteRenderer;

    Vector3 origin;
    float phase;

    void Start()
    {
        origin = transform.position;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        phase += speed / Mathf.Max(0.01f, distance) * Time.deltaTime;
        transform.position = origin + (Vector3)(axis.normalized * (Mathf.Sin(phase) * distance));

        // Face the direction of travel for horizontal patrols.
        if (spriteRenderer != null && Mathf.Abs(axis.x) > 0.01f)
            spriteRenderer.flipX = Mathf.Cos(phase) < 0f;
    }
}
