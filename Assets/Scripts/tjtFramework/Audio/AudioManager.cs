using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using tjtFramework.Singleton;
using tjtFramework.PublicMono;
using tjtFramework.Pool;

namespace tjtFramework.Audio
{
    public class AudioManager : NoMonoSingleton<AudioManager>
    {
        private AudioSource back_music = null;
        private float back_music_volume = 0.5f;

        private List<AudioSource> sound_list = new List<AudioSource>();
        private float sound_volume = 0.5f;
        private Transform sound_parent = null;

        public AudioManager()
        {
            if (sound_parent == null)
                sound_parent = new GameObject("Sounds").transform;

            MonoManager.Instance.AddFixedUpdateListener(CheckOverSound);
        }

        /// <summary>
        /// 移除播放完毕的音效组件
        /// </summary>
        private void CheckOverSound()
        {
            //为避免边遍历边移除，采用逆向遍历
            for (int i = sound_list.Count - 1; i >= 0; --i)
            {
                if (!sound_list[i].isPlaying)
                {
                    sound_list[i].Stop();
                    sound_list[i].clip = null;
                    GameObjectPool.Instance.CollectObject(sound_list[i].gameObject);
                    sound_list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="name"></param>
        public void PlayBackMusic(string name)
        {
            if (back_music == null)
            {
                GameObject go = new GameObject("BackMusic");
                GameObject.DontDestroyOnLoad(go);
                back_music = go.AddComponent<AudioSource>();
            }

            //ABResManager.Instance.LoadResAsync<AudioClip>("music", name, (clip) =>
            //  {
            //      back_music.clip = clip;
            //      back_music.loop = true;
            //      back_music.volume = back_music_volume;
            //      back_music.Play();
            //  });
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        /// <param name="name"></param>
        public void StopBackMusic(string name)
        {
            if (back_music == null)
                return;

            back_music.Stop();
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        /// <param name="name"></param>
        public void PauseBackMusic(string name)
        {
            if (back_music == null)
                return;

            back_music.Pause();
        }

        /// <summary>
        /// 改变背景音乐音量
        /// </summary>
        /// <param name="volume"></param>
        public void ChangeBackMusicVolume(float volume)
        {
            back_music_volume = volume;

            if (back_music != null)
            {
                back_music.volume = back_music_volume;
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="isSync">是否同步加载</param>
        /// <param name="callBack"></param>
        public void PlaySound(string name, bool isLoop = false, bool isSync = false, UnityAction<AudioSource> callBack = null)
        {
            //ABResManager.Instance.LoadResAsync<AudioClip>("sound", name, (clip) =>
            //  {
            //      GameObject go = GameObjectPool.Instance.CreateObject("soundObj",sound_parent);
            //      AudioSource source = go.GetComponent<AudioSource>();
            //      if (source == null)
            //          source = go.AddComponent<AudioSource>();

            //      source.Stop();
            //      source.clip = clip;
            //      source.loop = isLoop;
            //      source.volume = sound_volume;
            //      source.Play();

            //      if (!sound_list.Contains(source))
            //          sound_list.Add(source);

            //      callBack?.Invoke(source);
            //  }, isSync);
        }

        /// <summary>
        /// 停止播放音效
        /// </summary>
        /// <param name="source"></param>
        public void StopSound(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                sound_list.Remove(source);
                GameObjectPool.Instance.CollectObject(source.gameObject);
            }
        }

        /// <summary>
        /// 改变音效音量
        /// </summary>
        /// <param name="volume"></param>
        public void ChangeSoundVolume(float volume)
        {
            sound_volume = volume;

            for (int i = 0; i < sound_list.Count; i++)
            {
                sound_list[i].volume = sound_volume;
            }
        }

        /// <summary>
        /// 过场景时在清理缓存池之前调用
        /// </summary>
        public void ClearSounds()
        {
            for (int i = 0; i < sound_list.Count; i++)
            {
                sound_list[i].Stop();
                sound_list[i].clip = null;
                GameObjectPool.Instance.CollectObject(sound_list[i].gameObject);
            }

            sound_list.Clear();
        }
    }
}

