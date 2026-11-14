using UnityEngine;

namespace Slafurry.Utils.VFX
{
    /// <summary>
    /// Attach this to the root of any Animator-driven VFX prefab
    /// (explosion, hit spark, muzzle flash, etc).
    ///
    /// Setup:
    /// 1. Add this component to the VFX prefab root.
    /// 2. On the Animator's last keyframe, add an Animation Event that
    ///    calls Clean() (no parameters needed).
    /// 3. Spawn the VFX through VFXSystem.Play(key, position).
    ///
    /// Clean() always destroys the GameObject — no pooling is used.
    /// </summary>
    public class VFXCleaner : MonoBehaviour
    {
        /// <summary>
        /// Call this from an Animation Event on the last frame of the VFX
        /// animation, or from a ParticleSystem "Stop Action" callback.
        /// </summary>
        public void Clean()
        {
            Destroy(gameObject);
        }
    }
}
