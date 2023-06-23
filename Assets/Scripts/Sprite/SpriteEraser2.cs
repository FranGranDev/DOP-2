using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteEraser2 : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Min(0.1f)] private float brushSize = 1f;
    [SerializeField, Range(0.1f, 1f)] private float brushSpeed = 1f;
    [Header("Components")]
    [SerializeField] private Transform brushObject;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject maskPrefab;


    private List<GameObject> masks = new List<GameObject>();


    private Vector2 inputPoint;
    private Vector2 brushPoint;
    private bool isBrushing;

    private void BrushErase()
    {
        float delta = 0.1f;
        for (float i = 0; i < 1f; i += delta)
        {
            Vector3 point = Vector3.Lerp(brushPoint, inputPoint, i);

            GameObject mask = Instantiate(maskPrefab, point, Quaternion.identity, container);
            mask.transform.localScale = Vector3.one * brushSize * 2;

            masks.Add(mask);
        }
    }




    private void FixedUpdate()
    {
        if (isBrushing)
        {
            BrushErase();
        }

        brushPoint = Vector2.Lerp(brushObject.position, inputPoint, brushSpeed);


        brushObject.localScale = Vector2.Lerp(brushObject.localScale, Vector3.one * brushSize * 2, 0.1f);
        brushObject.position = brushPoint;
    }
    private void Update()
    {
        isBrushing = Input.GetKey(KeyCode.Mouse0);
        inputPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

}
