using System.Collections;
using Slafurry.Core.Interface;
using Slafurry.Systems;
using UnityEngine;

namespace Slafurry.Core.Abstract
{
    /// <summary>
    /// Base for all "...Manager" - session/scene-scoped gameplay
    /// coordinators. NOT a standalone singleton. Implements IInitializable
    /// so LoadingSystem can boot it as part of the loading sequence.
    ///
    /// Awake() registers with LoadingSystem. PostInitialize() is called
    /// after every IInitializable has finished Initialize() — safe to
    /// grab references to other systems there.
    ///
    /// Subclasses hook in via OnManagerAwake() / OnPostInitialize() /
    /// OnManagerDestroyed() so base-class Awake/PostInitialize logic
    /// is never forgotten.
    /// </summary>
    public abstract class Manager : MonoBehaviour, IInitializable
    {
        public virtual int Priority => 0;

        private void Awake()
        {
            LoadingSystem.Instance.Register(this);
            OnManagerAwake();
        }

        private void OnDestroy()
        {
            OnManagerDestroyed();
        }

        /// <summary>Additional Awake hook for subclasses. Must NOT touch other objects here.</summary>
        protected virtual void OnManagerAwake() { }

        /// <summary>Additional OnDestroy hook for subclasses.</summary>
        protected virtual void OnManagerDestroyed() { }

        public abstract IEnumerator Initialize();

        public void PostInitialize()
        {
            OnPostInitialize();
        }

        /// <summary>Implement your own PostInitialize logic here instead of overriding PostInitialize() directly.</summary>
        protected abstract void OnPostInitialize();
    }
}
