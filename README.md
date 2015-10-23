# CubeIt
A local multi-dimensional data cube for rapid querying.

Download using NuGet: [CubeIt](http://nuget.org/packages/CubeIt/)

## Overview
OLAP is a popular solution for quickly querying and filtering down on a set of data. Occasionally, we need the power of this platform when working with our data in-memory. CubeIt provides a low-tech, high-performance implementation geared towards making it easy to find just the data you're looking for.

Occasionally we know a lot about a piece of data and we want to ask complex questions about it. Typically, our code runs through a series of linear checks, filtering out any data that doesn't satisfy our conditions. For small amounts of data, this item-by-item checking is fine. However, when your dealing with an enormous amount of data *or* you need to ask a lots of questions, you're system will suddenly be overwhelmed.

The typical approach in programming is to create look-up tables. In C#, you do this with `Dictionary<TKey, TValue>`. As the number of questions grow and the size of the sample set increases your code becomes a tangled mess of look-up tables that are nearly impossible to manage. The moment you start considering nesting multiple levels of `Dictionary` it is probably time to look into CubeIt.

## How It Works
In order to quickly get at the data you're looking for, you must first build a "cube". A cube is a multi-dimensional look-up table. What does that mean? It means that each value you are looking for has a list of attributes that allow you to search for it. If you're familiar with SQL, each attribute is like a column that allows you to filter by it in a WHERE clause. You can filter down as much as you need, throwing away values that don't match your criteria. Once you finish filtering you can aggregate the data anyway you want to get at your answer. Most likely you will reuse the same cube to find answers to all your questions.

### Dimensions
With CubeIt, you define the dimensions you intend to search your data by. For example, imagine you're asked to summarize sales over the past year in different ways. You are asked to compare sales on a monthly basis (how did January compare to February?), on a weekly basis (which week of the month did best?) and on a daily basis (which days of the week saw the most sales?). In this case, you have three dimensions: month (1-12), week (1-4) and day of week (1-7). You can declare these dimensions like this:

    Dimension month = new Dimension(EqualityComparer<int>.Default);
    Dimension week = new Dimension(EqualityComparer<int>.Default);
    Dimension dayOfWeek = new Dimension(EqualityComparer<DayOfWeek>.Default);

Dimensions are completely arbitrary. You must ensure that you always use the same dimension when working with the cube. At first it might seem strange that your dimensions don't have a name or some other identifier. However, this actually makes working with cubes easier or more efficient. The only argument is an `IEqualityComparer` for comparing the look-up keys making up the dimension. Month and week are just numbers, which is why `EqualityComparer<int>` is used. The "day of week" dimension uses the built-in `DayOfWeek` enumeration, hence `EqualityComparer<DayOfWeek>`.

### Building a Cube
When looping through your data, you build up your cube one piece at a time. Here is the code that builds the cube from our example:

    Cube<decimal> result = Cube<decimal>.Define(month, week, dayOfWeek);
    foreach (Sale sale in getYearlySales())
    {
        int salesMonth = sale.Date.Month;
        TimeSpan timeSinceTheFirst = sale.Date - new DateTime(sale.Date.Year, salesMonth, 01);
        int week = Math.Min(4, timeSinceTheFirst.Days / 7) + 1;
        DayOfWeek dow = sale.Date.DayOfWeek;
        Key key = new Key(
            new KeyPart(month, salesMonth),
            new KeyPart(week, week),
            new KeyPart(dayOfWeek, dow));
        Cube<decimal> cube = Cube<decimal>.Singleton(key, sale.Amount);
        result = result.ResolvingMerge(group => group.Values.Sum(), cube);
    }
    return result;
    
Most of this code is figuring out what month, week and day that the sale occurred. What's important are the calls to `Define`, `Singleton` and `ResolvingMerge`. 

`Define` creates an empty `Cube` with the three dimensions we declared. `Cube`s are built by composing them from smaller parts. In this case, we are starting from nothing. `Merge` and `ResolvingMerge` expect that cubes to be merged have the same dimensions because it doesn't make sense to merge unrelated cubes. `Define` will make sure our initially empty cube has all the needed dimensions.

`Singleton` creates a cube that holds a single value. The value is associated with a single key. Each `Key` is comprised of zero or more `KeyPart`s and a `KeyPart` is a dimension and a value (phew). In the example, the "Month" dimension plus the value 1 (for January) make up a key part. Combined with the other key parts, we have a key. The same January key part would be reused for the other sales in January. As you can tell, a lot of effort can go into creating your keys.

Finally, the `ResolvingMerge` method merges the result cube with the singleton cube. Notice that this creates an entirely new cube, so we have to reassign `result` to the `ResolvingMerge`'s return value. The first argument is a function that tells cubes how to handle sales that occur on the same day. In this case, we are just adding the sales together. Depending on your situation, adding values together might not make sense.

If we can guarantee that our sales will always fall on different days, we can just call `Merge`. `Merge` doesn't worry about duplicates and treats them as errors. It is usually faster to call `Merge` because it performs less work when building your cubes.

### A Simple Performance Boost
It is possible to improve the performance of the code above. It is significantly faster to call `Merge` and `ResolvingMerge` once rather than multiple times for each value. The reason being that only one large merge needs to take place rather than lots of smaller ones. This reduces the number of throw-away cubes being built. Here is the revised code (it's almost the same):

    List<Cube<decimal>> cubes = new List<Cube<decimal>>();
    foreach (Sale sale in getYearlySales())
    {
        int salesMonth = sale.Date.Month;
        int daysSinceFirst = (sale.Date - new DateTime(sale.Date.Year, salesMonth, 01)).Days;
        int week = Math.Min(4, daysSinceFirst / 7) + 1;
        DayOfWeek dow = sale.Date.DayOfWeek;
        Key key = new Key(
            new KeyPart(month, salesMonth),
            new KeyPart(week, week),
            new KeyPart(dayOfWeek, dow));
        Cube<decimal> cube = Cube<decimal>.Singleton(key, sale.Amount);
        cubes.Add(cube);        
    }
    Cube<decimal> result = Cube<decimal>.Define(month, week, dayOfWeek);
    result = result.ResolvingMerge(group => group.Values.Sum(), cubes);
    return result;
    
### Summarizing Data By Month
In order to figure out sales on a month-by-month basis, we need to collapse the "Week" and "Day of Week" dimensions. When collapsing these dimensions, we simply want to sum the values appearing on different weeks and days. Here is the code:

    private static decimal sumGroup(Group<decimal> group)
    {
        return group => group.Pairs.Sum(pair => pair.Value);
    }
    ...
    Cube<decimal> sales = getYearlySalesCube();  // our code above
    Cube<decimal> monthlySales = sales.Collapse(week, sumGroup).Collapse(dayOfWeek, sumGroup);
    
I created a simple helper method called `sumGroup`. Whenever you collapse a dimension, the values are paired up with the key part value (week/day) they were associated with. In our case, we don't really care what the week or day was, so we just ignore the key part value. We call `Collapse` twice, collapsing the "Week" and "Day of Week" dimensions, in both cases just summing the values. As will all other cube operations, `Collapse` creates a new cube, so we store the result in the `monthlySales` variable.

At this point, our cube has a single dimension, "Month". We can ask for each month by calling the `GetUniqueKeyParts` method on the `monthlySales` cube:

    var months = monthlySales.GetUniqueKeyParts(month).Select(part => (int)part.Value);
    
From here, we can create the singleton keys for each month and grab the value for each:

    foreach (int salesMonth in months)
    {
        Key key = new Key(new KeyPart(month, salesMonth));
        decimal totalSales = monthlySales[key];
        Console.Out.WriteLine("{0}: {1:C}", salesMonth, totalSales);
    }
    
Or, we can do this more succinctly using:

    foreach (Key key in monthlySales.GetKeys())
    {
        int salesMonth = (int)key.GetKeyPart(month).Value;
        decimal totalSales = monthlySales[key];
        Console.Out.WriteLine("{0}: {1:C}", salesMonth, totalSales);
    }

## License
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
