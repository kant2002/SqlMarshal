SqlMarshal
===========================

[![NuGet](https://img.shields.io/nuget/v/StoredProcedureSourceGenerator.svg?style=flat)](https://www.nuget.org/packages/StoredProcedureSourceGenerator/)

This project generates typed functions for accessing stored procedures. Goal of this project to be AOT friendly.
Database connection can be used from the DbContext of DbConnection objects.

# Examples

- [DbConnection examples]
    - [Stored procedures which returns resultset](Stored procedures which returns resultset)
- (DbContext examples)[DbContext examples]
    - [Stored procedures which returns resultset](Stored procedures which returns resultset)
    - Adding parameters
    - Output parameters
    - Procedure which returns single row
    - Scalar resuls

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
