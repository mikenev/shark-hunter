using UnityEngine;

// Trigger zone that loads another scene when the boat or diver touches it,
// optionally gated on how many conches have been collected.
[RequireComponent(typeof(Collider2D))]
public class SceneTrigger : MonoBehaviour
{
    public string targetScene;
    public int requiredConches;
    [Tooltip("Use {n} for the number of conches still missing.")]
    public string blockedMessage = "YOU NEED {n} MORE CONCH";
    [Tooltip("Where the boat reappears on the sea map when it returns, relative to this trigger.")]
    public Vector2 returnOffset = new Vector2(0f, -2.5f);

    float nextMessage;
    bool loading;

    void OnTriggerStay2D(Collider2D other)
    {
        if (loading) return;

        bool isBoat = other.GetComponentInParent<BoatController>() != null;
        bool isDiver = other.GetComponentInParent<DiverController>() != null;
        if (!isBoat && !isDiver) return;

        var gs = GameState.Instance;
        int missing = requiredConches - gs.Conches;
        if (missing > 0)
        {
            if (Time.time >= nextMessage)
            {
                gs.ShowMessage(blockedMessage.Replace("{n}", missing.ToString()), 2f);
                nextMessage = Time.time + 2.5f;
            }
            return;
        }

        if (isBoat) gs.SaveBoatPos((Vector2)transform.position + returnOffset);

        loading = true;
        SceneLoader.Load(targetScene);
    }
}
