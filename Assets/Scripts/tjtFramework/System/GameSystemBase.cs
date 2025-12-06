namespace tjtFramework.GameSystem
{
    public interface IGameSystem
    {
        public bool needUpdate {  get; }
        public void OnInit() { }
        public void OnShoutDown() { }
        public void OnUpdate() { }
    }

    /// <summary>
    /// 游戏系统基类
    /// </summary>
    public abstract class GameSystemBase<T> : IGameSystem where T : GameSystemBase<T>
    {
        public static T Current { get; private set; }

        public abstract bool needUpdate { get;}

        public virtual void OnInit() { }

        /// <summary>
        /// 在具体系统的OnInit中调用，以告知程序该系统已准备好
        /// 当Current != null时为系统未准备好
        /// </summary>
        protected void MarkReady()
        {
            Current = (T)this;
            OnReady();
        }

        protected virtual void OnReady(){ }

        public virtual void OnShoutDown() 
        {
            Current = null;
        }

        public virtual void OnUpdate() { }
    }
}

