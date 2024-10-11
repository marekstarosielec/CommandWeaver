// using Models.Interfaces.Context;
//
// namespace Models;
//
// public class ValueResolver<T>(string name, string? shortName, string description): ValueResolver(name, shortName, description)
// {
//     public Type Type { get; } = typeof(T);
//     public T? Value { get; set; }
//
//     public T Resolve(IContext context)
//     {
//         if (Value != null)
//             return Value;
//         return default!;
//     }
// }
//
// public abstract class ValueResolver(string name, string? shortName, string description)
// {
//     public string Name { get; } = name;
//     public string Description { get; } = description;
//     public string? StringValue { get; set; }
// }