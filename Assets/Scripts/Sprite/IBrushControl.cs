using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Sprites
{
    public interface IBrushControl
    {
        public Vector2 InputPoint { get; set; }
        public void StartErase();
        public void EndErase();
    }
}
