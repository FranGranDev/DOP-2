using Cysharp.Threading.Tasks;
using System.Linq;
using Game.Services;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Game.Sprites;
using System;

namespace Game.Context
{
    public class GameController : MonoBehaviour, IGameEvent
    {
        [Header("Links")]
        [SerializeField] private GameUI gameUI;
        [SerializeField] private LevelManager levelManager;
        [Header("State")]
        [SerializeField] private GameStates gameState;


        private List<ITargetEvent> eventHandlers;
        private EventTypes lastEvent;
        private bool isEnded;


        public event Action<GameStates> OnStateChanged;
        public event Action<int, string> OnLevelLoaded;


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
        public GameStates GameState
        {
            get => gameState;
            set
            {
                gameState = value;
                OnStateChanged?.Invoke(value);
            }
        }


        private void Awake()
        {
            Setup();
            Initialize();
        }
        private void Start()
        {
            levelManager.LoadLevel();
        }


        private void Setup()
        {
            Application.targetFrameRate = 60;
        }
        private void Initialize()
        {
            levelManager.OnLevelLoaded += LevelLoaded;
            gameUI.OnNextClick += NextLevel;
            gameUI.OnRestartClick += RestartLevel;


            BindableService.AutoBind<IGameEvent>(this);
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
            GameState = GameStates.Win;
        }
        private void Fail()
        {
            GameState = GameStates.Fail;
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


        private void LevelLoaded(Level level)
        {
            GameState = GameStates.Game;
            lastEvent = EventTypes.None;
            isEnded = false;

            eventHandlers = new List<ITargetEvent>(GetComponentsInChildren<ITargetEvent>(true));
            foreach(ITargetEvent handler in eventHandlers)
            {
                handler.OnDone += OnDone;
            }

            OnLevelLoaded?.Invoke(level.Index + 1, level.Label);
        }

    }
}
