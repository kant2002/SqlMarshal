SqlMarshal
===========================

[![NuGet](https://img.shields.io/nuget/v/StoredProcedureSourceGenerator.svg?style=flat)](https://www.nuget.org/packages/StoredProcedureSourceGenerator/)

This project generates function for accessing stored procedures. Goal of this project to be AOT friendly.
For now only access to EF Core DbContext is available, but in principle this project can be adapted to map
values directly to DbConnection.

# Examples

## Stored procedures which returns resultset

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

## Adding parameters

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [StoredProcedureGenerated("persons_search")]
    public partial IList<Item> GetResults(string name, string city);
}
```

This code translated to `EXEC persons_search @name, @city`. Generated code do not use named parameters.

## Procedure which returns single row

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [StoredProcedureGenerated("persons_by_id")]
    public partial Item GetResults(int personId);
}
```

This code translated to `EXEC persons_by_id @person_id`. From mapped result set taken just single item, first one.

## Scalar resuls

```
public partial class DataContext
{
    private CustomDbContext dbContext;

    [StoredProcedureGenerated("total_orders")]
    public partial int GetTotal(int clientId);
}
```

This code translated to `EXEC total_orders @client_id`. Instead of executing over data reader, ExecuteScalar called. 

## Output parameters

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
