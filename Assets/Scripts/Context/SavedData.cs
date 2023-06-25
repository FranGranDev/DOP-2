using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Context
{
    public static class SavedData 
    {
        private const string LEVEL_INDEX_KEY = "level_index";

        public static int LevelIndex
        {
            get => PlayerPrefs.GetInt(LEVEL_INDEX_KEY, 0);
            set => PlayerPrefs.SetInt(LEVEL_INDEX_KEY, value);
        }
    }
}
