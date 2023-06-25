using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    public interface IGameEvent
    {
        public event System.Action<GameStates> OnStateChanged;
        public event System.Action<int, string> OnLevelLoaded;
    }
}
