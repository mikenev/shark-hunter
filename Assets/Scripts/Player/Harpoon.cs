using UnityEngine;

// Simple straight-flying projectile. Hit detection uses an overlap query each frame,
// so it needs no Rigidbody and works against trigger colliders.
public class Harpoon : MonoBehaviour
{
    const float Speed = 14f;
    const float Lifetime = 1.5f;
    const float HitRadius = 0.25f;

    Vector2 direction;
    float life = Lifetime;

    public static void Spawn(Sprite sprite, Vector2 position, int facing)
    {
        var go = new GameObject("Harpoon");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 6;
        sr.flipX = facing < 0;

        go.AddComponent<Harpoon>().direction = Vector2.right * facing;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * (Speed * Time.deltaTime));

        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, HitRadius))
        {
            var boss = hit.GetComponentInParent<SharkBoss>();
            if (boss == null) continue;
            boss.Hit(1);
            Destroy(gameObject);
            return;
        }
    }
}
