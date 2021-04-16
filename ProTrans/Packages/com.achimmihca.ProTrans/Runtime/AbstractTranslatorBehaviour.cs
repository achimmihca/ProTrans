using UnityEngine;

namespace ProTrans
{
    public abstract class AbstractTranslatorBehaviour : MonoBehaviour, ITranslator
    {
        protected virtual void Start()
        {
            UpdateTranslation();
        }
        
        public abstract void UpdateTranslation();
    }
}
