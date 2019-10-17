using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Eflatun.Pooling
{
    /// <summary>
    /// Pool for prefabs.
    /// </summary>
    public sealed class PrefabPool
    {
        public GameObject Prefab { get; private set; }
        public int AutoPopulateAmount { get; private set; }

        public GameObject Holder { get; private set; }
        public List<GameObject> ActiveObjects { get; private set; }
        public List<GameObject> InactiveObjects { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefabPool"/> class.
        /// </summary>
        /// <param name="prefab">Prefab that this pool will use.</param>
        /// <param name="prePopulateAmount">Populates pool with given amount instantly.</param>
        /// <param name="autoPopulateAmount">The amount to populate this pool automatically when there are no inactive objects left.</param>
        public PrefabPool(GameObject prefab, int prePopulateAmount, int autoPopulateAmount)
        {
            ActiveObjects = new List<GameObject>();
            InactiveObjects = new List<GameObject>();
            AutoPopulateAmount = autoPopulateAmount;
            Prefab = prefab;

            Holder = new GameObject(string.Format("Pool of {0}", prefab.name));
            Holder.transform.parent = PoolManager.Instance.gameObject.transform;

            Populate(prePopulateAmount);
        }

        /// <summary>
        /// Spawn a GameObject and returns it.
        /// </summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject spawned = _Spawn(position, rotation);
            NotifySpawn(spawned);
            return spawned;
        }

        /// <summary>
        /// Despawn a GameObject.
        /// </summary>
        public void Despawn(GameObject gameObject)
        {
            _Despawn(gameObject);
            NotifyDespawn(gameObject);
        }

        private GameObject _Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject toSpawn = GetInactiveOrInstantiate();
            toSpawn.transform.parent = null;
            toSpawn.transform.position = position;
            toSpawn.transform.rotation = rotation;
            toSpawn.SetActive(true);

            InactiveObjects.Remove(toSpawn);
            ActiveObjects.Add(toSpawn);

            PopulateIfRequired();

            return toSpawn;
        }

        private void _Despawn(GameObject toDespawn)
        {
            toDespawn.SetActive(false);
            toDespawn.transform.parent = Holder.transform;

            ActiveObjects.Remove(toDespawn);
            InactiveObjects.Add(toDespawn);
        }

        /// <summary>
        /// Calls all <see cref="IPoolInteractions.OnSpawn"/> methods on given GameObject.
        /// </summary>
        private static void NotifySpawn(GameObject spawned)
        {
            var interfaces = spawned.GetComponents<IPoolInteractions>();
            foreach (var item in interfaces)
            {
                item.OnSpawn();
            }
        }

        /// <summary>
        /// Calls all <see cref="IPoolInteractions.OnDespawn"/> methods on given GameObject.
        /// </summary>
        private static void NotifyDespawn(GameObject despawned)
        {
            var interfaces = despawned.GetComponents<IPoolInteractions>();
            foreach (var item in interfaces)
            {
                item.OnDespawn();
            }
        }

        /// <summary>
        /// Populates the pool if there are no inactive objects left.
        /// </summary>
        private void PopulateIfRequired()
        {
            if (!InactiveObjects.Any()) //if there are no inactive objects left...
            {
                Populate(AutoPopulateAmount); //...populate the pool.
            }
        }

        /// <summary>
        /// Gets the first inactivate object in the list. If there is no available gameObject, instantiates a new one and returns it.
        /// </summary>
        private GameObject GetInactiveOrInstantiate()
        {
            return InactiveObjects.Any() ? InactiveObjects[0] : PopulateSingle();
        }

        #region Maintenance

        /// <summary>
        /// Instantiates given amount of new GameObjects and adds them to the pool.
        /// </summary>
        private void Populate(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PopulateSingle();
            }
        }

        /// <summary>
        /// Instantiates a single GameObject, adds it to the pool and returns it.
        /// </summary>
        private GameObject PopulateSingle()
        {
            GameObject newObject = InstantiateSingle();
            Bind(newObject, true);
            return newObject;
        }

        /// <summary>
        /// Adds the given GameObject to this pool. If despawn is true, also despawns the object.
        /// </summary>
        private void Bind(GameObject gameObject, bool despawn)
        {
            ActiveObjects.Add(gameObject);

            if (despawn)
            {
                Despawn(gameObject);
            }
        }

        /// <summary>
        /// Instantiates a single instance of Prefab and returns it.
        /// </summary>
        private GameObject InstantiateSingle()
        {
            return Object.Instantiate(Prefab);
        }

        #endregion Maintenance
    }
}