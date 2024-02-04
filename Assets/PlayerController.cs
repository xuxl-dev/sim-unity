using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public void Look(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        // Debug.Log(input);
        // transform.eulerAngles += new Vector3(-input.y, input.x, 0);
    }

    Vector2 movePos;
    public void Move(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        movePos = input;
    }

    public void Click(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            Debug.Log("Click at " + movePos);
            PseudoClick.Instance.ClickAt(movePos.x, movePos.y);
        }
    }
}
