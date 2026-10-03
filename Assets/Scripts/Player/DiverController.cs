using UnityEngine;

// Side-view swimmer used in both the Dive and SharkFight scenes.
[RequireComponent(typeof(Rigidbody2D))]
public class DiverController : MonoBehaviour
{
    public float swimSpeed = 4f;
    public float sinkSpeed = 0.6f;
    public bool drainOxygen = true;
    public bool canFire;
    public float surfaceY = 14f;
    public Vector2 spawnPoint;
    public Sprite harpoonSprite;
    public SpriteRenderer spriteRenderer;

    const float FireCooldown = 0.35f;
    const float AirRefillRate = 8f;

    Rigidbody2D rb;
    Vector2 input;
    Vector2 knockVelocity;
    float knockUntil;
    float nextFire;
    int facing = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (GameState.Instance != null) GameState.Instance.OxygenActive = drainOxygen;
    }

    void OnDisable()
    {
        if (GameState.Instance != null) GameState.Instance.OxygenActive = false;
    }

    void Update()
    {
        var gs = GameState.Instance;
        input = GameInput.Move;

        if (Mathf.Abs(input.x) > 0.1f)
        {
            facing = input.x > 0f ? 1 : -1;
            if (spriteRenderer != null) spriteRenderer.flipX = facing < 0;
        }

        // Blink while invulnerable after a hit.
        if (spriteRenderer != null)
            spriteRenderer.enabled = !(gs.Invulnerable && (int)(Time.time * 15f) % 2 == 0);

        if (drainOxygen) UpdateOxygen(gs);

        if (canFire && harpoonSprite != null && GameInput.FirePressed && Time.time >= nextFire)
        {
            nextFire = Time.time + FireCooldown;
            Harpoon.Spawn(harpoonSprite, (Vector2)transform.position + Vector2.right * (facing * 0.6f), facing);
        }
    }

    void UpdateOxygen(GameState gs)
    {
        bool underwater = transform.position.y < surfaceY - 0.5f;
        gs.Oxygen = Mathf.Clamp(
            gs.Oxygen + (underwater ? -Time.deltaTime : AirRefillRate * Time.deltaTime),
            0f, GameState.MaxOxygen);

        if (gs.Oxygen > 0f) return;

        gs.Damage(1);
        gs.Oxygen = GameState.MaxOxygen;
        gs.ShowMessage("OUT OF AIR!", 2f);
        transform.position = spawnPoint;
        rb.linearVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (Time.time < knockUntil)
        {
            rb.linearVelocity = knockVelocity;
            return;
        }

        var velocity = input * swimSpeed;
        if (Mathf.Abs(input.y) < 0.1f) velocity.y = -sinkSpeed;
        rb.linearVelocity = velocity;
    }

    public void Knock(Vector2 direction)
    {
        knockVelocity = direction * 6f;
        knockUntil = Time.time + 0.2f;
    }
}
