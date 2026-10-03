using UnityEngine;
using UnityEngine.InputSystem;

// Minimal polling input: WASD/arrows/gamepad to move, Space/Z/south button to fire.
public static class GameInput
{
    public static Vector2 Move
    {
        get
        {
            var move = Vector2.zero;

            var k = Keyboard.current;
            if (k != null)
            {
                if (k.leftArrowKey.isPressed || k.aKey.isPressed) move.x -= 1f;
                if (k.rightArrowKey.isPressed || k.dKey.isPressed) move.x += 1f;
                if (k.downArrowKey.isPressed || k.sKey.isPressed) move.y -= 1f;
                if (k.upArrowKey.isPressed || k.wKey.isPressed) move.y += 1f;
            }

            var g = Gamepad.current;
            if (g != null)
            {
                var stick = g.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) move += stick;
                move += g.dpad.ReadValue();
            }

            return Vector2.ClampMagnitude(move, 1f);
        }
    }

    public static bool FirePressed
    {
        get
        {
            var k = Keyboard.current;
            if (k != null && (k.spaceKey.wasPressedThisFrame || k.zKey.wasPressedThisFrame)) return true;
            var g = Gamepad.current;
            return g != null && g.buttonSouth.wasPressedThisFrame;
        }
    }
}
