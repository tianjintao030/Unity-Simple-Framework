using System.Linq;
using System.Reflection;
using UnityEngine;
using tjtFramework.Singleton;
using Cysharp.Threading.Tasks;

public class AssemblyLauncher : MonoSingleton<AssemblyLauncher>
{
    private Assembly gameAssembly;

    public async UniTask Launch()
    {
        await AssetLoader.Instance.LoadConfigData();

        LoadAssembly();
    }

    public void LoadAssembly()
    {
        if(!AssetLoader.InEditor || AssetLoader.IsAssetBundleMode)
        {
            var gameDllTextAsset = AssetLoader.Instance.LoadAsset<TextAsset>("Src/Game.dll.bytes", gameObject);
            gameAssembly = Assembly.Load(gameDllTextAsset.bytes);
        }
        else
        {
            gameAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Game");
        }

        Debug.Log("º”‘ÿGameAssemblyÕÍ≥…");
    }
}
