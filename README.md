SqlMarshal
===========================

[![NuGet](https://img.shields.io/nuget/v/SqlMarshal.svg?style=flat)](https://www.nuget.org/packages/SqlMarshal/)

NativeAOT-friendly mini-ORM which care about nullability checks.

This project generates typed functions for accessing custom SQL and stored procedures. Goal of this project to be AOT friendly.
Database connection can be used from the DbContext of DbConnection objects.

# How to use

Add `SqlMarshal` Nuget package using

```
dotnet add package SqlMarshal
```

Then create your data context class inside your project.
```csharp
public class PersonInformation
{
    public int PersonId { get; set; }

    public string? PersonName { get; set; }
}

public partial class DataContext
{
    private DbConnection connection;

    public DataContext(DbConnection connection) => this.connection = connection;

    [SqlMarshal("persons_list")]
    public partial IList<PersonInformation> GetPersons();

    [SqlMarshal]
    public partial IList<PersonInformation> GetPersonFromSql([RawSql]string sql, int id);
}
```

# Temporary limitations or plans
Current version of library has several limitations which not because it cannot be implemented reasonably,
but because there was lack of time to think through all options. So I list all current limitations, so any user would be aware about them.
I think about these options like about plan to implement them.

- No ability to specify length of input/output string parameters, or type `varchar`/`nvarchar`.
- Simplified ORM for just mapping object properties from DbDataReader
- Ability to specify fields in code in the order different then returned from SQL.
- Automatic generation of DbSet<T> inside DbContext, since when working with stored procedures this is most likely burden.
- FormattableString support not implemented.

# Examples

- [DbConnection examples](#dbconnection-examples)
    - [Stored procedures which returns resultset](#stored-procedures-which-returns-resultset)
    - [Adding parameters](#Adding-parameters)
    - [Executing SQL](#Executing-SQL)
    - [Output parameters](#Output-parameters)
    - [Procedure which returns single row](#Procedure-which-returns-single-row)
    - [Scalar results](#Scalar-results)
    - [Sequences](#Sequence-results)
    - [INSERT or UPDATE](#Without-results)
    - [Join transactions](#Join-transactions)
- [DbContext examples](#dbcontext-examples)
    - [Stored procedures which returns resultset](#stored-procedures-which-returns-resultset-1)
    - [Adding parameters](#Adding-parameters-1)
    - [Output parameters](#Output-parameters-1)
    - [Procedure which returns single row](#Procedure-which-returns-single-row-1)
    - [Scalar results](#Scalar-results-1)
    - [INSERT or UPDATE](#Without-results-1)
    - [Join transactions](#Join-transactions-1)
- [Alternative options](#Alternative-options)
    - [Async methods](#Async-methods)
    - [Nullable parameters](#Nullable-parameters)
    - [Bidirectional parameters](#Bidirectional-parameters)
    - [Pass connection as parameter](#pass-connection-as-parameter)
    - [Pass transaction as parameter](#pass-transaction-as-parameter)
    - [CancellationToken support](#CancellationToken-support)

## Managing connections

Generated code does not interfere with the connection opening and closing. It is responsibility of developer to properly wrap code in the transaction and open connections.

```csharp
public partial class DataContext
{
    private DbConnection connection;

    public DataContext(DbConnection connection) => this.connection = connection;

    [SqlMarshal("persons_list")]
    public partial IList<Item> GetResult();
}
...

var connection = new SqlConnection("......");
connection.Open();
try
{
    var dataContext = new DataContext(connection);
    var items = dataContext.GetResult();
    // Do work on items here.
}
finally
{
    connection.Close();
}
```

Same rule applies to code which uses DbContext.

## Additional samples

In the repository located sample application which I use for testing, but they can be helpful as usage examples.

- https://github.com/kant2002/SqlMarshal/tree/main/SqlMarshal.CompilationTests

## Performance

Now I only hope (because no measurements yet) that performance would be on par with [Dapper](https://github.com/StackExchange/Dapper) or better.
At least right now generated code is visible and can be reason about.

## DbConnection examples

### Stored procedures which returns resultset

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_list")]
    public partial IList<Item> GetResult();
}
```

This code translated to `EXEC persons_list`.
When generated code retrieve data reader it starts iterating properties in the `Item` class in the
same order as they are declared and read values from the row. Order different then declaration order not supported now.

### Adding parameters

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

### Executing SQL

If stored procedure seems to be overkill, then you can add string parameter with attribute [RawSql]
and SQL passed to the function would be executed.

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal]
    public partial IList<PersonInformation> GetResultFromSql([RawSql]string sql, int maxId);
}
```

### Output parameters

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_search_ex")]
    public partial IList<Item> GetResults2(string name, string city, out int totalCount);
}
```

This code translated to `EXEC persons_search @name, @city, @total_count OUTPUT`.
Value returned in the @total_count parameter, saved to the `int totalCount` variable.

### Procedure which returns single row

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

### Scalar results

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

### Sequence results

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("total_orders")]
    public partial IList<string> GetStrings(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. First columns of the returning result set mapped to the sequence.
If you want return more then one columns, and do not want create classes, you can use tuples

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("total_orders")]
    public partial IList<(string, int)> GetPairs(int clientId);
}
```

### Join transactions

Not implemented.

### Without results

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("process_data")]
    public partial void ProcessData(int year);
}
```

This code translated to `EXEC process_data @year`. No data was returned, ExecuteNonQuery called. 

## DbContext examples

### Stored procedures which returns resultset

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_list")]
    public partial IList<Item> GetResult();
}
```

This code translated to `EXEC persons_list`.
Underlying assumption that in the custom context there definition of the `DbSet<Item>`.

### Adding parameters

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

### Output parameters

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_search_ex")]
    public partial IList<Item> GetResults2(string name, string city, out int totalCount);
}
```

This code translated to `EXEC persons_search @name, @city, @total_count OUTPUT`.
Value returned in the @total_count parameter, saved to the `int totalCount` variable.

### Procedure which returns single row

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

### Scalar results

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

### Without results

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("process_data")]
    public partial void ProcessData(int year);
}
```

This code translated to `EXEC process_data @year`. No data was returned, ExecuteNonQuery called. 

### Join transactions

Generated code automatically join any transaction opened using `DbContext.Database.BeginTransaction()`.


## Alternative options

### Async methods

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("total_orders")]
    public partial Task<int> GetTotal(int clientId);
}
```

or

```csharp
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_search")]
    public partial Task<IList<Item>> GetResults(string name, string city);
}
```

### Nullable parameters

The codegen honor nullable parameters. If you specify parameter as non-nullable, it will not work with NULL values in the database,
if you specify that null allowed, it properly convert NULL to null values in C#.

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("get_error_message")]
    public partial string? GetErrorMessage(int? clientId);
}
```

### Bidirectional parameters

If you have parameters which act as input and output parameters, you can specify them as `ref` values.
Codegen read values after SQL was executed.

```csharp
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("get_error_message")]
    public partial string? GetErrorMessage(ref int? clientId);
}
```

### Pass connection as parameter

Instead of having DbConnection as a field of the class, it can be passed as parameter, and even be placed in the extension method.

```csharp
public static partial class DataContext
{
    [SqlMarshal("persons_list")]
    public static partial IList<Item> GetResult(DbConnection connection);

    [SqlMarshal("persons_by_id")]
    public static partial Item GetResults(DbConnection connection, int personId);
}
```

### Pass transaction as parameter

If you want finegrained control over transactions, if you pass `DbTransaction` as parameter, generated code will set it to `DbCommand` or EF context will join that transaction using `Database.UseTransaction`.

```csharp
public static partial class DataContext
{
    [SqlMarshal("persons_list")]
    public static partial IList<Item> GetResult(DbTransaction tran);

    [SqlMarshal("persons_by_id")]
    public static partial Item GetResults(DbTransaction tran, int personId);
}
```

### CancellationToken support

You can add CancellationToken inside your code and it would be propagated inside ADO.NET calls.
You can use that with DbContext too.

```csharp
public static partial class DataContext
{
    [SqlMarshal("total_orders")]
    public partial Task<int> GetTotal(DbConnection connection, int clientId, CancellationToken cancellationToken);
}
```
