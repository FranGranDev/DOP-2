using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Sprites;


namespace Game.Context
{
    public interface ITargetEvent
    {
        public event System.Action<EventTypes> OnDone;
    }


    [RequireComponent(typeof(IErasedEvent))]
    public class TargetEvent : MonoBehaviour, ITargetEvent
    {
        [SerializeField] private EventTypes eventType;


        public event System.Action<EventTypes> OnDone;

        private void CallOnDone()
        {
            OnDone?.Invoke(eventType);
        }


        private void Awake()
        {
            GetComponent<IErasedEvent>().OnErased += CallOnDone;
        }
    }


    public enum EventTypes
    {
        None,
        Win,
        Lose,
    }
}