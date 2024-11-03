using Models;
using Models.Interfaces.Context;
using NSubstitute;
using System.Collections.Immutable;

namespace Cli2Context.Tests;

public class ContextVariableWriterTests
{
    [Fact]
    public void SetVariableValue_SetsCorrectValue_WhenReplacingWholeVariable_AndScopeIsCommand()
    {
        var storage = new ContextVariableStorage();
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"), "description");
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal("value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
        Assert.Equal("description", storage.Changes.FirstOrDefault()?.Description);
    }

    [Fact]
    public void SetVariableValue_RemovesPreviousChanges_WhenReplacingWholeVariable_AndScopeIsCommand()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Description = "old description" });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"), "description");
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal("value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
        Assert.Equal("description", storage.Changes.FirstOrDefault()?.Description);
    }

    [Fact]
    public void SetVariableValue_ReadsDescriptionFromPreviousChanges_WhenReplacingWholeVariable()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Description = "old description" });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"));
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal("value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
        Assert.Equal("old description", storage.Changes.FirstOrDefault()?.Description);
    }

    [Fact]
    public void SetVariableValue_ReadsOtherPropertiesFromPreviousChanges_WhenReplacingWholeVariable()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Description = "old description", LocationId = "locationId", AllowedValues = [ "allowed1" ] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"));
        Assert.Equal(3, storage.Changes.Count);
        Assert.Equal("locationId", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.LocationId);
        Assert.Equal("allowed1", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.AllowedValues?.FirstOrDefault());
    }

    [Fact]
    public void SetVariableValueOnTopLevelVariable_SetsCorrectValue_WhenReplacingWholeVariable_AndScopeIsSession()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelVariable(VariableScope.Session, "test", new VariableValue("test value"), "description", new List<string>(), "locationId");
        Assert.Equal(2, storage.Changes.Count);
        Assert.Equal("test", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Key);
        Assert.Equal(VariableScope.Session, storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Scope);
        Assert.Equal("test value", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.TextValue);
        Assert.Equal("description", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Description);
    }

    [Fact]
    public void SetVariableValueOnTopLevelVariable_SetsCorrectValue_WhenReplacingWholeVariable_AndScopeIsApplication()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelVariable(VariableScope.Application, "test", new VariableValue("test value"), "description", new List<string>(), "locationId");
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal(VariableScope.Application, storage.Changes.FirstOrDefault()?.Scope);
        Assert.Equal("test value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
        Assert.Equal("description", storage.Changes.FirstOrDefault()?.Description);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsCommand_AndPreviousValueExisted()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("applicationKey", "property", "applicationPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelList(VariableScope.Command, "test[commandKey]", new VariableValueFactory().Object().AddTextProperty("key", "commandKey").AddTextProperty("property", "newCommandPropertyValue").Build(), "description", new List<string>(), "locationId");
        Assert.Equal(3, storage.Changes.Count);
        Assert.Equal("test", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.Key);
        Assert.Equal("newCommandPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("sessionPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("applicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsCommand_AndPreviousValueDidNotExist()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("appliactionKey", "property", "applicationPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelList(VariableScope.Command, "test[commandKey]", new VariableValueFactory().Object().AddTextProperty("key", "commandKey").AddTextProperty("property", "newCommandPropertyValue").Build(), "description", new List<string>(), "locationId");
        Assert.Equal(3, storage.Changes.Count);
        Assert.Equal("test", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.Key);
        Assert.Equal("newCommandPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("sessionPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("applicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }


    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsSession_AndPreviousValueExisted()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("applicationKey", "property", "applicationPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelList(VariableScope.Session, "test[sessionKey]", new VariableValueFactory().Object().AddTextProperty("key", "sessionKey").AddTextProperty("property", "newSessionPropertyValue").Build(), "description", new List<string>(), "locationId");
        Assert.Equal(2, storage.Changes.Count);
        Assert.Equal("newSessionPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("applicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsSession_AndPreviousValueDidNotExist()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("appliactionKey", "property", "applicationPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelList(VariableScope.Session, "test[sessionKey]", new VariableValueFactory().Object().AddTextProperty("key", "sessionKey").AddTextProperty("property", "newSessionPropertyValue").Build(), "description", new List<string>(), "locationId");
        Assert.Equal(2, storage.Changes.Count);
        Assert.Equal("newSessionPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("applicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsApplication_AndPreviousValueExisted()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("applicationKey", "property", "applicationPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelList(VariableScope.Application, "test[applicationKey]", new VariableValueFactory().Object().AddTextProperty("key", "applicationKey").AddTextProperty("property", "newApplicationPropertyValue").Build(), "description", new List<string>(), "locationId");
        Assert.Single(storage.Changes);
        Assert.Equal("newApplicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsApplication_AndPreviousValueDidNotExist()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), Description = "old description", LocationId = "locationId", AllowedValues = ["allowed1"] });
        var writer = new ContextVariableWriter(Substitute.For<IOutput>(), storage);
        writer.SetVariableValueOnTopLevelList(VariableScope.Application, "test[applicationKey]", new VariableValueFactory().Object().AddTextProperty("key", "applicationKey").AddTextProperty("property", "newApplicationPropertyValue").Build(), "description", new List<string>(), "locationId");
        Assert.Single(storage.Changes);
        Assert.Equal("newApplicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }
}
