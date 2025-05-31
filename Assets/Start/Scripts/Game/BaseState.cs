using System;
using UnityEngine;


namespace Start.Scripts
{
    public abstract class BaseState
    {
        public BaseState(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
        }

        protected GameObject gameObject;
        protected Transform transform;

        public abstract Type Tick();
    }

}
