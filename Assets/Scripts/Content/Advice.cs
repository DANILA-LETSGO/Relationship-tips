using UnityEngine;

namespace Clicker.Content
{
    [CreateAssetMenu(menuName = "Clicker/Advice", fileName = "Advice")]
    public class Advice : ScriptableObject
    {
        public int index = 1;            // 1..15
        public string title = "Совет";
        [TextArea(2,4)]
        public string shortText = "Совет";
        [TextArea(6,20)]
        public string longExplanation = "Объяснение";
        
        public bool unlockedInSave = true;
    }
}
