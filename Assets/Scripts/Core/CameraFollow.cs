using UnityEngine;

// Follows the target, clamped so the view never leaves the level bounds.
// halfView matches the Pixel Perfect reference resolution (320x240 at 16 PPU = 20x15 units).
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 boundsMin;
    public Vector2 boundsMax;
    public Vector2 halfView = new Vector2(10f, 7.5f);

    void LateUpdate()
    {
        if (target == null) return;

        var p = target.position;
        p.x = ClampAxis(p.x, boundsMin.x + halfView.x, boundsMax.x - halfView.x);
        p.y = ClampAxis(p.y, boundsMin.y + halfView.y, boundsMax.y - halfView.y);
        p.z = -10f;
        transform.position = p;
    }

    // If the level is smaller than the view on this axis, center on it.
    static float ClampAxis(float v, float lo, float hi) =>
        lo > hi ? (lo + hi) * 0.5f : Mathf.Clamp(v, lo, hi);
}
