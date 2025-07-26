using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace tjtFramework.Utiliy
{
    public static class MathUtility
    {
        // 1. 幂运算
        /// <summary>
        /// 计算 a 的 b 次幂。
        /// </summary>
        public static double Power(double a, double b) => Math.Pow(a, b);

        // 2. 开方运算
        /// <summary>
        /// 计算一个数的平方根。
        /// </summary>
        public static double SquareRoot(double a) => Math.Sqrt(a);

        /// <summary>
        /// 计算一个数的立方根。
        /// </summary>
        public static double CubeRoot(double a) => Math.Pow(a, 1.0 / 3.0);

        // 3. 对数运算
        /// <summary>
        /// 计算一个数的自然对数（以 e 为底）。
        /// </summary>
        public static double NaturalLog(double a) => Math.Log(a);

        /// <summary>
        /// 计算一个数的常用对数（以 10 为底）。
        /// </summary>
        public static double Log10(double a) => Math.Log10(a);

        // 4. 阶乘
        /// <summary>
        /// 计算一个整数的阶乘。
        /// </summary>
        public static int Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);

        // 5. 指数运算
        /// <summary>
        /// 计算 e 的指定指数。
        /// </summary>
        public static double Exp(double exponent) => Math.Exp(exponent);

        // 6. 三角函数
        /// <summary>
        /// 计算正弦值（弧度制）。
        /// </summary>
        public static double Sin(double radians) => Math.Sin(radians);

        /// <summary>
        /// 计算余弦值（弧度制）。
        /// </summary>
        public static double Cos(double radians) => Math.Cos(radians);

        /// <summary>
        /// 计算正切值（弧度制）。
        /// </summary>
        public static double Tan(double radians) => Math.Tan(radians);

        // 7. 反三角函数
        /// <summary>
        /// 计算反正弦值（返回弧度）。
        /// </summary>
        public static double ArcSin(double value) => Math.Asin(value);

        /// <summary>
        /// 计算反余弦值（返回弧度）。
        /// </summary>
        public static double ArcCos(double value) => Math.Acos(value);

        /// <summary>
        /// 计算反正切值（返回弧度）。
        /// </summary>
        public static double ArcTan(double value) => Math.Atan(value);

        // 8. 角度和弧度转换
        /// <summary>
        /// 将角度转换为弧度。
        /// </summary>
        public static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

        /// <summary>
        /// 将弧度转换为角度。
        /// </summary>
        public static double RadiansToDegrees(double radians) => radians * 180 / Math.PI;

        // 9. 随机数生成
        /// <summary>
        /// 在指定范围内生成随机整数。
        /// </summary>
        public static int RandomInt(int min, int max) => UnityEngine.Random.Range(min, max);

        /// <summary>
        /// 在指定范围内生成随机浮点数。
        /// </summary>
        public static float RandomFloat(float min, float max) => UnityEngine.Random.Range(min, max);

        /// <summary>
        /// 生成符合正态分布的随机数。
        /// </summary>
        public static double RandomGaussian(double mean = 0, double stdDev = 1)
        {
            double u1 = 1.0 - UnityEngine.Random.value;
            double u2 = 1.0 - UnityEngine.Random.value;
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        // 10. 数值舍入
        /// <summary>
        /// 将小数四舍五入到指定小数位。
        /// </summary>
        public static double Round(double value, int decimals) => Math.Round(value, decimals);

        /// <summary>
        /// 将小数向上取整。
        /// </summary>
        public static double Ceil(double value) => Math.Ceiling(value);

        /// <summary>
        /// 将小数向下取整。
        /// </summary>
        public static double Floor(double value) => Math.Floor(value);

        /// <summary>
        /// 返回数值的绝对值。
        /// </summary>
        public static double Abs(double value) => Math.Abs(value);

        // 11. 统计运算
        /// <summary>
        /// 计算一组数的平均值。
        /// </summary>
        public static double Average(IEnumerable<double> numbers) => numbers.Average();

        /// <summary>
        /// 计算一组数的中位数。
        /// </summary>
        public static double Median(IEnumerable<double> numbers)
        {
            var sortedNumbers = numbers.OrderBy(n => n).ToArray();
            int count = sortedNumbers.Length;
            return count % 2 == 0 ? (sortedNumbers[count / 2 - 1] + sortedNumbers[count / 2]) / 2 : sortedNumbers[count / 2];
        }

        /// <summary>
        /// 计算一组数的方差。
        /// </summary>
        public static double Variance(IEnumerable<double> numbers)
        {
            double mean = Average(numbers);
            return numbers.Average(num => Math.Pow(num - mean, 2));
        }

        /// <summary>
        /// 计算一组数的标准差。
        /// </summary>
        public static double StandardDeviation(IEnumerable<double> numbers) => Math.Sqrt(Variance(numbers));

        // 12. 最小值和最大值
        /// <summary>
        /// 找到一组数中的最小值。
        /// </summary>
        public static double Min(IEnumerable<double> numbers) => numbers.Min();

        /// <summary>
        /// 找到一组数中的最大值。
        /// </summary>
        public static double Max(IEnumerable<double> numbers) => numbers.Max();

        // 13. 距离和向量运算
        /// <summary>
        /// 计算两点间的距离。
        /// </summary>
        public static float Distance(Vector2 point1, Vector2 point2) => Vector2.Distance(point1, point2);

        /// <summary>
        /// 计算两个向量的点积。
        /// </summary>
        public static float DotProduct(Vector2 vector1, Vector2 vector2) => Vector2.Dot(vector1, vector2);

        /// <summary>
        /// 计算两个向量的叉积。
        /// </summary>
        public static float CrossProduct(Vector2 vector1, Vector2 vector2) => vector1.x * vector2.y - vector1.y * vector2.x;

        // 14. 线性插值和插值运算
        /// <summary>
        /// 计算在两个值之间的线性插值。
        /// </summary>
        public static float Lerp(float start, float end, float t) => Mathf.Lerp(start, end, t);


        /// <summary>
        /// 计算3D空间中两点间的距离。
        /// </summary>
        public static float Distance3D(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        /// <summary>
        /// 计算2D空间中两点间的距离。
        /// </summary>
        public static float Distance2D(Vector2 point1, Vector2 point2)
        {
            return Vector2.Distance(point1, point2);
        }

        /// <summary>
        /// 计算两点在指定平面（XZ, YZ, XY）的距离。
        /// </summary>
        public static float DistanceOnPlane(Vector3 point1, Vector3 point2, string plane)
        {
            switch (plane.ToLower())
            {
                case "xz":
                    return Vector2.Distance(new Vector2(point1.x, point1.z), new Vector2(point2.x, point2.z));
                case "yz":
                    return Vector2.Distance(new Vector2(point1.y, point1.z), new Vector2(point2.y, point2.z));
                case "xy":
                    return Vector2.Distance(new Vector2(point1.x, point1.y), new Vector2(point2.x, point2.y));
                default:
                    throw new System.ArgumentException("Invalid plane. Use 'xz', 'yz', or 'xy'.");
            }
        }

        /// <summary>
        /// 判断一个物体是否在屏幕外。
        /// </summary>
        public static bool IsOutsideScreen(Vector3 worldPosition, Camera camera)
        {
            Vector3 screenPoint = camera.WorldToViewportPoint(worldPosition);
            return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
        }

        /// <summary>
        /// 检查一个点是否在指定半径内。
        /// </summary>
        public static bool IsInRange(Vector3 point, Vector3 center, float range)
        {
            return Vector3.Distance(point, center) <= range;
        }

        /// <summary>
        /// 检查一个物体是否在指定范围内（指定半径）。
        /// </summary>
        public static bool IsObjectInRange(GameObject obj, Vector3 center, float range)
        {
            return Vector3.Distance(obj.transform.position, center) <= range;
        }

        /// <summary>
        /// 检查多个物体是否在指定范围内（指定半径）。
        /// </summary>
        public static bool AreObjectsInRange(GameObject[] objects, Vector3 center, float range)
        {
            foreach (var obj in objects)
            {
                if (Vector3.Distance(obj.transform.position, center) > range)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 判断一个点是否在3D锥形范围内。
        /// </summary>
        /// <param name="point">要检查的目标点。</param>
        /// <param name="sectorCenter">扇形的中心点。</param>
        /// <param name="sectorDirection">扇形的方向向量，通常是扇形的“前方”或“正面”。</param>
        /// <param name="sectorAngle">扇形的总角度，单位为度。例如，90度表示一个直角扇形。</param>
        /// <param name="sectorRadius">扇形的半径，即扇形的最大距离。</param>
        /// <returns>如果点在锥形范围内，则返回true；否则返回false。</returns>
        public static bool IsPointInSector(Vector3 point, Vector3 sectorCenter, Vector3 sectorDirection, float sectorAngle, float sectorRadius)
        {
            Vector3 directionToPoint = (point - sectorCenter).normalized;

            float angleBetween = Vector3.Angle(sectorDirection, directionToPoint);
            if (angleBetween <= sectorAngle / 2)
            {
                float distance = Vector3.Distance(sectorCenter, point);
                if (distance <= sectorRadius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取在球形范围内的所有物体。
        /// </summary>
        /// <param name="sphereCenter">球形范围的中心。</param>
        /// <param name="sphereRadius">球形范围的半径。</param>
        /// <param name="layerMask">需要检测的层级。</param>
        /// <returns>返回在球形范围内的所有物体。</returns>
        public static Collider[] GetObjectsInSphere(Vector3 sphereCenter, float sphereRadius, LayerMask layerMask)
        {
            // 使用OverlapSphere获取球形范围内的所有碰撞体
            return Physics.OverlapSphere(sphereCenter, sphereRadius, layerMask);
        }

        /// <summary>
        /// 检测盒子范围内的所有物体。
        /// </summary>
        /// <param name="boxCenter">盒子的中心位置</param>
        /// <param name="boxSize">盒子的尺寸</param>
        /// <param name="boxRotation">盒子的旋转角度（Quaternion）</param>
        /// <returns>返回在盒子范围内的所有碰撞体</returns>
        public static Collider[] GetObjectsInBox(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation)
        {
            // 使用Physics.OverlapBox进行碰撞体检测，注意需要传入旋转信息
            Collider[] hitColliders = Physics.OverlapBox(boxCenter, boxSize / 2, boxRotation);

            // 返回所有检测到的物体
            return hitColliders;
        }
    }
}


