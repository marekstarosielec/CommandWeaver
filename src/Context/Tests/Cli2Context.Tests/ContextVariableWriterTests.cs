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
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"));
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal("value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
    }

    [Fact]
    public void SetVariableValue_RemovesPreviousChanges_WhenReplacingWholeVariable_AndScopeIsCommand()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"));
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal("value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
    }

    [Fact]
    public void SetVariableValue_ReadsDescriptionFromPreviousChanges_WhenReplacingWholeVariable()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"));
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal("value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
    }

    [Fact]
    public void SetVariableValue_ReadsOtherPropertiesFromPreviousChanges_WhenReplacingWholeVariable()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValue(VariableScope.Command, "test", new VariableValueFactory().SingleTextValue("value"));
        Assert.Equal(3, storage.Changes.Count);
        Assert.Equal("locationId", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Command)?.LocationId);
    }

    [Fact]
    public void SetVariableValueOnTopLevelVariable_SetsCorrectValue_WhenReplacingWholeVariable_AndScopeIsSession()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelVariable(VariableScope.Session, "test", new DynamicValue("test value"));
        Assert.Equal(2, storage.Changes.Count);
        Assert.Equal("test", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Key);
        Assert.Equal(VariableScope.Session, storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Scope);
        Assert.Equal("test value", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelVariable_SetsCorrectValue_WhenReplacingWholeVariable_AndScopeIsApplication()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelVariable(VariableScope.Application, "test", new DynamicValue("test value"));
        Assert.Single(storage.Changes);
        Assert.Equal("test", storage.Changes.FirstOrDefault()?.Key);
        Assert.Equal(VariableScope.Application, storage.Changes.FirstOrDefault()?.Scope);
        Assert.Equal("test value", storage.Changes.FirstOrDefault()?.Value?.TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsCommand_AndPreviousValueExisted()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("applicationKey", "property", "applicationPropertyValue").Build(), LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelList(VariableScope.Command, "test[commandKey]", new VariableValueFactory().Object().AddTextProperty("key", "commandKey").AddTextProperty("property", "newCommandPropertyValue").Build());
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
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("appliactionKey", "property", "applicationPropertyValue").Build(), LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelList(VariableScope.Command, "test[commandKey]", new VariableValueFactory().Object().AddTextProperty("key", "commandKey").AddTextProperty("property", "newCommandPropertyValue").Build());
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
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("applicationKey", "property", "applicationPropertyValue").Build(), LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelList(VariableScope.Session, "test[sessionKey]", new VariableValueFactory().Object().AddTextProperty("key", "sessionKey").AddTextProperty("property", "newSessionPropertyValue").Build());
        Assert.Equal(2, storage.Changes.Count);
        Assert.Equal("newSessionPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("applicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsSession_AndPreviousValueDidNotExist()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("appliactionKey", "property", "applicationPropertyValue").Build(), LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelList(VariableScope.Session, "test[sessionKey]", new VariableValueFactory().Object().AddTextProperty("key", "sessionKey").AddTextProperty("property", "newSessionPropertyValue").Build());
        Assert.Equal(2, storage.Changes.Count);
        Assert.Equal("newSessionPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Session)?.Value?.ListValue?.Single()["property"].TextValue);
        Assert.Equal("applicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsApplication_AndPreviousValueExisted()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Application, Value = new VariableValueFactory().List().AddElementWithTextProperty("applicationKey", "property", "applicationPropertyValue").Build(), LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelList(VariableScope.Application, "test[applicationKey]", new VariableValueFactory().Object().AddTextProperty("key", "applicationKey").AddTextProperty("property", "newApplicationPropertyValue").Build());
        Assert.Single(storage.Changes);
        Assert.Equal("newApplicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }

    [Fact]
    public void SetVariableValueOnTopLevelList_SetsCorrectValue_WhenScopeIsApplication_AndPreviousValueDidNotExist()
    {
        var storage = new ContextVariableStorage();
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Command, Value = new VariableValueFactory().List().AddElementWithTextProperty("commandKey", "property", "commandPropertyValue").Build(), LocationId = "locationId" });
        storage.Changes.Add(new Variable { Key = "test", Scope = VariableScope.Session, Value = new VariableValueFactory().List().AddElementWithTextProperty("sessionKey", "property", "sessionPropertyValue").Build(), LocationId = "locationId" });
        var writer = new ContextVariableWriter(Substitute.For<IContext>(), storage);
        writer.WriteVariableValueOnTopLevelList(VariableScope.Application, "test[applicationKey]", new VariableValueFactory().Object().AddTextProperty("key", "applicationKey").AddTextProperty("property", "newApplicationPropertyValue").Build());
        Assert.Single(storage.Changes);
        Assert.Equal("newApplicationPropertyValue", storage.Changes.FirstOrDefault(v => v.Scope == VariableScope.Application)?.Value?.ListValue?.Single()["property"].TextValue);
    }
}
