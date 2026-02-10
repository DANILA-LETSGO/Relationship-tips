Advice ScriptableObjects helper
-------------------------------

1) Open Unity, load the project.
2) In the editor menu: Tools -> Clicker -> Create 15 Advice assets
   - This will create 15 ScriptableObject assets in Assets/Resources/Advices/Advice_1.asset ... Advice_15.asset
   - Also creates/updates Assets/Resources/AdviceDB.asset linking them.
3) You can then edit each Advice_X in the Inspector: title, shortText, longExplanation.
4) LevelUI will automatically load AdviceDB from Resources and display the corresponding texts when an explanation unlocks at 58%.
