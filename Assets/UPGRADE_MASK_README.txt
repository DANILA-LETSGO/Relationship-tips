
Upgrade Mask Patch
------------------

What this patch does:
- Replaces Assets/Scripts/UI/UpgradeUIEntry.cs with a version that hides the title and description
  behind '??' for upgrades that:
    * have level == 0 (never purchased) AND
    * the player currently cannot afford them.
- The first two upgrades (index 0 and 1 in UpgradeManager.upgrades) are exempt and are always shown.
- The UI updates automatically when the player's currency changes or when upgrade stats change.

How to apply:
1. Copy the file Assets/Scripts/UI/UpgradeUIEntry.cs into your project (overwrite existing).
2. Open Unity and allow scripts to recompile.
3. No other steps needed — instantiate/prefab usage stays the same.

If you want a different rule (for example, reveal after player reaches some other threshold) tell me and I'll adapt it.
