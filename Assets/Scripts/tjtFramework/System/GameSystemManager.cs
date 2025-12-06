using System;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.PublicMono;
using tjtFramework.Singleton;

namespace tjtFramework.GameSystem
{
    /// <summary>
    /// 游戏系统管理器
    /// </summary>
    public class GameSystemManager : MonoSingleton<GameSystemManager>
    {
        private readonly Dictionary<Type, IGameSystem> systems = new();

        public T RegisterSystem<T>() where T : GameSystemBase<T>, new()
        {
            var type = typeof(T);

            if(systems.ContainsKey(type))
            {
                return (T)systems[type];
            }

            var system = new T();
            systems.Add(type, system);

            system.OnInit();
            if (system.needUpdate)
            {
                MonoManager.Instance.AddUpdateListener(system.OnUpdate);
            }
            return system;
        }

        public void UnregisterSystem<T>() where T : GameSystemBase<T>
        {
            var type = typeof(T);
            if (!systems.TryGetValue(type, out var system))
            {
                return;
            }

            system.OnShoutDown();
            systems.Remove(type);
        }

        public T GetSystem<T>() where T : GameSystemBase<T>
        {
            if(systems.TryGetValue(typeof(T), out var system))
            {
                return (T)system;
            }
            return null;
        }

        public override void Destroy()
        {
            base.Destroy();

            if(systems != null && systems.Count > 0)
            {
                foreach( var system in systems.Values)
                {
                    system.OnShoutDown();
                }
            }
            systems.Clear();
        }
    }
}

