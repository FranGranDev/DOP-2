using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace Game.Context
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private bool editorMode;
        [SerializeField, Min(0)] private int levelIndex;
        [Header("Data")]
        [SerializeField] private LevelsData levelsData;
        [Header("Components")]
        [SerializeField] private Transform levelContainer;

        public int LevelIndex
        {
            get
            {
                if (editorMode)
                {
                    return levelIndex;
                }

                return SavedData.LevelIndex;
            }
            set
            {
                if (editorMode)
                {
                    levelIndex = Mathf.Max(value, 0);
                }

                SavedData.LevelIndex = Mathf.Max(value, 0);
            }
        }
        public int NormalizedLevelIndex
        {
            get
            {
                if (editorMode)
                {
                    return levelIndex % levelsData.Levels.Count;
                }

                return SavedData.LevelIndex % levelsData.Levels.Count;
            }
        }



        public event System.Action<Level> OnLevelLoaded;


        public void LoadNextLevel()
        {
            LevelIndex++;

            LoadLevel();
        }

        public void LoadLevel()
        {
            ClearLevel();

            Level level = levelsData.Levels[NormalizedLevelIndex];
            GameObject prefab = GetLevelPrefab(level);
            if(prefab == null)
            {
                return;
            }

            Instantiate(prefab, levelContainer);

            OnLevelLoaded?.Invoke(level);
        }
        private void ClearLevel()
        {
            while (levelContainer.childCount > 0)
            {
                DestroyImmediate(levelContainer.GetChild(0).gameObject);
            }
        }
        private GameObject GetLevelPrefab(Level level)
        {
            try
            {
                return Resources.Load(level.Path, typeof(GameObject)) as GameObject;
            }
            catch
            {
                Debug.LogError($"Can't load level at path: {level.Path}");
                return null;
            }
        }


        #region Internal
#if UNITY_EDITOR
        [Button("Next Level")]
        private void NextLevel()
        {
            LevelIndex++;

            LoadLevel();            
        }
        [Button("Prev Level")]
        private void PrevLevel()
        {
            LevelIndex--;

            LoadLevel();
        }
#endif
        #endregion
    }
}
