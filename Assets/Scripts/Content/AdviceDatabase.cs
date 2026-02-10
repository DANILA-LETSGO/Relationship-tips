using System.Collections.Generic;
using UnityEngine;

namespace Clicker.Content
{
    [CreateAssetMenu(menuName = "Clicker/AdviceDatabase", fileName = "AdviceDB")]
    public class AdviceDatabase : ScriptableObject
    {
        public List<Advice> advices = new List<Advice>();

        public Advice GetAdvice(int index)
        {
            if (advices == null) return null;
            for (int i = 0; i < advices.Count; i++)
            {
                var a = advices[i];
                if (a != null && a.index == index) return a;
            }
            return null;
        }
    }
}
