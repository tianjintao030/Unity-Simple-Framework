using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.Utiliy
{
    public class ListWithPriority
    {
        public int priority = 0;
    }

    public class ListWithPriorityMonoBehaviour : MonoBehaviour
    {
        public int priority = 0;
    }

    public static class ListUtility
    {
        public static void MergeList<T>(List<T> from_list, List<T> to_list)
        {
            for (int i = 0; i < from_list.Count; i++)
                if (to_list.Contains(from_list[i]) == false)
                    to_list.Add(from_list[i]);
        }



        public static void AddItemWithPriority<T>(List<T> list, T item, bool is_descending = true) where T : ListWithPriority
        {
            if (list.Contains(item) == false)
                list.Add(item);

            list.Sort((x, y) => (is_descending == true ? -1 : 1) * x.priority.CompareTo(y.priority));
        }


        public static void AddItemWithPriorityMonoBehaviour<T>(List<T> list, T item, bool is_descending = true) where T : ListWithPriorityMonoBehaviour
        {
            if (list.Contains(item) == false)
                list.Add(item);

            list.Sort((x, y) => (is_descending == true ? -1 : 1) * x.priority.CompareTo(y.priority));
        }



        public static int GetIndexOfItem<T>(List<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Equals(item) == true)
                    return i;

            return -1;
        }


        public static int[] GetIntArray(string[] string_array)
        {
            int[] int_array = new int[string_array.Length];

            for (int i = 0; i < string_array.Length; i++)
                int.TryParse(string_array[i], out int_array[i]);

            return int_array;
        }

        /// <summary>
        /// 列表中获取随机一个值
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogError($"列表为空或未初始化！");
                return default(T);
            }
            return list[Random.Range(0, list.Count)];
        }


        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}

    


