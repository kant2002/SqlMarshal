SqlMarshal
===========================

[![NuGet](https://img.shields.io/nuget/v/StoredProcedureSourceGenerator.svg?style=flat)](https://www.nuget.org/packages/StoredProcedureSourceGenerator/)

NativeAOT-friendly mini-ORM which care about nullability checks.

This project generates typed functions for accessing custom SQL and sstored procedures. Goal of this project to be AOT friendly.
Database connection can be used from the DbContext of DbConnection objects.

# Examples

- [DbConnection examples](#dbconnection-examples)
    - [Stored procedures which returns resultset](#stored-procedures-which-returns-resultset)
    - [Adding parameters](#Adding-parameters)
    - [Executing SQL](#Executing-SQL)
    - [Output parameters](#Output-parameters)
    - [Procedure which returns single row](#Procedure-which-returns-single-row)
    - [Scalar resuls](#Scalar-resuls)
    - [INSERT or UPDATE](#Without-results)
- [DbContext examples](#dbcontext-examples)
    - [Stored procedures which returns resultset](#stored-procedures-which-returns-resultset-1)
    - [Adding parameters](#Adding-parameters-1)
    - [Output parameters](#Output-parameters-1)
    - [Procedure which returns single row](#Procedure-which-returns-single-row-1)
    - [Scalar resuls](#Scalar-resuls-1)
    - [INSERT or UPDATE](#Without-results-1)
- [Alternative options](#Alternative-options)
    - [Async methods](#Async-methods)
    - [Nullable parameters](#Nullable-parameters)
    - [Bidirectional parameters](#Bidirectional-parameters)

# Temporary limitations or plans
Current version of library has several limitation which not because it cannot be implemented reasonably,
but because there was lack ot time to think through all options. So I list all current limitations, so any user would be aware about them.
I think about these options like about plan to implement them.

- No ability to specify length of intput/output string parameters, or type `varchar`/`nvarchar`.
- No ability execute custom SQL
- Simplified ORM for just mapping object properties from DbDataReader
- Ability to specify fields in code in the order different then returned from SQL.
- Automatic generation of DbSet<T> inside DbContext, since when working with stored procedures this is most likely burden.
- FormattableString support not implemented.

## Managing connections

Generated code do not interfere in the connection opening, closing process. It is responsibility of developer to properly wrap code in the transaction and open connections.

```
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
    var items = connection.GetResult();
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

Now I only hope (becasue no measurements yet) that performance would be on par with [Dapper](https://github.com/StackExchange/Dapper) or better.
At least right now generated code is visible and can be reason about.

## DbConnection examples

### Stored procedures which returns resultset

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_list")]
    public partial IList<Item> GetResult();
}
```

This code translated to `EXEC persons_list`.
When generated code retreive data reader it starts iterate properties in the `Item` class in the
same order as they are declared and read values from the row. Order different then declaration order do not supported now.

### Adding parameters

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

### Executing SQL

If stored procedure seems to be overkill, then you can add string parameter with attibute [RawSql]
and SQL passed to the function would be executed.

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal]
    public partial IList<PersonInformation> GetResultFromSql([RawSql]string sql, int maxId);
}
```

### Output parameters

```
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

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

### Scalar resuls

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

### Without results

```
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

```
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

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

### Output parameters

```
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

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

### Scalar resuls

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

### Without results

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("process_data")]
    public partial void ProcessData(int year);
}
```

This code translated to `EXEC process_data @year`. No data was returned, ExecuteNonQuery called. 


## Alternative options

### Async methods

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("total_orders")]
    public partial Task<int> GetTotal(int clientId);
}
```

or

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [SqlMarshal("persons_search")]
    public partial Task<IList<Item>> GetResults(string name, string city);
}
```

### Nullable parameters

The codegen honor nullable parameters. If you specify paramter as non-nullable, it would not work with NULL values in the database,
if you specify that null allowed, it properly convert NULL to null values in C#.

```
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

```
public partial class DataContext
{
    private DbConnection connection;

    [SqlMarshal("get_error_message")]
    public partial string? GetErrorMessage(ref int? clientId);
}
```
