Advice Menu Patch
-----------------
Files added:
- Assets/Scripts/Core/AdviceData.cs (ScriptableObject for each advice)
- Assets/Scripts/UI/AdviceEntryUI.cs (UI entry component)
- Assets/Scripts/UI/AdviceDataHolder.cs (helper holder)
- Assets/Scripts/UI/AdviceMenuController.cs (menu controller, populates entries)
- Assets/Editor/CreateAdviceMenuEditor.cs (Editor Menu: Tools -> Clicker -> Create Advice Menu (UI + sample Advices))

How to use:
1) In Unity open Tools -> Clicker -> Create Advice Menu (UI + sample Advices).
   This will create a panel under the first Canvas found, create a prefab for entries, and create 15 sample AdviceData assets under Assets/Resources/Advices.
2) Place the menu where you want. The panel will be created active.
3) The AdviceMenuController will auto-load AdviceData from Resources/Advices if no explicit list assigned and will try to detect your LevelManager to determine unlocked state.
4) Call AdviceMenuController.Populate() or .Refresh() if you add advices later or need to update unlocked state during gameplay.

Notes:
- The detection of whether an advice is unlocked is best-effort via reflection. If your LevelManager exposes a known API, the controller will try to call it (IsExplanationUnlocked(int) or GetLevelProgress(int) or read arrays named explanation(s)Unlocked). If it doesn't find anything, advices will be considered locked. You can manually call controller.Populate() after your LevelManager updates, or adapt the IsAdviceUnlocked method to fit your LevelManager API.
