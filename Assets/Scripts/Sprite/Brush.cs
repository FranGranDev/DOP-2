using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public SpriteArea Area { get; private set; }
    public Vector2 Position
    {
        get => transform.position;
        set => transform.position = value;
    }


    private void Awake()
    {
        Area = new SpriteArea(spriteRenderer);
    }
}
