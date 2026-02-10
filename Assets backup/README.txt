AutoClicker Unity 2020.3 Sample Project
---------------------------------------

1) Open in Unity 2020.3.x
2) Go to menu: Tools -> Clicker -> Build Scene & Prefabs
   - This will:
     * Convert textures to Sprites
     * Create ScriptableObject upgrades (20+)
     * Create a Canvas with currency text, a big Click button, and a scrollable list of upgrades
     * Create a prefab for upgrade entry and wire it up
     * Save scene at Assets/Clicker_AutoScene.unity
3) Press Play.
4) Click the button to earn currency. Buy upgrades in the list. Auto-clicker runs based on purchased upgrades.
5) PlayerPrefs stores progress; simple prestige logic is included on GameManager (call DoPrestige from Debug or add a UI).

This is a compact, dependency-free UI using the built-in legacy UI (UnityEngine.UI).
