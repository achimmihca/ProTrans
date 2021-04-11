using UnityEngine;

namespace ProTrans
{
    public abstract class AbstractTranslatorBehaviour : MonoBehaviour, ITranslator
    {
        abstract public void UpdateTranslation();
    }
}
