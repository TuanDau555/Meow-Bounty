using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    #region Parameters
    [SerializeField] private bool _addToDontDestroyOnLoad = false;

    // This will contain all the object pools
    private GameObject _emptyHolder;

    // This contain all the normal game object is created 
    private static GameObject _gameObjectEmpty;

    // This contain all the sound fx is created 
    private static GameObject _soundFXEmpty;

    // Pool from the prefab
    private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;

    // Map Instance (clone) to Prefab
    // to know which pool we need to return
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public static PoolType PoolingType;

    #endregion

    #region Execute

    private void Awake()
    {
        _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetUpEmties();
    }

    #endregion

    #region Init

    /// <summary>
    /// Create the Empty GameObject to cotain the pool's object
    /// </summary>
    private void SetUpEmties()
    {
        _emptyHolder = new GameObject("Object Pools");

        _gameObjectEmpty = new GameObject("Game Objects Pool");
        _gameObjectEmpty.transform.SetParent(_emptyHolder.transform);

        _soundFXEmpty = new GameObject("Sound FX Object Pool");
        _soundFXEmpty.transform.SetParent(_emptyHolder.transform);

        // If you want the object alive through use could use _addToDontDestroyOnLoad parameters
    }

    #endregion

    #region Create Pool

    /// <summary>
    /// Create pool for Prefab at default spawn
    /// </summary>
    /// <param name="poolType">Game object at default</param>
    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, pos, rot, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnRealeaseObject,
            actionOnDestroy: OnDestroyObject
        );
        
        _objectPools.Add(prefab, pool);
    }

    /// <summary>
    /// Create pool for Prefab (spawn in the parent)
    /// </summary>
    /// <param name="poolType">Game object at default</param>
    private static void CreatePool(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, parent, rot, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnRealeaseObject,
            actionOnDestroy: OnDestroyObject
        );
        
        _objectPools.Add(prefab, pool);
    }

    #endregion

    #region Create Object
    
    /// <summary>
    /// Create a new object instance
    /// </summary>
    /// <param name="poolType">Game object at default</param>
    /// <returns>obj to create</returns>
    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, pos, rot);

        prefab.SetActive(true);

        GameObject parentObject = SetParentObject(poolType);
        obj.transform.SetParent(parentObject.transform);

        return obj;
    }


    private static GameObject CreateObject(GameObject prefab, Transform parent, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, parent);

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = rot;
        obj.transform.localScale = Vector3.one;
        
        prefab.SetActive(true);

        return obj;
    }
    
    #endregion
    
    #region Pool Callback
    
    private static void OnGetObject(GameObject obj)
    {
        // Optional logic when we get the object
    }

    private static void OnRealeaseObject(GameObject obj)
    {
        // Prevent Audio source still play when return
        AudioSource audio = obj.GetComponent<AudioSource>();
        if(audio != null)
        {
            audio.Stop();
            audio.clip = null;
        }
        
        obj.SetActive(false);
    }

    private static void OnDestroyObject(GameObject obj)
    {
        if (_cloneToPrefabMap.ContainsKey(obj))
        {
            _cloneToPrefabMap.Remove(obj);
        }
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.GameObjects:
                return _gameObjectEmpty;
            
            case PoolType.SoundFX:
                return _soundFXEmpty;
            
            default:
                return null;
        }
    }

    /// <summary>
    /// Return the object to pool instead of destroy it 
    /// </summary>
    /// <param name="poolType">Game object at default</param>
    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if(_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParentObject(poolType);

            if(obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform);
            }

            if(_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
        }
    }

    #endregion
    
    #region Spawn object
    
    /// <summary>
    /// Spawn object
    /// </summary>
    /// <returns>Return expect component</returns>
    private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) 
    where T : Object
    {
        if (!_objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
        }

        GameObject obj = _objectPools[objectToSpawn].Get();

        if(obj != null)
        {
            if (!_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRotation;
            obj.SetActive(true);

            if(typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();
            if(component == null)
            {
                Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}");
                return null;
            }

            return component;
        }

        return null;
    }

    private static T SpawnObject<T>(GameObject objectToSpawn, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) 
    where T : Object
    {
        if (!_objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, parent, spawnRotation, poolType);
        }

        GameObject obj = _objectPools[objectToSpawn].Get();

        if(obj != null)
        {
            // Store mapping clone for prefab to know which pool to return
            if (!_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = spawnRotation;
            obj.SetActive(true);

            if(typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();
            if(component == null)
            {
                Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}");
                return null;
            }

            return component;
        }

        return null;
    }

    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) 
    where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    }

    public static T SpawnObject<T>(T typePrefab, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) 
    where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, parent, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, parent, spawnRotation, poolType);
    }

    #endregion

}

// Note: if you add more categories, make sure you add them in the enum
public enum PoolType
{
    GameObjects,
    SoundFX
}