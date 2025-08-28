using System;

namespace DominoNext.Models.Music
{
    /// <summary>
    /// 音乐分数，用于表示音符时值和位置
    /// 使用传统音乐记谱法：四分音符 = 1/4，全音符 = 1/1
    /// </summary>
    public readonly struct MusicalFraction : IEquatable<MusicalFraction>, IComparable<MusicalFraction>
    {
        public int Numerator { get; }
        public int Denominator { get; }

        // 定义四分音符的标准tick值
        public const int QUARTER_NOTE_TICKS = 96;

        public MusicalFraction(int numerator, int denominator)
        {
            if (denominator <= 0)
                throw new ArgumentException("分母必须大于0", nameof(denominator));

            // 特殊处理：如果分子为0，表示位置0，直接设置为0/1
            if (numerator == 0)
            {
                Numerator = 0;
                Denominator = 1;
                return;
            }

            // 修复 OverflowException: 安全处理 int.MinValue
            if (numerator == int.MinValue)
            {
                // 对于极端情况，不进行简化
                Numerator = numerator;
                Denominator = denominator;
                return;
            }

            // 简化分数
            var gcd = GreatestCommonDivisor(Math.Abs(numerator), denominator);
            Numerator = numerator / gcd;
            Denominator = denominator / gcd;
        }

        /// <summary>
        /// 转换为tick值
        /// 使用传统音乐记谱法计算：1/4 = 四分音符 = QUARTER_NOTE_TICKS
        /// </summary>
        /// <param name="quarterNoteTicks">四分音符的tick数（默认96）</param>
        /// <returns>tick值</returns>
        public double ToTicks(int quarterNoteTicks = QUARTER_NOTE_TICKS)
        {
            // 传统音乐记谱法转换：
            // 1/1 = 全音符 = 4 * 四分音符 = 4 * quarterNoteTicks
            // 1/2 = 二分音符 = 2 * 四分音符 = 2 * quarterNoteTicks  
            // 1/4 = 四分音符 = 1 * 四分音符 = quarterNoteTicks
            // 1/8 = 八分音符 = 0.5 * 四分音符 = quarterNoteTicks/2
            // 1/16 = 十六分音符 = 0.25 * 四分音符 = quarterNoteTicks/4

            // 公式：(4/分母) * (分子) * quarterNoteTicks
            return (double)Numerator * 4 / Denominator * quarterNoteTicks;
        }

        /// <summary>
        /// 从tick值创建音乐分数
        /// </summary>
        public static MusicalFraction FromTicks(double ticks, int quarterNoteTicks = QUARTER_NOTE_TICKS)
        {
            // 添加安全检查
            if (double.IsNaN(ticks) || double.IsInfinity(ticks) || ticks < 0)
            {
                return new MusicalFraction(1, 16); // 默认十六分音符
            }

            // 修复：如果ticks为0或接近0，直接返回0
            if (Math.Abs(ticks) < 1e-10)
            {
                return new MusicalFraction(0, 1); // 0时间位置
            }

            // 计算相对于四分音符的倍数
            var quarterNoteMultiple = ticks / quarterNoteTicks;

            // 支持常见的音符时值分母：1, 2, 4, 8, 16, 32, 64
            var commonDenominators = new[] { 1, 2, 4, 8, 16, 32, 64 };

            foreach (var denominator in commonDenominators)
            {
                // 按照传统记谱法计算分子
                var numerator = Math.Round(quarterNoteMultiple * denominator / 4.0);

                if (numerator >= 1 && numerator <= int.MaxValue)
                {
                    var intNumerator = (int)numerator;
                    var testFraction = new MusicalFraction(intNumerator, denominator);

                    // 检查转换后的tick值是否匹配
                    if (Math.Abs(testFraction.ToTicks(quarterNoteTicks) - ticks) < 0.001)
                    {
                        return testFraction;
                    }
                }
            }

            // 默认使用64分音符精度
            var bestNumerator = Math.Max(1, Math.Round(quarterNoteMultiple * 64 / 4.0));
            if (bestNumerator <= int.MaxValue)
            {
                return new MusicalFraction((int)bestNumerator, 64);
            }

            // 如果仍然溢出，返回安全的默认值
            return new MusicalFraction(1, 16); // 默认十六分音符
        }

        /// <summary>
        /// 获取网格贴合单位（以tick为单位）
        /// </summary>
        /// <param name="quarterNoteTicks">四分音符的tick数</param>
        /// <returns>网格单位的tick数</returns>
        public double GetGridUnit(int quarterNoteTicks = QUARTER_NOTE_TICKS)
        {
            return ToTicks(quarterNoteTicks);
        }

        /// <summary>
        /// 对位置进行网格量化
        /// </summary>
        /// <param name="positionInTicks">要量化的位置（tick）</param>
        /// <param name="gridUnit">网格单位（此MusicalFraction）</param>
        /// <param name="quarterNoteTicks">四分音符的tick数</param>
        /// <returns>量化后的位置（tick）</returns>
        public static double QuantizeToGrid(double positionInTicks, MusicalFraction gridUnit, int quarterNoteTicks = QUARTER_NOTE_TICKS)
        {
            // 添加安全检查
            if (double.IsNaN(positionInTicks) || double.IsInfinity(positionInTicks))
            {
                return 0;
            }

            // 修复：如果位置已经非常接近0，直接返回0，避免量化偏移
            if (Math.Abs(positionInTicks) < 1e-10)
            {
                return 0;
            }

            var gridSizeInTicks = gridUnit.GetGridUnit(quarterNoteTicks);
            if (gridSizeInTicks <= 0)
            {
                return positionInTicks; // 如果网格大小无效，返回原值
            }

            // 使用更精确的量化算法，确保0位置不会偏移
            var quantizedPosition = Math.Round(positionInTicks / gridSizeInTicks) * gridSizeInTicks;
            
            // 确保量化后的位置不为负数（除非原位置就是负数）
            if (positionInTicks >= 0 && quantizedPosition < 0)
            {
                quantizedPosition = 0;
            }

            return quantizedPosition;
        }

        /// <summary>
        /// 计算从起始位置到结束位置的量化长度
        /// </summary>
        /// <param name="startTicks">起始位置（tick）</param>
        /// <param name="endTicks">结束位置（tick）</param>
        /// <param name="gridUnit">网格单位</param>
        /// <param name="quarterNoteTicks">四分音符的tick数</param>
        /// <returns>量化后的长度分数</returns>
        public static MusicalFraction CalculateQuantizedDuration(double startTicks, double endTicks, MusicalFraction gridUnit, int quarterNoteTicks = QUARTER_NOTE_TICKS)
        {
            var durationTicks = Math.Max(gridUnit.GetGridUnit(quarterNoteTicks), endTicks - startTicks);
            var gridSizeInTicks = gridUnit.GetGridUnit(quarterNoteTicks);

            if (gridSizeInTicks <= 0)
            {
                return gridUnit; // 返回默认网格单位
            }

            var gridUnits = Math.Max(1, Math.Round(durationTicks / gridSizeInTicks));

            // 添加安全检查，避免溢出
            var resultNumerator = gridUnits * gridUnit.Numerator;
            if (resultNumerator >= int.MinValue && resultNumerator <= int.MaxValue)
            {
                return new MusicalFraction((int)resultNumerator, gridUnit.Denominator);
            }

            // 如果溢出，返回原网格单位
            return gridUnit;
        }

        /// <summary>常用音符时值（传统音乐记谱法）</summary>
        public static MusicalFraction WholeNote => new(1, 1);        // 全音符 = 1/1
        public static MusicalFraction HalfNote => new(1, 2);         // 二分音符 = 1/2
        public static MusicalFraction QuarterNote => new(1, 4);      // 四分音符 = 1/4
        public static MusicalFraction EighthNote => new(1, 8);       // 八分音符 = 1/8
        public static MusicalFraction SixteenthNote => new(1, 16);   // 十六分音符 = 1/16
        public static MusicalFraction ThirtySecondNote => new(1, 32); // 三十二分音符 = 1/32

        // 三连音
        public static MusicalFraction TripletHalf => new(1, 3);      // 三连二分音符 = 1/3
        public static MusicalFraction TripletQuarter => new(1, 6);   // 三连四分音符 = 1/6
        public static MusicalFraction TripletEighth => new(1, 12);   // 三连八分音符 = 1/12
        public static MusicalFraction TripletSixteenth => new(1, 24); // 三连十六分音符 = 1/24

        // 附点音符（原时值的1.5倍）
        public static MusicalFraction DottedHalf => new(3, 4);       // 附点二分音符 = 3/4
        public static MusicalFraction DottedQuarter => new(3, 8);    // 附点四分音符 = 3/8
        public static MusicalFraction DottedEighth => new(3, 16);    // 附点八分音符 = 3/16

        private static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }
            return a;
        }

        public bool Equals(MusicalFraction other) =>
            Numerator == other.Numerator && Denominator == other.Denominator;

        public int CompareTo(MusicalFraction other) =>
            ToTicks().CompareTo(other.ToTicks());

        public override bool Equals(object? obj) =>
            obj is MusicalFraction other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Numerator, Denominator);

        public override string ToString() => $"{Numerator}/{Denominator}";

        public static bool operator ==(MusicalFraction left, MusicalFraction right) => left.Equals(right);
        public static bool operator !=(MusicalFraction left, MusicalFraction right) => !left.Equals(right);
        public static bool operator <(MusicalFraction left, MusicalFraction right) => left.CompareTo(right) < 0;
        public static bool operator >(MusicalFraction left, MusicalFraction right) => left.CompareTo(right) > 0;

        public static MusicalFraction operator +(MusicalFraction left, MusicalFraction right)
        {
            var numerator = left.Numerator * right.Denominator + right.Numerator * left.Denominator;
            var denominator = left.Denominator * right.Denominator;
            return new MusicalFraction(numerator, denominator);
        }

        public static MusicalFraction operator -(MusicalFraction left, MusicalFraction right)
        {
            var numerator = left.Numerator * right.Denominator - right.Numerator * left.Denominator;
            var denominator = left.Denominator * right.Denominator;
            return new MusicalFraction(numerator, denominator);
        }

        public static MusicalFraction operator *(MusicalFraction left, int multiplier)
        {
            return new MusicalFraction(left.Numerator * multiplier, left.Denominator);
        }
    }
}