using UnityEngine;

public interface IHoverListener
{
        public virtual void OnHoverStart(GameObject emitter) {}
        
        public virtual void OnHoverEnd(GameObject emitter) {}
}

