namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Provides the class generator options which are needed to generate a class.
/// </summary>
public sealed class ClassGeneratorOptions
{
    /// <summary>
    /// Gets the desired output path.
    /// </summary>
    /// <remarks>
    /// The generated classes are saved in this directory.
    /// </remarks>
    public required string Output { get; init; }

    /// <summary>
    /// Gets the desired namespace of the classes.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a sealed class should be created.
    /// </summary>
    public required bool SealedClass { get; init; }

    /// <summary>
    /// Gets the desired modifier.
    /// </summary>
    public required string Modifier { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a class should be created which can be used with <i>Entity Framework Core</i>.
    /// </summary>
    public required bool DbModel { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a column attribute should be added to each property.
    /// </summary>
    public required bool AddColumnAttribute { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a backing field should be created for each property.
    /// </summary>
    public required bool WithBackingField { get; init; }

    /// <summary>
    /// Gets the value which indicates whether the <i>SetProperty</i> function of the <a href="https://learn.microsoft.com/de-de/dotnet/communitytoolkit/mvvm/">CommunityToolKit.MVVM</a> should be used.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: If the value is set to <see langword="true"/>, a backing field is automatically created, even if the value of <see cref="WithBackingField"/> is set to <see langword="false"/>.
    /// </remarks>
    public required bool AddSetProperty { get; init; }

    /// <summary>
    /// Gets the value which indicates whether an empty <i>summary</i> should be added to the class and each property.
    /// </summary>
    public required bool AddSummary { get; init; }

    /// <summary>
    /// Gets the value which indicates whether the output directory should be cleared before the export starts.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: If set to <see langword="true"/>, all <c>*.cs</c> files in the output directory are irretrievably deleted!
    /// </remarks>
    public required bool EmptyOutputDirectoryBeforeExport { get; init; }

    /// <summary>
    /// Gets the value which indicates whether the table name should be added in the class summary.
    /// </summary>
    public required bool AddTableNameToClassSummary { get; init; }
}
