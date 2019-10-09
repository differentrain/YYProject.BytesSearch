/*
 MIT License

Copyright (c) 2019 differentrain

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */


/*
Requirements:
    C# 7.3
    .NET Frameworks 4.5;

 Build Options:
    Enable 'Allow unsafe code';
*/

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

#pragma warning disable CA1303

namespace YYProject.BytesSearch
{
    /// <summary>
    /// Represents a byte array finder with the immutable pattern.
    /// </summary>
    /// <remarks>
    /// Rules For String Pattern
    /// <para>Any space character (0x20) should be ignored.</para>
    /// <para>A byte number must be expressed as two-digit hexadecimal number, excludes any prefix or postfix.</para>
    /// <para>Question mark (0x3F) represents wildcard and must be in pairs, or it has a leading or trailing hexadecimal number.</para>
    /// </remarks>
    public class BytesFinder
    {


        private readonly PatternInfo _mPattInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BytesFinder"/> class for the specified bytes pattern.
        /// </summary>
        /// <param name="pattern">The bytes pattern to seek.</param>
        /// <exception cref="ArgumentException"><paramref name="pattern"/> is null or empty.</exception>
        public BytesFinder(byte[] pattern)
        {
            if (pattern == null || pattern.Length == 0) throw new ArgumentException("pattern is null or empty.", nameof(pattern));
            _mPattInfo = new PatternInfo(pattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytesFinder"/> class for the specified <see cref="string"/> pattern.
        /// </summary>
        /// <param name="pattern">The <see cref="string"/> pattern to seek.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="FormatException">
        /// The length of <paramref name="pattern"/> is 0 or not equal to this value division by 2.
        /// <para>- Or -</para>
        /// Unexpected char in <paramref name="pattern"/>.
        /// </exception>
        public BytesFinder(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern), "pattern is null.");
            _mPattInfo = new PatternInfo(pattern.Replace(" ", string.Empty)); //remove placeholder
        }

        #region instance methods

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the pattern in the specified bytes.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <returns>The zero-based index position of the occurrence if the pattern is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is null or empty.</exception>
        public int FindIndexIn(byte[] source)
        {
            if (source == null || source.Length == 0) throw new ArgumentException("source is null or empty.", nameof(source));
            return InnerFindIndex(source, in _mPattInfo, 0, source.Length);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the pattern in the specified bytes.
        /// The search starts at the specified position.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <returns>The zero-based index position of the occurrence if the pattern is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0.
        /// <para>- Or -</para>
        /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="source"/>.
        /// </exception>
        public int FindIndexIn(byte[] source, int startIndex)
        {
            Ensure_source_startIndex(source, startIndex);
            return InnerFindIndex(source, in _mPattInfo, startIndex, source.Length);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the pattern in the specified bytes.
        /// The search starts at the specified position and examines a specified number of <see cref="byte"/> positions.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of <see cref="byte"/> positions to examine.</param>
        /// <returns>The zero-based index position of the occurrence if the pattern is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0.
        /// <para>- Or -</para>
        /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="source"/>.
        /// <para>- Or -</para>
        /// <paramref name="count"/> is less than or equal to 0.
        /// <para>- Or -</para>
        /// <paramref name="count"/> is greater than the length of source minus <paramref name="startIndex"/>.
        /// </exception>
        public int FindIndexIn(byte[] source, int startIndex, int count)
        {
            Ensure_source_startIndex_count(source, startIndex, count);
            return InnerFindIndex(source, in _mPattInfo, startIndex, count);
        }

        #endregion

        #region static methods

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified pattern in the specified bytes source.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="pattern">The bytes pattern to seek.</param>
        /// <returns>The zero-based index position of the occurrence if the <paramref name="pattern"/> is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="source"/> is null or empty.
        /// <para>- Or -</para>
        /// <paramref name="pattern"/> is null or empty.
        /// </exception>
        public static int FindIndex(byte[] source, byte[] pattern)
        {
            if (source == null || source.Length == 0) throw new ArgumentException("source is null or empty.", nameof(source));
            if (pattern == null || pattern.Length == 0) throw new ArgumentException("pattern is null or empty.", nameof(pattern));
            using (var pattInfo = new PatternInfo(pattern))
            {
                return InnerFindIndex(source, in pattInfo, 0, source.Length);
            }

        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified pattern in the specified bytes source.
        /// The search starts at the specified position.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="pattern">The bytes pattern to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <returns>The zero-based index position of the occurrence if the <paramref name="pattern"/> is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="source"/> is null or empty.
        /// <para>- Or -</para>
        /// <paramref name="pattern"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0.
        /// <para>- Or -</para>
        /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="source"/>.
        /// </exception>
        public static int FindIndex(byte[] source, byte[] pattern, int startIndex)
        {
            if (pattern == null || pattern.Length == 0) throw new ArgumentException("pattern is null or empty.", nameof(pattern));
            Ensure_source_startIndex(source, startIndex);
            using (var pattInfo = new PatternInfo(pattern))
            {
                return InnerFindIndex(source, in pattInfo, startIndex, source.Length);
            }
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified pattern in the specified bytes source.
        /// The search starts at the specified position and examines a specified number of <see cref="byte"/> positions.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="pattern">The bytes pattern to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of <see cref="byte"/> positions to examine.</param>
        /// <returns>The zero-based index position of the occurrence if the <paramref name="pattern"/> is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="source"/> is null or empty.
        /// <para>- Or -</para>
        /// <paramref name="pattern"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0.
        /// <para>- Or -</para>
        /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="source"/>.
        /// <para>- Or -</para>
        /// <paramref name="count"/> is less than or equal to 0.
        /// <para>- Or -</para>
        /// <paramref name="count"/> is greater than the length of source minus <paramref name="startIndex"/>.
        /// </exception>
        public static int FindIndex(byte[] source, byte[] pattern, int startIndex, int count)
        {
            if (pattern == null || pattern.Length == 0) throw new ArgumentException("pattern is null or empty.", nameof(pattern));
            Ensure_source_startIndex_count(source, startIndex, count);
            using (var pattInfo = new PatternInfo(pattern))
            {
                return InnerFindIndex(source, in pattInfo, startIndex, count);
            }
        }


        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified pattern in the specified bytes source.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="pattern">The <see cref="string"/> pattern to seek.</param>
        /// <returns>The zero-based index position of the occurrence if the <paramref name="pattern"/> is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="FormatException">
        /// The length of <paramref name="pattern"/> is 0 or not equal to this value division by 2.
        /// <para>- Or -</para>
        /// Unexpected char in <paramref name="pattern"/>.
        /// </exception>
        public static int FindIndex(byte[] source, string pattern)
        {
            if (source == null || source.Length == 0) throw new ArgumentException("source is null or empty.", nameof(source));
            return (new BytesFinder(pattern)).FindIndexIn(source);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified pattern in the specified bytes source.
        /// The search starts at the specified position.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="pattern">The <see cref="string"/> pattern to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <returns>The zero-based index position of the occurrence if the <paramref name="pattern"/> is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="FormatException">
        /// The length of <paramref name="pattern"/> is 0 or not equal to this value division by 2.
        /// <para>- Or -</para>
        /// Unexpected char in <paramref name="pattern"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0.
        /// <para>- Or -</para>
        /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="source"/>.
        /// </exception>
        public static int FindIndex(byte[] source, string pattern, int startIndex)
        {
            Ensure_source_startIndex(source, startIndex);
            return (new BytesFinder(pattern)).FindIndexIn(source, startIndex);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified pattern in the specified bytes source.
        /// The search starts at the specified position and examines a specified number of <see cref="byte"/> positions.
        /// </summary>
        /// <param name="source">The bytes to search for an occurrence.</param>
        /// <param name="pattern">The <see cref="string"/> pattern to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of <see cref="byte"/> positions to examine.</param>
        /// <returns>The zero-based index position of the occurrence if the <paramref name="pattern"/> is found, otherwise, -1.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="FormatException">
        /// The length of <paramref name="pattern"/> is 0 or not equal to this value division by 2.
        /// <para>- Or -</para>
        /// Unexpected char in <paramref name="pattern"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is less than 0.
        /// <para>- Or -</para>
        /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="source"/>.
        /// <para>- Or -</para>
        /// <paramref name="count"/> is less than or equal to 0.
        /// <para>- Or -</para>
        /// <paramref name="count"/> is greater than the length of source minus <paramref name="startIndex"/>.
        /// </exception>
        public static int FindIndex(byte[] source, string pattern, int startIndex, int count)
        {
            Ensure_source_startIndex_count(source, startIndex, count);
            return (new BytesFinder(pattern)).FindIndexIn(source, startIndex, count);
        }


        #endregion

        private static int InnerFindIndex(byte[] source, in PatternInfo pattern, int startIndex, int count)
        {
            var patternLength = pattern.PatternLength;
            var pattMaxIdx = patternLength - 1;
            var maxLen = count - patternLength + 1;
            unsafe
            {
                fixed (int* next = pattern.MoveTable)
                {
                    fixed (byte* src = source)
                    {
                        while (startIndex < maxLen)
                        {
                            var mov = next[src[startIndex + pattMaxIdx]];
                            if (mov < patternLength)
                            {
                                startIndex += mov;
                                if (pattern.CheckWithPattern(source, startIndex)) return startIndex;
                                ++startIndex;
                            }
                            else
                            {
                                startIndex += patternLength;
                            }
                        }
                        return -1;
                    }
                }
            }
        }

        private static void Ensure_source_startIndex(byte[] source, int startIndex)
        {
            if (source == null || source.Length == 0) throw new ArgumentException("source is null or empty.", nameof(source));
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is less than 0.");
            if (startIndex >= source.Length) throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is greater than or equal to the length of source.");
        }

        private static void Ensure_source_startIndex_count(byte[] source, int startIndex, int count)
        {
            if (source == null || source.Length == 0) throw new ArgumentException("source is null or empty.", nameof(source));
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is less than 0.");
            if (startIndex >= source.Length) throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is greater than or equal to the length of source.");
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(startIndex), "count is less than or equal to 0.");
            if (count > source.Length - startIndex) throw new ArgumentOutOfRangeException(nameof(count), "count is greater than the length of source minus startIndex.");
        }

        private readonly struct PatternInfo : IDisposable
        {

            private static readonly ConcurrentBag<int[]> _MoveTablePool = new ConcurrentBag<int[]>();

            private delegate bool ComparePatternFunc(byte[] source, byte[] pattern, int index);

            #region expressions
            private static readonly ParameterExpression _ExpParamSource = Expression.Parameter(typeof(byte[]), "source");
            private static readonly ParameterExpression _ExpParamIndex = Expression.Parameter(typeof(int), "index");
            private static readonly Expression _ExpSourceMaxCount = Expression.Subtract(Expression.ArrayLength(_ExpParamSource), _ExpParamIndex);

            private static readonly ParameterExpression _ExpUnusedParamBytesPattern = Expression.Parameter(typeof(byte[]), "unusedBytesPattern");
            private static readonly BinaryExpression _ExpArrayItemIterator = Expression.ArrayIndex(_ExpParamSource, Expression.PostIncrementAssign(_ExpParamIndex));
            private static readonly BlockExpression _ExpUnconditionalTrue = Expression.Block(Expression.PreIncrementAssign(_ExpParamIndex), Expression.Constant(true, typeof(bool)));
            #endregion

            private readonly ComparePatternFunc Comparer;

            public readonly int PatternLength;

            public readonly byte[] BytesPattern;

            public readonly int[] MoveTable;

            public PatternInfo(byte[] pattern)
            {
                PatternLength = pattern.Length;
                BytesPattern = pattern;
                MoveTable = GetTableFormBag(PatternLength);
                var pattMaxIdx = PatternLength - 1;

                unsafe
                {
                    fixed (int* next = MoveTable)
                    {
                        fixed (byte* patt = pattern)
                        {
                            for (int i = 0; i < PatternLength; i++)
                            {
                                next[patt[i]] = pattMaxIdx - i;
                            }
                        }
                    }
                }

                Comparer = new ComparePatternFunc(CompareCore);
            }

            public PatternInfo(string pattern)
            {
                var strLen = pattern.Length;
                if (strLen == 0 || (strLen & 1) == 1) throw new FormatException("The length of pattern is 0 or not equal to this value division by 2.");
                var patternLength = strLen >> 1;
                var maxMove = patternLength - 1;
                var moveTable = GetTableFormBag(patternLength);

                Expression exp = Expression.LessThanOrEqual(
                    Expression.Constant(patternLength, typeof(int)),
                    _ExpSourceMaxCount);

                #region  generates move table and comparison expression
                unsafe
                {
                    fixed (int* next = moveTable)
                    {
                        fixed (char* patt = pattern)
                        {
                            var iPatt = (int*)patt;

                            var idx = 0;
                            while (idx < strLen)
                            {
                                var badMove = maxMove - (idx >> 1);
                                var currentChar = patt[idx++];
                                var nextChar = patt[idx++];
                                int nextDigit;
                                if (currentChar == '?')
                                {
                                    if (nextChar == '?') //??
                                    {
                                        SetMultiBadMove(next, badMove, 0, 1); //update move table
                                                                              //update expression
                                        exp = Expression.AndAlso(exp, _ExpUnconditionalTrue);
                                    }
                                    else //?a
                                    {
                                        nextDigit = GetHexDigit(nextChar);
                                        SetMultiBadMove(next, badMove, nextDigit, 0x10); //update move table
                                        exp = MakeExpCmpDigit(exp, nextDigit, 0x0F); //update expression
                                    }
                                }
                                else
                                {
                                    var firstDigit = GetHexDigit(currentChar) << 4;

                                    if (nextChar == '?') //a?
                                    {
                                        SetMultiBadMove(next, badMove, firstDigit, 1); //update move table
                                        exp = MakeExpCmpDigit(exp, firstDigit, 0xF0); //update expression
                                    }
                                    else //ab
                                    {
                                        nextDigit = GetHexDigit(nextChar);
                                        var hexNum = (byte)(firstDigit | nextDigit);
                                        next[hexNum] = badMove; //update move table
                                                                //update expression
                                        exp = Expression.AndAlso(
                                                exp,
                                                Expression.Equal(
                                                    _ExpArrayItemIterator,
                                                    Expression.Constant(hexNum, typeof(byte))));
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                Comparer = Expression.Lambda<ComparePatternFunc>(
                    exp, _ExpParamSource, _ExpUnusedParamBytesPattern, _ExpParamIndex)
                    .Compile();

                PatternLength = patternLength;
                MoveTable = moveTable;
                BytesPattern = null;
            }

            public bool CheckWithPattern(byte[] source, int index) => Comparer(source, BytesPattern, index);

            private static Expression MakeExpCmpDigit(Expression exp, int digit, int mask) => Expression.AndAlso(
                exp,
                Expression.Equal(
                    Expression.And(
                        _ExpArrayItemIterator,
                        Expression.Constant((byte)mask, typeof(byte))),
                    Expression.Constant((byte)digit, typeof(byte))));

            private static bool CompareCore(byte[] source, byte[] pattern, int index)
            {
                var length = pattern.Length;
                unsafe
                {
                    fixed (byte* src = source, patt = pattern)
                    {
                        for (var i = 0; i < length; i++)
                        {
                            if (src[index + i] != patt[i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }

            private unsafe static void SetMultiBadMove(int* moveTable, int badMove, int start, int step)
            {
                for (int i = start; i < 256; i += step)
                {
                    moveTable[i] = badMove;
                }
            }

            private static int GetHexDigit(char number)
            {
                if (number >= '0' && number <= '9')
                {
                    return number - '0';
                }
                else if ((number >= 'a' && number <= 'f') ||
                         (number >= 'A' && number <= 'F'))
                {
                    return (number & 7) + 9;     //  'a'=0x61, 'A'=0x41
                }
                throw new FormatException("Unexpected char in pattern.");
            }

            private static int[] GetTableFormBag(int patternLength)
            {
                var result = _MoveTablePool.TryTake(out var item) ? item : new int[256];
                unsafe
                {
                    fixed (int* buffer = result)
                    {
                        for (int i = 0; i < 256; i++)
                        {
                            buffer[i] = patternLength;
                        }
                    }
                }
                return result;
            }
            public void Dispose()
            {
                _MoveTablePool.Add(MoveTable);
            }
        }
    }
}
