using System.Collections.Generic;
using UnityEngine;

namespace EcoSort.Gameplay
{
    /// <summary>
    /// Tracks level attempts, mistakes, and bin mastery progression across scene reloads.
    /// Ensures that if the player makes a mistake and the level restarts, the attempt count
    /// is preserved and reflected on the Level Completed performance screen.
    /// </summary>
    public static class EcoSessionTracker
    {
        private static readonly Dictionary<int, int> _attemptsByLevel = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> _hintsAcrossAttempts = new Dictionary<int, int>();
        private static readonly Dictionary<int, bool> _restartedDueToMistake = new Dictionary<int, bool>();
        private static readonly Dictionary<int, string> _lastMistakeMessageEs = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> _lastMistakeMessageEn = new Dictionary<int, string>();

        public static int GetAttempts(int levelNumber)
        {
            if (!_attemptsByLevel.TryGetValue(levelNumber, out int count) || count < 1)
            {
                _attemptsByLevel[levelNumber] = 1;
                return 1;
            }
            return count;
        }

        public static int GetAccumulatedHints(int levelNumber)
        {
            return _hintsAcrossAttempts.TryGetValue(levelNumber, out int hints) ? hints : 0;
        }

        public static void AddHintUsed(int levelNumber)
        {
            _hintsAcrossAttempts[levelNumber] = GetAccumulatedHints(levelNumber) + 1;
        }

        public static bool WasRestartedFromMistake(int levelNumber)
        {
            return _restartedDueToMistake.TryGetValue(levelNumber, out bool flag) && flag;
        }

        public static string GetLastMistakeEs(int levelNumber)
        {
            return _lastMistakeMessageEs.TryGetValue(levelNumber, out string msg) ? msg : "";
        }

        public static string GetLastMistakeEn(int levelNumber)
        {
            return _lastMistakeMessageEn.TryGetValue(levelNumber, out string msg) ? msg : "";
        }

        public static void RecordMistakeAndPrepareRestart(int levelNumber, string mistakeEs, string mistakeEn)
        {
            int current = GetAttempts(levelNumber);
            _attemptsByLevel[levelNumber] = current + 1;
            _restartedDueToMistake[levelNumber] = true;
            _lastMistakeMessageEs[levelNumber] = mistakeEs;
            _lastMistakeMessageEn[levelNumber] = mistakeEn;
        }

        public static void ClearLevelSession(int levelNumber)
        {
            _attemptsByLevel[levelNumber] = 1;
            _hintsAcrossAttempts[levelNumber] = 0;
            _restartedDueToMistake[levelNumber] = false;
            _lastMistakeMessageEs[levelNumber] = "";
            _lastMistakeMessageEn[levelNumber] = "";
        }

        /// <summary>
        /// Returns the 6 bin mastery percentages in order:
        /// [0] Green (Orgánico), [1] Grey (Normal), [2] Red (Punzocortantes),
        /// [3] Blue (Inorgánico), [4] Yellow (Vidrios), [5] Orange (Especiales).
        /// -1 means "Pronto / Coming soon" (not yet introduced).
        /// </summary>
        public static int[] GetBinMasteriesForLevel(int completedLevel)
        {
            return completedLevel switch
            {
                // Tutorial: All 6 bins are "Pronto / Coming soon" (matches EjemploTutorial3.png!)
                <= 0 => new[] { -1, -1, -1, -1, -1, -1 },
                // Level 1: Introduces Green, Grey, Blue (1st sorting with Green)
                1 => new[] { 50, 25, -1, 25, -1, -1 },
                // Level 2: Green, Grey, Blue mastery grows
                2 => new[] { 80, 60, -1, 75, -1, -1 },
                // Level 3: Introduces Red (Punzocortantes)
                3 => new[] { 100, 100, 50, 80, -1, -1 },
                // Level 4: Introduces Yellow (Vidrios)
                4 => new[] { 100, 100, 75, 90, 50, -1 },
                // Level 5: Introduces Orange (All 6 bins active!)
                5 => new[] { 100, 100, 75, 100, 65, 40 },
                // Level 6: Practicing Yellow, Red, Orange, Grey
                6 => new[] { 100, 100, 90, 100, 80, 70 },
                // Level 7: Multi-bin mastery
                7 => new[] { 100, 100, 90, 100, 90, 85 },
                // Level 8: Advanced sorting
                8 => new[] { 100, 100, 100, 100, 95, 90 },
                // Level 9: Master of all 6 bins!
                _ => new[] { 100, 100, 100, 100, 100, 100 }
            };
        }
    }
}
