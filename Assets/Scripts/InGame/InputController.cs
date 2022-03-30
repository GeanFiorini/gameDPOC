using UnityEngine;
using Lean.Touch;

public class InputController : MonoBehaviour
{
    public static InputController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputController>();
            }
            return instance;
        }
    }

    private static InputController instance = null;

    private bool _up, _down, _left, _right;

    private void Start()
    {
        LeanTouch.OnFingerSwipe += OnSwipe;
    }

    private void LateUpdate()
    {
        this._up = this._down = this._left = this._right = false;
    }

    private void OnDestroy()
    {
        instance = null;
        LeanTouch.OnFingerSwipe -= OnSwipe;
    }

    private void OnSwipe(LeanFinger finger)
    {
        Vector2 swipeDelta = finger.SwipeScreenDelta.normalized;

        const float dotThreshold = 0.5f;

        if (Vector2.Dot(swipeDelta, Vector2.right) > dotThreshold)
        {
            this._right = true;
        }
        else if (Vector2.Dot(swipeDelta, Vector2.left) > dotThreshold)
        {
            this._left = true;
        }
        else if (Vector2.Dot(swipeDelta, Vector2.up) > dotThreshold)
        {
            this._up = true;
        }
        else if (Vector2.Dot(swipeDelta, Vector2.down) > dotThreshold)
        {
            this._down = true;
        }
    }

    public bool GoingLeft()
    {
        return Input.GetKeyDown(KeyCode.LeftArrow) || this._left;
    }

    public bool GoingRight()
    {
        return Input.GetKeyDown(KeyCode.RightArrow) || this._right;
    }

    public bool GoingUp()
    {
        return Input.GetKeyDown(KeyCode.UpArrow) || this._up;
    }

    public bool GoingDown()
    {
        return Input.GetKeyDown(KeyCode.DownArrow) || this._down;
    }
}
