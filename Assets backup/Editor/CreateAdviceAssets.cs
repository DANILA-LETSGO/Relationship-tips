#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Clicker.Content;
using System.Collections.Generic;

public static class CreateAdviceAssets
{
    [MenuItem("Tools/Clicker/Create 15 Advice assets")]
    public static void Create15Advices()
    {
        string dir = "Assets/Resources/Advices";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var list = new List<Advice>();

        for (int i = 1; i <= 15; i++)
        {
            string path = $"{dir}/Advice_{i}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Advice>(path);
            if (existing != null)
            {
                list.Add(existing);
                continue;
            }

            var adv = ScriptableObject.CreateInstance<Advice>();
            adv.index = i;
            adv.title = $"Совет {i}";
            adv.shortText = $"Совет {i}";
            adv.longExplanation = $"Объяснение {i} (здесь ты позже впишешь текст).";
            AssetDatabase.CreateAsset(adv, path);
            list.Add(adv);
        }

        // Create or update AdviceDatabase in Resources
        string dbPath = "Assets/Resources/AdviceDB.asset";
        var db = AssetDatabase.LoadAssetAtPath<AdviceDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<AdviceDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }
        db.advices = new List<Advice>(list);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created/updated 15 Advice assets and AdviceDB at Assets/Resources/");
    }
}
#endif
