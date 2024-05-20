using System.Collections.Generic;
using UnityEngine;

namespace HietakissaUtils
{
    /// <summary>
    /// WIP, DO NOT USE. BROKEN. WILL BE REFACTORED IN A FUTURE VERSION. 
    /// </summary>
    public class HKPool
    {
        Queue<GameObject> poolQueue;

        GameObject poolObject;
        Transform parent;

        public int GrowSize
        {
            get => GrowSize;
            set
            {
                GrowSize = Mathf.Clamp(value, 1, 1000);
            }
        }

        public int MaxSize
        {
            get => MaxSize;
            set => Mathf.Clamp(value, 1, 1000);
        }
        public int currentSize;
        public int objectsAvailable;

        public HKPool(GameObject poolObject, Transform parent, int growSize = 10, int maxSize = 1000)
        {
            poolQueue = new Queue<GameObject>();

            this.poolObject = poolObject;
            this.parent = parent;
            this.GrowSize = growSize;
            this.MaxSize = maxSize;

            currentSize = 0;
            objectsAvailable = 0;

            GrowPool();
        }


        public GameObject Get()
        {
            if (objectsAvailable == 0) GrowPool();
            if (objectsAvailable == 0) return null;

            GameObject getObject = poolQueue.Dequeue();
            objectsAvailable--;

            getObject.SetActive(true);
            return getObject;
        }
        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            GameObject getObject = Get();

            if (getObject == null) return null;

            getObject.transform.position = position;
            getObject.transform.rotation = rotation;

            return getObject;
        }

        public void ReturnObject(GameObject returnObject)
        {
            returnObject.SetActive(false);
            if (!poolQueue.Contains(returnObject)) poolQueue.Enqueue(returnObject);

            objectsAvailable++;
        }

        public bool GrowPool()
        {
            if (currentSize >= MaxSize) return false;

            for (int i = 0; i < GrowSize && currentSize < MaxSize; i++)
            {
                GameObject newObject = MonoBehaviour.Instantiate(poolObject, parent.position, Quaternion.identity);
                newObject.transform.parent = parent;

                newObject.SetActive(false);
                poolQueue.Enqueue(newObject);

                currentSize++;
                objectsAvailable++;
            }

            return true;
        }

        public void EmptyPool()
        {
            currentSize = 0;
            objectsAvailable = 0;

            poolQueue.Clear();
        }
    }
}