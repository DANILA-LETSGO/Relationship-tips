Auto Income UI Patch
---------------------

This patch adds a UI element and component that displays the current automatic income per second
(the amount AutoClicker would add each second if the player does nothing).

Files included:
- Assets/Scripts/UI/AutoIncomeUI.cs
- Assets/Editor/AddAutoIncomeUIText.cs

Usage:
1) Import these files into your Unity project (place under Assets/).
2) In Unity Editor, run: Tools -> Clicker -> Add Auto Income UI
   - This will create a Text object on your Canvas and attach AutoIncomeUI.
3) Play the scene. The text will update automatically when upgrade stats change or periodically.

Notes:
- The displayed value is computed as:
    shown = UpgradeManager.CurrentStats.autoIncomePerSecond * UpgradeManager.TotalMultiplier * GameManager.prestigeMultiplier
  which mirrors how GameManager.AddCurrency multiplies incomes when adding currency.
- If you changed the calculation logic elsewhere, adjust AutoIncomeUI accordingly.
