using System.Collections.Generic;
using UnityEngine;
using System;

namespace HietakissaUtils.Pooling
{
    public class ObjectPool<TPoolObject>
    {
        Func<TPoolObject> createPooledObject;
        Action<TPoolObject> destroyPooledObject;

        Action<TPoolObject> onReturnToPool;
        Action<TPoolObject> onGetFromPool;

        public readonly List<TPoolObject> availableObjects = new List<TPoolObject>();
        public readonly TPoolObject[] pooledObjects;
        
        public int PooledObjectCount { get; private set; }
        public readonly int MaxSize;
        public readonly int GrowSize;

        int accessIndex;

        public ObjectPool(Action<TPoolObject> onReturnToPool, Action<TPoolObject> onGetFromPool, Func<TPoolObject> createPooledObject, int maxSize = 50, int growSize = 5)
        {
            this.onReturnToPool = onReturnToPool;
            this.onGetFromPool = onGetFromPool;
            this.createPooledObject = createPooledObject;

            pooledObjects = new TPoolObject[maxSize];

            MaxSize = maxSize;
            GrowSize = growSize;
        }

        public TPoolObject Get()
        {
            if (availableObjects.Count == 0)
            {
                if (PooledObjectCount < MaxSize) GrowPool(GrowSize);
                else
                {
                    TPoolObject poolObject = pooledObjects[accessIndex];
                    accessIndex = (accessIndex + 1) % MaxSize;
                    onReturnToPool(poolObject);
                    onGetFromPool(poolObject);
                    return poolObject;
                }
            }

            TPoolObject retrievedObject = availableObjects[^1];
            availableObjects.RemoveAt(availableObjects.Count - 1);
            onGetFromPool(retrievedObject);
            return retrievedObject;
        }
        public void Return(TPoolObject poolObject)
        {
            onReturnToPool(poolObject);
            availableObjects.Add(poolObject);
        }
        public void GrowPool(int objectsToCreate)
        {
            objectsToCreate = Mathf.Min(MaxSize - PooledObjectCount, GrowSize);
            for (int i = 0; i < objectsToCreate; i++)
            {
                TPoolObject createdObject = createPooledObject();
                availableObjects.Add(createdObject);
                pooledObjects[PooledObjectCount + i] = createdObject;
            }

            PooledObjectCount += objectsToCreate;
        }

        public void EmptyPool() => ShrinkPool(PooledObjectCount);
        public void SetDestroyDelegate(Action<TPoolObject> destroyPooledObject) => this.destroyPooledObject = destroyPooledObject;
        public void ShrinkPool(int shrinkCount)
        {
            shrinkCount = Mathf.Min(shrinkCount, PooledObjectCount);
            for (int i = shrinkCount - 1; i >= 0; i--)
            {
                destroyPooledObject(pooledObjects[i]);
                pooledObjects[i] = default;
            }
            PooledObjectCount -= shrinkCount;
        }
    }
}