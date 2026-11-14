using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Abstract;
using Slafurry.Utils.VFX;


namespace Slafurry.Systems.VFX
{
    public static class VFX
    {
        public static void Play(string key, Vector3 position)
        {
            VFXSystem.Instance.Play(key, position);
        }

        public static void Play(string key, Vector3 position, Quaternion rotation)
        {
            VFXSystem.Instance.Play(key, position, rotation);
        }
    }

    public class VFXSystem : GameSystem<VFXSystem>
    {
        [SerializeField] private VFXEntry[] vfxEntries;

        private readonly Dictionary<string, VFXEntry> _entries = new();

        public override IEnumerator Initialize()
        {
            foreach (var entry in vfxEntries)
            {
                if (entry == null || entry.prefab == null) continue;
                _entries[entry.key] = entry;
            }

            yield return null;
        }

        public override void PostInitialize() { }
        protected override void OnSingletonAwake() => base.OnSingletonAwake();

        public VFXCleaner Play(string key, Vector3 position, Quaternion rotation = default)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                Debug.LogWarning($"[VFXSystem] VFX '{key}' not found.");
                return null;
            }

            var instance = Object.Instantiate(entry.prefab, position, rotation);
            return instance;
        }
    }
}
