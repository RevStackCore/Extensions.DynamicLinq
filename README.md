# RevStackCore.Extensions.DynamicLinq

[![Build status](https://ci.appveyor.com/api/projects/status/o5d37k2mty0as2vf?svg=true)](https://ci.appveyor.com/project/tachyon1337/extensions-dynamiclinq)

An AspNetCore quick and dirty OData replacement for queryable APIs. Works for standard POCO entities. No EDM hassles. No middleware configuration.
However, this is not an OData parser. Although $top,$skip,$orderBy work just like OData syntax, $filter is not an OData AST parser. Instead it works by a custom string deserialization implementation for parsing by the System.Linq.Dynamic IQueryable extensions.

# Installation

``` bash



```

# $filter

The $filter querystring should be a stringified array of QueryFilterOptions

```cs
public class QueryFilterOptions
{
    public FilterOperation Operation { get; set; }
    public string Property { get; set; }
    public object Value { get; set; }
    public string Sql { get; set; }
    public FilterTransform? Transform { get; set; }
}

public enum FilterOperation
{
    Eq,
    Ne,
    Gt,
    Ge,
    Lt,
    Le,
    Contains,
    StartsWith,
    EndsWith,
    Sql,
    SqlUnion
}
public enum FilterTransform
{
    Lower,
    Upper,
    Trim
}
```


# Usage


## Controller

Importing the library introduces two extension methods on Http Request object.

### ApplyTo

```cs
IQueryable<TEntity> ApplyTo<TEntity>(this HttpRequest request, IQueryable<TEntity> query)

```

### QueryResult

```cs
QueryResult<TEntity> PageResult<TEntity>(this HttpRequest request, IQueryable<TEntity> query)

```

QueryResult has the same structure as the OData PageResult

```cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RevStackCore.Extensions.DynamicLinq;

namespace MyApp.Controllers
{

    [Route("api/[controller]")]
    public class MyEntityController : Controller
    {
        private readonly IService _service;
        public MyEntityController(IService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<QueryResult<MyEntity>> Get()
        {
            var query = await _service.GetAsync();
            query = query.AsQueryable();
            return Request.PageResult(query);
        }

    }
}

```

## Client-Side(Javascript)


```js

let endpoint='/api/MyEntity';
let filterModel=[{
    property:'Name',
    operation:'Contains',
    transform:'Lower',
    value:'bob'
}];
let $filter = JSON.stringify(filterModel);
$filter=encodeURIComponent($filter);
let query=endpoint + "/?$filter=" + $filter + "&$top=100&$orderby='Name'";

fetch(query, {
    method: 'get',
    headers: new Headers({
        'Content-Type': 'application/json'
    })
})
.then(function(response){
    //response
})
.catch(function(err){
    //error
})


```




