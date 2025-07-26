using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace tjtFramework.Utiliy
{
    public static class TextUtility
    {
        /// <summary>
        /// 将字符串按指定的分隔符拆分为列表
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>拆分后的字符串列表</returns>
        public static List<string> SplitString(string input, char delimiter)
        {
            return new List<string>(input.Split(delimiter));
        }

        /// <summary>
        /// 将秒数转换为时分秒格式的字符串
        /// </summary>
        /// <param name="seconds">输入的秒数</param>
        /// <returns>时分秒格式的字符串</returns>
        public static string SecondsToHMS(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;
            return $"{hours}小时 {minutes}分钟 {secs}秒";
        }

        /// <summary>
        /// 将秒数转换为 00:00:00 格式的字符串
        /// </summary>
        /// <param name="seconds">输入的秒数</param>
        /// <returns>00:00:00 格式的字符串</returns>
        public static string SecondsToHHMMSS(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;
            return $"{hours:D2}:{minutes:D2}:{secs:D2}";
        }

        /// <summary>
        /// 将大数值转换为简短的字符串格式（如：1000 -> 1K, 1000000 -> 1M）
        /// </summary>
        /// <param name="number">输入的大数值</param>
        /// <returns>简短的字符串表示</returns>
        public static string LargeNumberToString(long number)
        {
            if (number >= 1_000_000_000)
                return $"{number / 1_000_000_000.0:F1}B"; // 亿（Billion）
            else if (number >= 1_000_000)
                return $"{number / 1_000_000.0:F1}M"; // 百万（Million）
            else if (number >= 1_000)
                return $"{number / 1_000.0:F1}K"; // 千（Thousand）
            else
                return number.ToString();
        }

        /// <summary>
        /// 去除字符串中的指定符号，并移除前后空白字符。
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="symbols">要移除的符号字符数组</param>
        /// <returns>去除符号和空白后的字符串</returns>
        public static string TrimAndRemoveSymbols(string input, char[] symbols)
        {
            foreach (var symbol in symbols)
            {
                input = input.Replace(symbol.ToString(), ""); // 将指定符号移除
            }
            return input.Trim(); // 去除前后空白字符
        }

        /// <summary>
        /// 替换字符串中的某个子串，可以忽略大小写。
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="oldValue">要替换的子串</param>
        /// <param name="newValue">用于替换的子串</param>
        /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplaceSubstring(string input, string oldValue, string newValue, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                // 使用正则表达式进行忽略大小写的替换
                return Regex.Replace(input, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
            }
            return input.Replace(oldValue, newValue); // 普通替换
        }

        /// <summary>
        /// 将字符串列表按照指定的分隔符拼接成单个字符串。
        /// </summary>
        /// <param name="strings">要拼接的字符串列表</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>拼接后的字符串</returns>
        public static string JoinStrings(List<string> strings, string delimiter)
        {
            return string.Join(delimiter, strings); // 使用指定的分隔符将列表拼接成单个字符串
        }

        /// <summary>
        /// 将字符串反转。
        /// </summary>
        /// <param name="input">要反转的字符串</param>
        /// <returns>反转后的字符串</returns>
        public static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray(); // 将字符串转换为字符数组
            Array.Reverse(charArray); // 反转字符数组
            return new string(charArray); // 将反转后的字符数组重新转换为字符串
        }

        /// <summary>
        /// 检查字符串中是否包含某个子串，可以忽略大小写。
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="substring">要检查的子串</param>
        /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
        /// <returns>如果包含子串，则返回 true；否则返回 false</returns>
        public static bool ContainsSubstring(string input, string substring, bool ignoreCase = false)
        {
            return ignoreCase
                ? input.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0 // 忽略大小写的包含检查
                : input.Contains(substring); // 普通包含检查
        }

        /// <summary>
        /// 将每个单词的首字母大写。
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <returns>首字母大写后的字符串</returns>
        public static string CapitalizeFirstLetterOfEachWord(string input)
        {
            var words = input.Split(' '); // 根据空格拆分为单词数组
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1); // 将首字母大写
                }
            }
            return string.Join(" ", words); // 拼接为完整字符串并返回
        }

        /// <summary>
        /// 格式化数字，添加千分位符号（如：1,000）。
        /// </summary>
        /// <param name="number">要格式化的数字</param>
        /// <returns>带有千分位的字符串</returns>
        public static string FormatNumberWithCommas(int number)
        {
            return number.ToString("N0"); // 使用 "N0" 格式化为带千分位的字符串
        }

        /// <summary>
        /// 将 DateTime 对象格式化为指定的字符串格式。
        /// </summary>
        /// <param name="date">DateTime 对象</param>
        /// <param name="format">格式字符串（例如："yyyy-MM-dd HH:mm:ss"）</param>
        /// <returns>格式化后的日期字符串</returns>
        public static string FormatDate(DateTime date, string format)
        {
            return date.ToString(format); // 使用指定格式将 DateTime 转换为字符串
        }

        /// <summary>
        /// 截取字符串作为摘要，超过指定长度则添加省略号。
        /// </summary>
        /// <param name="text">原始字符串</param>
        /// <param name="length">最大长度</param>
        /// <returns>截取后的摘要字符串</returns>
        public static string GetSummary(string text, int length)
        {
            return text.Length <= length ? text : text.Substring(0, length) + "..."; // 截取并添加省略号
        }

        /// <summary>
        /// 将字符串进行 Base64 编码。
        /// </summary>
        /// <param name="input">要编码的字符串</param>
        /// <returns>Base64 编码后的字符串</returns>
        public static string EncodeToBase64(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input); // 将字符串转换为字节数组
            return Convert.ToBase64String(bytes); // 将字节数组转换为 Base64 编码字符串
        }

        /// <summary>
        /// 计算字符串中的字符数量。
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="includeSpaces">是否包含空格，默认为 true</param>
        /// <returns>字符数量</returns>
        public static int CountCharacters(string input, bool includeSpaces = true)
        {
            return includeSpaces ? input.Length : input.Replace(" ", "").Length; // 根据选项返回字符数
        }

        /// <summary>
        /// 使用正则表达式检查字符串是否符合特定模式。
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <returns>如果匹配则返回 true，否则返回 false</returns>
        public static bool RegexMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern); // 使用正则表达式进行匹配
        }

        /// <summary>
        /// 删除多行文本中的空白行。
        /// </summary>
        /// <param name="text">多行文本</param>
        /// <returns>删除空白行后的文本</returns>
        public static string RemoveEmptyLines(string text)
        {
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); // 按行拆分，并移除空白行
            return string.Join(Environment.NewLine, lines); // 拼接为完整文本
        }
    }
}

