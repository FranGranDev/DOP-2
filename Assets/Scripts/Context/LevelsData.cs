using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Game.Context
{
    [CreateAssetMenu(menuName = "Data/Levels")]
    public class LevelsData : ScriptableObject
    {
        [SerializeField] private List<Level> levels = new List<Level>();


        public List<Level> Levels
        {
            get => levels;
        }


        #region Internal
#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                Level level = levels[i];
                level.Index = i;

                if (level.LevelPrefab == null)
                    continue;
                level.Name = level.LevelPrefab.name;
                string path = AssetDatabase.GetAssetPath(level.LevelPrefab).Replace("Assets/Resources/", "").Replace(".prefab", "");

                level.Path = path;
                level.LevelPrefab = null;
            }
        }

#endif
        #endregion
    }

    [System.Serializable]
    public class Level
    {
        [Header("Info")]
        [SerializeField] private string name;
        [SerializeField] private string path;
        [SerializeField] private int index;
        [Header("Settings")]
        [SerializeField] private string label;

        [SerializeField] private GameObject levelPrefab;


        public string Name
        {
            get => name;
            set => name = value;
        }
        public string Path
        {
            get => path;
            set => path = value;
        }
        public int Index
        {
            get => index;
            set => index = value;
        }
        public string Label
        {
            get => label;
        }
        public GameObject LevelPrefab
        {
            get => levelPrefab;
            set => levelPrefab = value;
        }
    }
}

