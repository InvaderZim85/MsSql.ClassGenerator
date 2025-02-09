# MsSql.ClassGenerator

**Content**

<!-- TOC -->

- [General](#general)
- [Usage](#usage)
    - [Parameter](#parameter)
        - [Modifier](#modifier)
        - [Log level](#log-level)
        - [Filter](#filter)
- [Planned features](#planned-features)
- [Sources / Other](#sources--other)
    - [Icon](#icon)

<!-- /TOC -->

---

## General

With the help of this program you can easily generate C# classes from database tables.

You can select which tables are to be used. In addition, you can use a separate *alias* for each table or column name.

The program is the *little brother* of the program [MsSqlToolBelt](https://github.com/InvaderZim85/MsSqlToolBelt).

**Note**: This programme only supports MS SQL databases.

## Usage

The programme can be used in two ways:

1. desktop application
2. via console (*cmd*, *powershell*, etc.)

### Parameter

The following parameters are provided by the program:

| Parameter | Description | Example | Program |
|---|---|---|---|
| `-s` / `--server` | The name / path of the MS SQL Server - Mandatory for CLI / Optional for WPF App. | `-s (localdb)\MsSqlLocalDb` | Both |
| `-d` / `--database` | The name of the desired database - Mandatory for CLI / Optional for WPF App. | `-d AdventureWorks` | Both |
| `-o` / `--output` | The path of the output directory wich should hold the genereated classes - Mandatory. | `D:\SomeDir` | Only CLI |
| `-n` / `--namespace` | The desired `namespace` which should be used - Mandatory. | `-n MyNameSpace` | Only CLI |
| `-m` / `--modifier` | The desired modifier which should be used - Optional; Default = `public`. See [Modifier](#modifier). | `-m internal` | Only CLI |
| `-f` / `--filter` | The filter which should be used to determine the tables - Optional; Default = *Empty*. See [Filter](#filter) | `-f Person*` | Only CLI |
| `--sealed` | Adds the `sealed` modifiert to the classes - Optinal; Default = `false`. | `--sealed` | Only CLI |
| `--db-model` | Adds all neded attribute to use the class with *Entity Framework Core*. - Optional; Default = `false` | `--db-model` | Only CLI |
| `--column-attribute` | Adds a *Column* attribute to each property. - Optional; Default `false` | `--column-attribute` | Only CLI |
| `--backing-field` | Creates a *backing field* for each property. - Optional; Default = `false` | `--backing-field` | Only CLI |
| `--set-property` | Adds the `SetProperty` method to the properties. **Note**: To use this function, you need to add the [CommunityToolkit.Mvvm](https://learn.microsoft.com/de-de/dotnet/communitytoolkit/mvvm/) to your project or implement it by yourself. - Optional; Default = `false` | `--set-property` | Only CLI |
| `--summary` | Adds a summary to each property and to the class. - Optional; Default = `false` | `--summary` | Only CLI |
| `-c` / `--clean` | Specifies if the output directory should be *cleaned* before the class are generated. **Note**: All `*.cs` files in the specified directory will be deleted irretrievable! - Optional; Default = `false` | `-c` | Only CLI |
| `--table-name` | Adds the name of the table to the class summary. - Optional; Default = `false` | `--table-name` | Only CLI |
| `-l` / `--log-level` | The desired *min.* log level. See [Log level](#log-level) - Optional; Default = `2` | `-l 0` | Both |

#### Modifier

The following modifier are supported:

- `public` 
- `internal`
- `protected`
- `protected internal`

#### Log level

| Id | Name |
|---|---|
| 0 | Verbose |
| 1 | Debug |
| 2 | Information (default) |
| 3 | Warning |
| 4 | Error |
| 5 | Fatal |

#### Filter

You can use the filter als follows:

| Example | Description |
|---|---|
| `value` | The name of the table must match the filter value. |
| `value*` | The name of the table must beginn with the filter value. |
| `*value` | The name of the table must end with the filter value. |
| `*value*` | The name of the table must contain the filter value. |

## Planned features

1. Color selection
2. Update info

## Sources / Other

### Icon

Special thank to [Hopstarter](https://www.flaticon.com/authors/hopstarter) for the icon > [Pyhton icons created by Hopstarter - Flaticon](https://www.flaticon.com/free-icons/phyton)