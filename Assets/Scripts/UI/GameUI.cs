using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Services;
using TMPro;


namespace UI
{
    public class GameUI : MonoBehaviour, IBindable<IGameEvent>
    {
        [Header("Menus")]
        [SerializeField] private GameObject gameMenu;
        [SerializeField] private GameObject winMenu;
        [SerializeField] private GameObject failMenu;
        [Header("Buttons")]
        [SerializeField] private ButtonUI nextButton;
        [SerializeField] private ButtonUI restartButton;
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private TextMeshProUGUI levelNumber;
        [Header("States")]
        [SerializeField] private GameStates state;

        private Dictionary<GameStates, List<UIPanel>> menuPanels;

        private IGameEvent gameEvent;



        public event Action OnNextClick;
        public event Action OnRestartClick;



        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            menuPanels = new Dictionary<GameStates, List<UIPanel>>()
            {
                {GameStates.Game, gameMenu.GetComponentsInChildren<UIPanel>(true).ToList() },
                {GameStates.Win, winMenu.GetComponentsInChildren<UIPanel>(true).ToList() },
                {GameStates.Fail, failMenu.GetComponentsInChildren<UIPanel>(true).ToList() },
            };
            menuPanels.Values.SelectMany(x => x).ToList().ForEach(x => x.Initilize());

            nextButton.OnClick += NextClick;
            restartButton.OnClick += RestartClick;


            TurnUI(true);            
        }
        public void Bind(IGameEvent obj)
        {
            gameEvent = obj;

            gameEvent.OnLevelLoaded += UpdateLevel;
            gameEvent.OnStateChanged += ChangeState;
        }

        public void UpdateLevel(int level, string label)
        {
            levelLabel.text = label;
            levelNumber.text = $"LEVEL {level}";
        }

        private void ChangeState(GameStates value)
        {
            OnStateEnd(state);
            state = value;
            OnStateStart(state);
        }
        private void OnStateStart(GameStates state)
        {
            menuPanels[state].ForEach(x => x.IsShown = true);
        }
        private void OnStateEnd(GameStates state)
        {
            menuPanels[state].ForEach(x => x.IsShown = false);
        }

        private void TurnUI(bool on)
        {
            winMenu.SetActive(on);
            failMenu.SetActive(on);
            gameMenu.SetActive(on);
        }


        private void RestartClick()
        {
            if (state != GameStates.Fail)
                return;
            OnRestartClick?.Invoke();   
        }
        private void NextClick()
        {
            if (state != GameStates.Win)
                return;
            OnNextClick?.Invoke();
        }

    }
}
