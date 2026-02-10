using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clicker.Core
{
    [Serializable]
    public class SaveData
    {
        public double currency;
        // In-memory convenience dictionary
        public Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();
        public long lastQuitUnixSeconds;
        public int prestige;
        public double totalEarned;
    }

    [Serializable]
    internal class SerializableSaveData
    {
        public double currency;
        public string[] upgradeIds;
        public int[] upgradeLevels;
        public long lastQuitUnixSeconds;
        public int prestige;
        public double totalEarned;
    }

    public static class SaveSystem
    {
        private const string Key = "CLICKER_SAVE_V1";

        public static void Save(SaveData data)
        {
            if (data == null) return;

            var ids = new List<string>();
            var levels = new List<int>();
            if (data.upgradeLevels != null)
            {
                foreach (var kv in data.upgradeLevels)
                {
                    ids.Add(kv.Key);
                    levels.Add(kv.Value);
                }
            }

            var ser = new SerializableSaveData()
            {
                currency = data.currency,
                upgradeIds = ids.ToArray(),
                upgradeLevels = levels.ToArray(),
                lastQuitUnixSeconds = data.lastQuitUnixSeconds,
                prestige = data.prestige,
                totalEarned = data.totalEarned
            };

            string json = JsonUtility.ToJson(ser);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
#if UNITY_EDITOR
            Debug.Log("[SaveSystem] Saved " + json);
#endif
        }

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return new SaveData();

            try
            {
                string json = PlayerPrefs.GetString(Key);
#if UNITY_EDITOR
                Debug.Log("[SaveSystem] Loading " + json);
#endif
                var ser = JsonUtility.FromJson<SerializableSaveData>(json);
                var data = new SaveData();
                data.currency = ser != null ? ser.currency : 0;
                data.lastQuitUnixSeconds = ser != null ? ser.lastQuitUnixSeconds : 0;
                data.prestige = ser != null ? ser.prestige : 0;
                data.totalEarned = ser != null ? ser.totalEarned : 0;
                data.upgradeLevels = new Dictionary<string, int>();

                if (ser != null && ser.upgradeIds != null && ser.upgradeLevels != null)
                {
                    int n = Math.Min(ser.upgradeIds.Length, ser.upgradeLevels.Length);
                    for (int i = 0; i < n; i++)
                    {
                        var id = ser.upgradeIds[i];
                        var lv = ser.upgradeLevels[i];
                        if (!string.IsNullOrEmpty(id))
                            data.upgradeLevels[id] = lv;
                    }
                }

                return data;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[SaveSystem] Failed to load save: " + e);
#endif
                return new SaveData();
            }
        }
    }
}
