using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slafurry.Systems.Save
{
    /// <summary>Serializable save data for the game loop.</summary>
    [Serializable]
    public class GameSaveData
    {
        [Header("Time")]
        public int currentDay = 1;
        public float currentTimeSeconds = 0f;
        public float gameStartTimeSeconds = 0f;

        [Header("Teams")]
        public int terminatedCount = 0;
        public List<string> terminatedTeamIds = new();

        [Header("Tasks")]
        public List<string> completedTaskIds = new();

        public GameSaveData() { }
    }
}
