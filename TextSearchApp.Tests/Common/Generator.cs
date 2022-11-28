using Bogus;

namespace TextSearchApp.Tests.Common;

/// <summary>
/// Генератор фейковых данных.
/// </summary>
public static class Generator
{
    /// <summary> Генератор данных с русской локализацией. </summary>
    public static readonly Faker FakerRu = new("ru");
}