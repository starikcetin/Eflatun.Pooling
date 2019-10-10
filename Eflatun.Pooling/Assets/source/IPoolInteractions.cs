namespace Eflatun.Pooling
{
    /// <summary>
    /// Includes essential methods for objects that will be pooled.
    /// </summary>
    public interface IPoolInteractions
    {
        /// <summary>
        /// This is an imitation of Awake() method.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// This will be called just before the GameObject despawns.
        /// </summary>
        void OnDespawn();
    }
}