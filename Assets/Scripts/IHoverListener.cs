using UnityEngine;

public interface IHoverListener
{
        public virtual void StartHover(GameObject emitter) {}
        
        public virtual void EndHover(GameObject emitter) {}
}

