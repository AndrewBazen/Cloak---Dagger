using UnityEngine;

namespace Start.Scripts
{
    public class DestroyOnAnimationEnd : MonoBehaviour
    {
        public void DestroyParent(){
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
