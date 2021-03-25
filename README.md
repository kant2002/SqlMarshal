SqlMarshal
===========================

[![NuGet](https://img.shields.io/nuget/v/StoredProcedureSourceGenerator.svg?style=flat)](https://www.nuget.org/packages/StoredProcedureSourceGenerator/)

This project generates typed functions for accessing stored procedures. Goal of this project to be AOT friendly.
Database connection can be used from the DbContext of DbConnection objects.

# Examples

- [DbConnection examples](#dbconnection-examples)
    - [Stored procedures which returns resultset](#stored-procedures-which-returns-resultset)
    - [Adding parameters](#Adding-parameters)
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

# Temporary limitations
Current version of library has several limitation which not becasue it cannot be implemented reasonably,
but because there was lack ot time to think through all options. So I list all current limitations, so any user would be aware
- No ability to specify length of intput/output string parameters, or type `varchar`/`nvarchar`.
- No ability execute custom SQL
- Simplified ORM for just mapping object properties from DbDataReader
- Ability to specify fields in code in the order different then returned from SQL.

## DbConnection examples

### Stored procedures which returns resultset

```
public partial class DataContext
{
    private DbConnection connection;

    [StoredProcedureGenerated("persons_list")]
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

    [StoredProcedureGenerated("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

### Output parameters

```
public partial class DataContext
{
    private DbConnection connection;

    [StoredProcedureGenerated("persons_search_ex")]
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

    [StoredProcedureGenerated("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

### Scalar resuls

```
public partial class DataContext
{
    private DbConnection connection;

    [StoredProcedureGenerated("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

### Without results

```
public partial class DataContext
{
    private DbConnection connection;

    [StoredProcedureGenerated("process_data")]
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

    [StoredProcedureGenerated("persons_list")]
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

    [StoredProcedureGenerated("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

### Output parameters

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [StoredProcedureGenerated("persons_search_ex")]
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

    [StoredProcedureGenerated("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

### Scalar resuls

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [StoredProcedureGenerated("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

### Without results

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [StoredProcedureGenerated("process_data")]
    public partial void ProcessData(int year);
}
```

This code translated to `EXEC process_data @year`. No data was returned, ExecuteNonQuery called. 
