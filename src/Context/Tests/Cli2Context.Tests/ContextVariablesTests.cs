﻿//using Models;
//using Models.Interfaces.Context;
//using NSubstitute;
//using System.Collections.Immutable;

//namespace Cli2Context.Tests;

//public class ContextVariablesTests
//{
//    [Fact]
//    public void ContextVariables_ReturnsCurrentSessionName_IfItWasDefined()
//    {
//        var contextVariableStorage = new ContextVariableStorage
//        {
//            BuiltIn = new List<Variable> {
//            new Variable {
//                Key = "currentSessionName",
//                Value = new DynamicValue("testValue")
//            } }.ToImmutableList()
//        };
//        var contextVariables = new ContextVariables(Substitute.For<IContext>(), contextVariableStorage);

//        Assert.Equal("testValue", contextVariables.CurrentSessionName);
//    }

//    [Fact]
//    public void ContextVariables_ReturnsDefaultCurrentSessionName_IfItWasNotDefined()
//    {
//        var contextVariables = new ContextVariables(Substitute.For<IContext>());
//        Assert.Equal("session1", contextVariables.CurrentSessionName);
//    }

//    [Fact]
//    public void ContextVariables_UpdatesTextVariable_WhenItWasNotDefined()
//    {
//        var contextVariableStorage = new ContextVariableStorage
//        {
//            BuiltIn = ImmutableList<Variable>.Empty
//        };
//        var contextVariables = new ContextVariables(Substitute.For<IContext>(), contextVariableStorage);
//        contextVariables.WriteVariableValue(VariableScope.Command, "testVariable", new DynamicValue("variableText"));
//        Assert.Single(contextVariableStorage.Changes);
//        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
//        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
//        Assert.Equal("variableText", contextVariableStorage.Changes.First().Value?.TextValue);
//    }

//    [Fact]
//    public void ContextVariables_UpdatesTextVariable_WhenItWasAlreadyDefined()
//    {
//        var contextVariableStorage = new ContextVariableStorage
//        {
//            BuiltIn = new List<Variable> {
//            new Variable {
//                Key = "testVariable",
//                Value = new DynamicValue("testValue")
//            } }.ToImmutableList()
//        };
//        var contextVariables = new ContextVariables(Substitute.For<IContext>(), contextVariableStorage);
//        contextVariables.WriteVariableValue(VariableScope.Command, "testVariable", new DynamicValue("variableText"));
//        Assert.Equal("testVariable", contextVariableStorage.BuiltIn.First().Key);
//        Assert.Equal("testValue", contextVariableStorage.BuiltIn.First().Value?.TextValue);

//        Assert.Single(contextVariableStorage.Changes);
//        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
//        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
//        Assert.Equal("variableText", contextVariableStorage.Changes.First().Value?.TextValue);
//    }

//    [Fact]
//    public void ContextVariables_UpdatesObjectVariable_WhenItWasNotDefined()
//    {
//        var newVariableValue = new VariableValueFactory().Object().AddTextProperty("objectProperty", "objectPropertyValue").Build();
//        var contextVariableStorage = new ContextVariableStorage
//        {
//            BuiltIn = ImmutableList<Variable>.Empty
//        };
//        var contextVariables = new ContextVariables(Substitute.For<IContext>(), contextVariableStorage);
//        contextVariables.WriteVariableValue(VariableScope.Command, "testVariable", newVariableValue);
//        Assert.Single(contextVariableStorage.Changes);
//        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
//        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
//        Assert.Equal("objectPropertyValue", contextVariableStorage.Changes.First().Value?.ObjectValue?["objectProperty"]?.TextValue);
//    }

//    [Fact]
//    public void ContextVariables_UpdatesObjectVariable_WhenItWasAlreadyDefined()
//    {
//        var newVariableValue = new VariableValueFactory().Object().AddTextProperty("objectProperty", "objectPropertyValue").Build();
//        var contextVariableStorage = new ContextVariableStorage
//        {
//            BuiltIn = new List<Variable> {
//            new Variable {
//                Key = "testVariable",
//                Value = new VariableValueFactory().Object().AddTextProperty("objectProperty", "old").Build(),
//            } }.ToImmutableList()
//        };
//        var contextVariables = new ContextVariables(Substitute.For<IContext>(), contextVariableStorage);
//        contextVariables.WriteVariableValue(VariableScope.Command, "testVariable", newVariableValue);
//        Assert.Single(contextVariableStorage.Changes);
//        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
//        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
//        Assert.Equal("objectPropertyValue", contextVariableStorage.Changes.First().Value?.ObjectValue?["objectProperty"]?.TextValue);
//    }

//    [Fact]
//    public void ContextVariables_UpdatesListVariable_WhenItWasNotDefined()
//    {
//        var newVariableValue = new VariableValueFactory().List().AddElementWithTextProperty("firstElement", "property", "propertyValue1").AddElementWithTextProperty("secondElement", "property", "propertyValue2").Build();
//        var contextVariableStorage = new ContextVariableStorage
//        {
//            BuiltIn = ImmutableList<Variable>.Empty
//        };
//        var contextVariables = new ContextVariables(Substitute.For<IContext>(), contextVariableStorage);
//        contextVariables.WriteVariableValue(VariableScope.Command, "testVariable", newVariableValue);
//        Assert.Single(contextVariableStorage.Changes);
//        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
//        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
//        Assert.Equal("propertyValue1", contextVariableStorage.Changes.First().Value?.ListValue?.FirstOrDefault()?["property"]?.TextValue);
//        Assert.Equal("propertyValue2", contextVariableStorage.Changes.First().Value?.ListValue?.LastOrDefault()?["property"]?.TextValue);
//    }

//}
