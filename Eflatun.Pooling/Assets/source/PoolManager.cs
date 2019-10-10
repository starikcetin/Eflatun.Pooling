using System.Collections.Generic;
using UnityEngine;

namespace Eflatun.Pooling
{
    public class PoolManager : SceneSingleton<PoolManager>
    {
        //prevent initialization
        protected PoolManager()
        {
        }

        [SerializeField] private PrefabPool_Inspector[] _allPoolSetups;

        /// <summary>
        /// (Prefab, Pool) Dictionary
        /// </summary>
        private readonly OrderedDictionary<GameObject, PrefabPool> _allPools =
            new OrderedDictionary<GameObject, PrefabPool>();

        /// <summary>
        /// Dictionary of all pools in (Prefab, Pool) format.
        /// </summary>
        public OrderedDictionary<GameObject, PrefabPool> AllPools
        {
            get { return _allPools; }
        }

        private void Awake()
        {
            foreach (var item in _allPoolSetups)
            {
                CreatePool(item.Prefab, item.PrePopulateAmount, item.AutoPopulateAmount);
            }
        }

        /// <summary>
        /// Creates and registers (to <see cref="AllPools"/>) a new <see cref="PrefabPool"/> and returns it.
        /// </summary>
        public PrefabPool CreatePool(GameObject prefab, int prePopulateAmount, int autoPopulateAmount)
        {
            var newPrefabPool = new PrefabPool(prefab, prePopulateAmount, autoPopulateAmount);
            _allPools.Add(prefab, newPrefabPool);
            return newPrefabPool;
        }

        /// <summary>
        /// If <paramref name="prefab"/> has a pool, spawns a pooled object and returns it; otherwise instantiates a new instance and returns it.
        /// </summary>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (_allPools.ContainsKey(prefab))
            {
                PrefabPool pool = _allPools[prefab];
                return pool.Spawn(position, rotation);
            }
            else
            {
                GameObject newObject = Instantiate(prefab, position, rotation) as GameObject;
                return newObject;
            }
        }

        /// <summary>
        /// If <paramref name="toDespawn"/> is a pooled object, despawns it; otherwise destroys it.
        /// </summary>
        public void Despawn(GameObject toDespawn)
        {
            KeyValuePair<GameObject, PrefabPool> foundEntry =
                _allPools.SingleOrDefault(a => a.Value.ActiveObjects.Contains(gameObject));

            if (foundEntry.Key != null)
            {
                PrefabPool pool = foundEntry.Value;
                pool.Despawn(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}