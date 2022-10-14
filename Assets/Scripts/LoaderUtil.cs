/**
 * @time 2020-07-21 13:56:23
 * @author cumi
*/

using System;
using System.Collections;
using System.Threading.Tasks;
using Script.Resloader;
using UnityEngine;
using VEngine;
using Object = UnityEngine.Object;

[XLua.LuaCallCSharp]
public class LoaderUtil
{
    public const string PNG = ".png";
    public const string JPG = ".jpg";
    
    #region Object.Instantiate 实例化prefab接口

    public static GameObject SpawnPrefab(GameObject prefab, Vector3 pos, Vector3 scale, Quaternion qua,
        Transform parent = null, bool local = true)
    {
        GameObject go = Object.Instantiate(prefab, parent);
        var transform = go.transform;
        transform.localScale = scale;
        if (local)
        {
            transform.localPosition = pos;
            transform.localRotation = qua;
        }
        else
        {
            transform.position = pos;
            transform.rotation = qua;
        }

        return go;
    }

    public static T SpawnPrefab<T>(T prefab, Vector3 pos, Vector3 scale, Quaternion qua,
        Transform parent = null, bool local = true) where T : MonoBehaviour
    {
        T go = Object.Instantiate(prefab, parent);
        var transform = go.transform;
        transform.localScale = scale;
        if (local)
        {
            transform.localPosition = pos;
            transform.localRotation = qua;
        }
        else
        {
            transform.position = pos;
            transform.rotation = qua;
        }

        return go;
    }

    #endregion

    #region 新同步异步加载接口

    public static T LoadResSync<T>(string path, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return default;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                Asset req = LfResLoader.LoadAssetSync<T>(path);
                t = req?.Get<T>();
                if (t == null)
                {
                    DKInfoData.AddNullAsset(path);
                    return t;
                }
                else
                {
                    DKInfoData.ZeroNullAsset(path);
                }
                LfSystemResMgr.Ins.AddRes(systemId, req);
                LfResPool.Ins.AddResToPool(path, t);
            }

            return t;
        }

        return default;
    }

    public static T LoadResWithLanguageSync<T>(string path, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return default;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                Asset req = LfResLoader.LoadAssetWithLanguageSync<T>(path);
                t = req?.Get<T>();
                if (t == null)
                {
                    DKInfoData.AddNullAsset(path);
                }
                else
                {
                    DKInfoData.ZeroNullAsset(path);
                    LfResPool.Ins.AddResToPool(path, t);
                }

                if (req != null)
                    LfSystemResMgr.Ins.AddRes(systemId, req);
            }

            return t;
        }

        return default;
    }

    public static T LoadResWithQualitySync<T>(string path, long systemId = 0,
        LfResQualityType qualityType = LfResQualityType.NQ) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return default;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                Asset req = LfResLoader.LoadAssetWithQualitySync<T>(path, qualityType);
                t = req?.Get<T>();
                if (t == null)
                {
                    DKInfoData.AddNullAsset(path);
                }
                else
                {
                    DKInfoData.ZeroNullAsset(path);
                    LfResPool.Ins.AddResToPool(path, t);
                }

                if (req != null)
                    LfSystemResMgr.Ins.AddRes(systemId, req);
            }

            return t;
        }

        return default;
    }

    public static void LoadResAsync<T>(string path, Action<T> complete, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            complete?.Invoke(default);
            return;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                LfResLoader.LoadAssetAsync<T>(path, (req) =>
                {
                    T asset = req?.Get<T>();
                    if (asset == null)
                    {
                        DKInfoData.AddNullAsset(path);
                    }
                    else
                    {
                        DKInfoData.ZeroNullAsset(path);
                        LfResPool.Ins.AddResToPool(path, asset);
                    }

                    if (req != null)
                        LfSystemResMgr.Ins.AddRes(systemId, req);

                    complete?.Invoke(asset);
                });
            }
            else
            {
                complete?.Invoke(t);
            }
        }
        else
        {
            complete?.Invoke(default);
        }
    }

    public static IEnumerator LoadResAsync_IE<T>(string path, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            yield return default;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                Asset req = LfResLoader.LoadAssetAsync<T>(path);
                if (req == null)
                {
                    DKInfoData.AddNullAsset(path);
                }
                else
                {
                    yield return req;
                    t = req.Get<T>();
                    if (t != null)
                    {
                        DKInfoData.ZeroNullAsset(path);
                        LfResPool.Ins.AddResToPool(path, t);
                    }
                    else
                    {
                        DKInfoData.AddNullAsset(path);
                    }
                    LfSystemResMgr.Ins.AddRes(systemId, req);
                }
            }
            yield return t;
        }
        else
        {
            yield return default;
        }
    }

    public static void LoadResWithLanguageAsync<T>(string path, Action<T> complete, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            complete?.Invoke(default);
            return;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                LfResLoader.LoadAssetWithLanguageAsync<T>(path, (req) =>
                {
                    T asset = req?.Get<T>();
                    if (asset == null)
                    {
                        DKInfoData.AddNullAsset(path);
                    }
                    else
                    {
                        DKInfoData.ZeroNullAsset(path);
                        LfResPool.Ins.AddResToPool(path, asset);
                    }

                    if (req != null)
                        LfSystemResMgr.Ins.AddRes(systemId, req);

                    complete?.Invoke(asset);
                });
            }
            else
            {
                complete?.Invoke(t);
            }
        }
        else
        {
            complete?.Invoke(default);
        }
    }

    public static void LoadResWithQualityAsync<T>(string path, Action<T> complete, long systemId = 0,
        LfResQualityType qualityType = LfResQualityType.NQ) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            complete?.Invoke(default);
            return;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            T t = LfResPool.Ins.GetResFromPool<T>(path);
            if (t == null)
            {
                LfResLoader.LoadAssetWithQualityAsync<T>(path, qualityType, (req) =>
                {
                    T asset = req?.Get<T>();
                    if (asset == null)
                    {
                        DKInfoData.AddNullAsset(path);
                    }
                    else
                    {
                        DKInfoData.ZeroNullAsset(path);
                        LfResPool.Ins.AddResToPool(path, asset);
                    }

                    if (req != null)
                        LfSystemResMgr.Ins.AddRes(systemId, req);

                    complete?.Invoke(asset);
                });
            }
            else
            {
                complete?.Invoke(t);
            }
        }
        else
        {
            complete?.Invoke(default);
        }
    }

    #endregion

    #region 新接口-同步加载Sprite

    public static Sprite LoadSpriteSync_png(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadSpriteSync(path + PNG, systemId);
    }

    public static Sprite LoadSpriteSync_jpg(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadSpriteSync(path + JPG, systemId);
    }

    private static Sprite LoadSpriteSync(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            Sprite t = LfResPool.Ins.GetResFromPool<Sprite>(path);
            if (t == null)
            {
                Asset req = LfResLoader.LoadAssetSync<Sprite>(path);
                t = req?.Get<Sprite>();
                if (t == null)
                {
                    DKInfoData.AddNullAsset(path);
                    return t;
                }
                else
                {
                    DKInfoData.ZeroNullAsset(path);
                }
                LfSystemResMgr.Ins.AddRes(systemId, req);
                LfResPool.Ins.AddResToPool(path, t);
            }

            return t;
        }

        return default;
    }

    #endregion

    #region 新接口-异步加载Sprite

    public static void LoadSpriteAsync_png(string path, Action<Sprite> complete, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            complete?.Invoke(null);
            return;
        }

        LoadSpriteAsync(path + ".png", complete);
    }

    public static void LoadSpriteAsync_jpg(string path, Action<Sprite> complete, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            complete?.Invoke(null);
            return;
        }

        LoadSpriteAsync(path + ".jpg", complete);
    }

    private static void LoadSpriteAsync(string path, Action<Sprite> complete, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            complete?.Invoke(null);
            return;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            Sprite t = LfResPool.Ins.GetResFromPool<Sprite>(path);
            if (t == null)
            {
                LfResLoader.LoadAssetAsync<Sprite>(path, (req) =>
                {
                    Sprite sprite = req?.Get<Sprite>();
                    if (sprite == null)
                    {
                        DKInfoData.AddNullAsset(path);
                    }
                    else
                    {
                        DKInfoData.ZeroNullAsset(path);
                        LfResPool.Ins.AddResToPool(path, sprite);
                    }

                    if (req != null)
                        LfSystemResMgr.Ins.AddRes(systemId, req);

                    complete?.Invoke(sprite);
                });
            }
            else
            {
                complete?.Invoke(t);
            }
        }
    }

    #endregion

    #region 新接口-实例化接口

    public static GameObject InstantiateSync(string path, Transform parent = null, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string pathWithExt = path + ".prefab";
        GameObject go = LfResPool.Ins.GetResFromPool<GameObject>(pathWithExt);

        if (go == null)
        {
            if (!DKInfoData.IsNullAsset(pathWithExt))
            {
                Asset req = LfResLoader.LoadAssetSync<GameObject>(pathWithExt);
                var o = req?.Get<GameObject>();
                if (o == null || o == default)
                {
                    DKInfoData.AddNullAsset(pathWithExt);
                    return null;
                }

                DKInfoData.ZeroNullAsset(pathWithExt);
                LfSystemResMgr.Ins.AddRes(systemId, req);
                go = Object.Instantiate(o, parent);
                LfResPool.Ins.AddResToPool(pathWithExt, go);
            }
        }
        else
        {
            go.transform.SetParent(parent);
        }

        return go;
    }

    public static GameObject InstantiateWithQualitySync(string path, Transform parent = null,
        LfResQualityType qualityType = LfResQualityType.NQ, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string pathWithExt = path + ".prefab";
        GameObject go = LfResPool.Ins.GetResFromPool<GameObject>(pathWithExt);

        if (go == null)
        {
            if (!DKInfoData.IsNullAsset(pathWithExt))
            {
                Asset req = LfResLoader.LoadAssetWithQualitySync<GameObject>(pathWithExt, qualityType);
                var o = req?.Get<GameObject>();
                if (o == null || o == default)
                {
                    DKInfoData.AddNullAsset(pathWithExt);
                    return null;
                }

                DKInfoData.ZeroNullAsset(pathWithExt);
                LfSystemResMgr.Ins.AddRes(systemId, req);
                go = Object.Instantiate(o, parent);
                LfResPool.Ins.AddResToPool(pathWithExt, go);
            }
        }
        else
        {
            go.transform.SetParent(parent);
        }

        return go;
    }

    public static void InstantiateAsync(string path, Transform parent = null, Action<GameObject> complete = null,
        long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string pathWithExt = path + ".prefab";
        GameObject go = LfResPool.Ins.GetResFromPool<GameObject>(pathWithExt);

        if (go == null)
        {
            if (!DKInfoData.IsNullAsset(pathWithExt))
            {
                LfResLoader.LoadAssetAsync<GameObject>(pathWithExt, (req) =>
                {
                    GameObject o = req?.Get<GameObject>();
                    if (o == null || o == default)
                    {
                        DKInfoData.AddNullAsset(pathWithExt);
                        complete?.Invoke(null);
                        return;
                    }
                    DKInfoData.ZeroNullAsset(pathWithExt);
                    LfSystemResMgr.Ins.AddRes(systemId, req);

                    go = Object.Instantiate(o, parent);
                    LfResPool.Ins.AddResToPool(pathWithExt, go);
                    complete?.Invoke(go);
                });
            }
            else
            {
                complete?.Invoke(null);
            }
        }
        else
        {
            complete?.Invoke(go);
        }
    }

    public static void InstantiateWithQualityAsync(string path, Transform parent = null, Action<GameObject> complete = null,
        LfResQualityType qualityType = LfResQualityType.NQ, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string pathWithExt = path + ".prefab";
        GameObject go = LfResPool.Ins.GetResFromPool<GameObject>(pathWithExt);

        if (go == null)
        {
            if (!DKInfoData.IsNullAsset(pathWithExt))
            {
                LfResLoader.LoadAssetWithQualityAsync<GameObject>(pathWithExt, qualityType, (req) =>
                {
                    GameObject o = req?.Get<GameObject>();
                    if (o == null || o == default)
                    {
                        DKInfoData.AddNullAsset(pathWithExt);
                        complete?.Invoke(null);
                        return;
                    }

                    DKInfoData.ZeroNullAsset(pathWithExt);
                    LfSystemResMgr.Ins.AddRes(systemId, req);

                    go = Object.Instantiate(o, parent);
                    LfResPool.Ins.AddResToPool(pathWithExt, go);
                    complete?.Invoke(go);
                });
            }
            else
            {
                complete?.Invoke(null);
            }
        }
        else
        {
            complete?.Invoke(go);
        }
    }

    #endregion

    #region Load with async..await 由于系统升级，为兼容之前的接口，以下as的加载接口都换成了同步接口，有异步加载接口需求的请看上面的一批新接口

    #region 实例化的接口 由于系统升级，为兼容之前的接口，以下as的加载接口都换成了同步接口，有异步加载接口需求的请看上面的一批新接口

    public static async Task<T> SpawnObjectAsync<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return default;
        }

        GameObject go = InstantiateSync(path);

        T t = default;
        if (go != null)
        {
            t = go.GetComponent<T>();
        }

        return t;
    }

    public static async Task<GameObject> SpawnGameObjectAsync(string path, Vector3 pos, Vector3 scale, Quaternion qua,
        Transform parent = null, bool local = true, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        GameObject go = InstantiateSync(path, parent, systemId);

        if (go != null)
        {
            var transform = go.transform;
            transform.localScale = scale;
            if (local)
            {
                transform.localPosition = pos;
                transform.localRotation = qua;
            }
            else
            {
                transform.position = pos;
                transform.rotation = qua;
            }
        }

        return go;
    }

    public static async Task<GameObject> SpawnGameObjectAsync(string path, Transform parent = null, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return InstantiateSync(path, parent, systemId);
    }

    public static bool DestroyGameObject(GameObject go)
    {
        Object.Destroy(go);
        return true;
    }

    #endregion

    public static async Task<GameObject> LoadPrefabAsync(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResSync<GameObject>(path + ".prefab", systemId);
    }

    public static async Task<Sprite> LoadSpriteAsync(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadSpriteSync_png(path, systemId);
    }

    public static async Task<AudioClip> LoadAudioAsync(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        return LoadResSync<AudioClip>(path + ".mp3", systemId);
    }

    public static async Task<AudioClip> LoadAudioWithLanguageAsync(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResWithLanguageSync<AudioClip>(path + ".mp3", systemId);
    }

    public static async Task<Material> LoadMaterialAsync(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResWithLanguageSync<Material>(path + ".mat", systemId);
    }

    public static async Task<Material> LoadMaterialAsync2(string path, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        return LoadResSync<Material>(path + ".mat", systemId);
    }
    public static async Task<TextAsset> LoadExcelAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        Asset excel = Asset.Load(path, typeof(TextAsset));
        return excel.Get<TextAsset>();
    }

    public static TextAsset LoadExcelSync(string excelName)
    {
        if (string.IsNullOrEmpty(excelName))
        {
            return null;
        }

        Asset req = LfResLoader.LoadAssetSync<TextAsset>($"Excel/{AppConfig.Config.Language}/{excelName}.bytes");
        if (req != null)
        {
            TextAsset textAsset = req.Get<TextAsset>();
            req.Release();
            return textAsset;
        }

        return null;
    }

    public static async Task<T> LoadResAsync<T>(string path, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResSync<T>(path, systemId);
    }

    //去多语言目录加载资源
    public static async Task<T> LoadResWithLanguageAsync<T>(string path, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResWithLanguageSync<T>(path, systemId);
    }

    //加载不同品质的资源，默认前往Middle目录加载
    public static async Task<T> LoadResWithQualityAsync<T>(string path, LfResQualityType qualityType, long systemId = 0)
        where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResWithQualitySync<T>(path, systemId, qualityType);
    }

    //根据系统品质加载资源
    public static async Task<T> LoadResWithQualityAsync<T>(string path, long systemId = 0)
        where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return LoadResWithQualitySync<T>(path, systemId);
    }

    //根据系统品质加载资源
    public static async Task<GameObject> SpawnGameObjectWithQualityAsync(string path,long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        GameObject prefab = LoadResWithQualitySync<GameObject>(path + ".prefab", systemId);
        if (prefab != null)
        {
            GameObject go = Object.Instantiate(prefab);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            return go;
        }

        return null;
    }

    public static async Task<GameObject> SpawnGameObjectWithQualityAsync(string path, Vector3 pos, Vector3 scale,
        Quaternion qua, Transform parent = null, bool local = true, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        GameObject prefab = LoadResWithQualitySync<GameObject>(path + ".prefab", systemId);

        if (prefab != null)
        {
            GameObject go = Object.Instantiate(prefab);

            if (parent != null)
            {
                go.transform.SetParent(parent);
            }

            var transform = go.transform;
            transform.localScale = scale;
            if (local)
            {
                transform.localPosition = pos;
                transform.localRotation = qua;
            }
            else
            {
                transform.position = pos;
                transform.rotation = qua;
            }

            return go;
        }

        return null;
    }

    public static async Task<GameObject> SpawnGameObjectWithQualityAsync(LfResQualityType qualityType, string path,
        Vector3 pos, Vector3 scale, Quaternion qua, Transform parent = null, bool local = true, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        GameObject prefab = LoadResWithQualitySync<GameObject>(path + ".prefab", systemId, qualityType);

        if (prefab != null)
        {
            GameObject go = Object.Instantiate(prefab);

            if (parent != null)
            {
                go.transform.SetParent(parent);
            }

            var transform = go.transform;
            transform.localScale = scale;
            if (local)
            {
                transform.localPosition = pos;
                transform.localRotation = qua;
            }
            else
            {
                transform.position = pos;
                transform.rotation = qua;
            }

            return go;
        }

        return null;
    }

    #endregion

    #region load with callback 由于系统升级，为兼容之前的接口，以下as的加载接口都换成了同步接口，有异步加载接口的请看上面的一批新接口

    public static void SpawnGameObjectAsyncWithCallback(string path, Action<GameObject> callback, long id = 0)
    {
        LoadResAsyncWithCallback<GameObject>(path + ".prefab", (o) =>
        {
            GameObject go = null;
            if (o != null)
            {
                go = Object.Instantiate(o);
            }

            callback?.Invoke(go);
        }, id);
    }

    /// <summary>
    /// callback 不要嵌套太多层，尽量使用await
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    /// <param name="systemId"></param>
    /// <typeparam name="T"></typeparam>
    public static void LoadResAsyncWithCallback<T>(string path, Action<T> callback, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            callback(default);
            return;
        }

        LoadResAsync(path, callback, systemId);
    }

    public static void LoadResWithLanguageAsyncWithCallback<T>(string path, Action<T> callback, long systemId = 0) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            callback(default);
            return;
        }

        LoadResWithLanguageAsync(path, callback, systemId);
    }

    #endregion

    #region  This Interface For Lua


    public static Object LoadResSyncForLua(string path, Type T, long systemId = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            return default;
        }

        if (!DKInfoData.IsNullAsset(path))
        {
            Object  t = LfResPool.Ins.GetResFromPool(path);
            if (t == null)
            {
                Asset req = LfResLoader.LoadAssetSync (path, T);
                t = req?.asset;
                if (t == null)
                {
                    DKInfoData.AddNullAsset(path);
                    return t;
                }
                else
                {
                    DKInfoData.ZeroNullAsset(path);
                }
                LfSystemResMgr.Ins.AddRes(systemId, req);
                LfResPool.Ins.AddResToPool(path, t);
            }

            return t;
        }

        return default;
    }



    #endregion
}