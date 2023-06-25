using Cysharp.Threading.Tasks;
using Game.Services;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Game.Sprites;


namespace Game.Context
{
    public class GameController : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private GameUI gameUI;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SpriteEraser spriteEraser;
        [Header("State")]
        [SerializeField] private GameStates gameState;


        private List<ITargetEvent> eventHandlers;
        private EventTypes lastEvent;
        private bool isEnded;

        public EventTypes LastEvent
        {
            get => lastEvent;
            set
            {
                if((int)value > (int)lastEvent)
                {
                    lastEvent = value;
                }
            }
        }


        private void Awake()
        {
            levelManager.OnLevelLoaded += OnLevelLoaded;
            gameUI.OnNextClick += NextLevel;
            gameUI.OnRestartClick += RestartLevel;
        }
        private void Start()
        {
            levelManager.LoadLevel();
        }


        private void RestartLevel()
        {
            levelManager.LoadLevel();
        }
        private void NextLevel()
        {
            levelManager.LoadNextLevel();
        }


        private void Win()
        {
            gameState = GameStates.Win;

            gameUI.State = gameState;
        }
        private void Fail()
        {
            gameState = GameStates.Fail;

            gameUI.State = gameState;
        }


        private void OnDone(EventTypes obj)
        {
            LastEvent = obj;
            if(!isEnded)
            {
                CheckEnd();
                isEnded = true;
            }
        }
        private async void CheckEnd()
        {
            await UniTask.Delay(250);

            switch(LastEvent)
            {
                case EventTypes.Win:
                    Win();
                    break;
                default:
                    Fail();
                    break;
            }
        }


        private void OnLevelLoaded(Level level)
        {            
            gameState = GameStates.Game;
            lastEvent = EventTypes.None;
            isEnded = false;

            eventHandlers = new List<ITargetEvent>(GetComponentsInChildren<ITargetEvent>(true));
            foreach(ITargetEvent handler in eventHandlers)
            {
                handler.OnDone += OnDone;
            }

            gameUI.UpdateLevel(level.Index + 1, level.Label);
            gameUI.State = gameState;

            spriteEraser.Clear();
        }

    }
}
