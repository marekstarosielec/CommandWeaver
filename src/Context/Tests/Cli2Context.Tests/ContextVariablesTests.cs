// using Models;
//
// namespace Cli2Context.Tests;
//
// public class ContextVariablesTests
// {
//     [Fact]
//     public void ContextVariables_ReturnsValueFromBuiltIn_IfOthersWereNotProvided()
//     {
//         var sut = new ContextVariables();
//         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
//         Assert.Equal("value", sut.GetVariable("key")?.Value as string);
//     }
//     
//     [Fact]
//     public void ContextVariables_ReturnsValueFromLocal_IfSessionWasNotProvided()
//     {
//         var sut = new ContextVariables();
//         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
//         sut._local.Add(new Variable { Key = "key", Value = "value2"});
//
//         Assert.Equal("value2", sut.GetVariable("key")?.Value as string);
//     }
//     
//     [Fact]
//     public void ContextVariables_ReturnsValueFromSession_IfSessionWasProvided()
//     {
//         var sut = new ContextVariables();
//         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
//         sut._local.Add(new Variable { Key = "key", Value = "value2"});
//         sut._session.Add(new Variable { Key = "key", Value = "value3"});
//
//         Assert.Equal("value3", sut.GetVariable("key")?.Value as string);
//     }
//     
//     [Fact]
//     public void ContextVariables_ReturnsCurrentSessionName_IfItWasDefined()
//     {
//         var sut = new ContextVariables();
//         sut._builtIn.Add(new Variable { Key = "currentSessionName", Value = "value"});
//         Assert.Equal("value", sut.CurrentSessionName);
//     }
//     
//     [Fact]
//     public void ContextVariables_ReturnsDefaultCurrentSessionName_IfItWasNotDefined()
//     {
//         var sut = new ContextVariables();
//         Assert.Equal("session1", sut.CurrentSessionName);
//     }
// }