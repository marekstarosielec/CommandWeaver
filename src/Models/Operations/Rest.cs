// namespace Models.Operations;
//
// public class Rest: CommandOperation
// {
//     //TODO: Add test - parameters in all operations should have setter, otherwise deserialize cannot fill it.
//     public RestParameters Parameters { get; set; } = new();
//     
//     public override void Run(Context environment)
//     {
//         Console.WriteLine("Running rest call");
//         var url = Parameters.Url.Resolve(environment);
//         Console.WriteLine($"Url: {url}");
//     }
// }