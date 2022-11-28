using System;
using System.Runtime.CompilerServices;

namespace TextSearchApp.Host.Common;

/// <summary> Методы-расширения для упрощения валидации входных данных. </summary>
    public static class ValidationExtensions
    {
        /// <summary> Выбрасывает <see cref="ArgumentNullException"/> если <paramref name="source"/> = <see langword="null"/>. </summary>
        public static T EnsureNotNull<T>(this T source, [CallerArgumentExpression("source")] string paramName = "source")
            => source ?? throw new ArgumentNullException(paramName);

        /// <summary> Выбрасывает <see cref="ArgumentException"/> если строка <paramref name="source"/> = <see langword="null"/> или пуста. </summary>
        public static string EnsureNotNullOrWhiteSpace(this string source, [CallerArgumentExpression("source")] string paramName = "source")
            => source is null
                ? throw new ArgumentNullException(paramName)
                : string.IsNullOrWhiteSpace(source)
                    ? throw new ArgumentException($"{paramName} is whitespace or empty.", paramName)
                    : source;
    }