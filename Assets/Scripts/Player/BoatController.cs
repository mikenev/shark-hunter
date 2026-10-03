using UnityEngine;

// Top-down boat for the sea map. Only the child visual rotates, so the collider stays upright.
[RequireComponent(typeof(Rigidbody2D))]
public class BoatController : MonoBehaviour
{
    public float speed = 5f;
    public Transform visual;

    Rigidbody2D rb;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var gs = GameState.Instance;
        if (gs != null && gs.HasSavedBoatPos) transform.position = gs.SavedBoatPos;
    }

    void Update()
    {
        input = GameInput.Move;
        if (visual != null && input.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg - 90f;
            visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}
