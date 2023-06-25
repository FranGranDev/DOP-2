using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Brush : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField, Min(0)] private float animationTime = 0.25f;
    [SerializeField] private Ease animationEase = Ease.InOutSine;
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;


    private bool hidden;
    private Tween tween;
    private Vector3 scale;

    public SpriteArea Area { get; private set; }
    public Vector2 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
    public Vector2 Size
    {
        get => spriteRenderer.size;
    }


    public bool Hidden
    {
        get => hidden;
        set
        {
            if (hidden == value)
                return;

            hidden = value;
            Animate(hidden ? Vector3.zero : scale);
        }
    }

    private void Animate(Vector3 scale)
    {
        if (tween != null)
        {
            tween.Kill();
        }

        tween = transform.DOScale(scale, animationTime)
            .SetEase(animationEase);
    }


    private void Awake()
    {
        Area = new SpriteArea(spriteRenderer);
        scale = transform.localScale;
    }
}
