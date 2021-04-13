using UnityEngine;

namespace ProTrans
{
    public abstract class AbstractTranslatorBehaviour : MonoBehaviour, ITranslator
    {
        protected virtual void Start()
        {
            UpdateTranslation();
        }
        
        abstract public void UpdateTranslation();
    }
}
