using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private Vector2 _cursorPosition;

    private void Awake() { Cursor.visible = false; }

    private void Update()
    {
        _cursorPosition    = ObjectHolder.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = _cursorPosition;
    }
}