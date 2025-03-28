using System.Collections.Immutable;
using NSubstitute;

public class ReaderTests
{
    private readonly IOutputService _outputService = Substitute.For<IOutputService>();
    private readonly IVariableStorage _variableStorage = Substitute.For<IVariableStorage>();
    private readonly Reader _reader;

    public ReaderTests()
    {
        // Initialize all variable storages with empty lists to avoid null issues.
        _variableStorage.BuiltIn.Returns(new List<Variable>().ToImmutableList());
        _variableStorage.Application.Returns(new List<Variable>());
        _variableStorage.Session.Returns(new List<Variable>());
        _variableStorage.Command.Returns(new List<Variable>());

        _reader = new Reader(_outputService, _variableStorage);
    }

    [Fact]
    public void ReadVariableValue_ShouldReturnTextValue_WhenSimpleKeyIsProvided()
    {
        // Arrange
        var variable = new Variable { Key = "simpleKey", Value = new DynamicValue("SimpleText") };
        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{simpleKey}}"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SimpleText", result.TextValue);
    }
    
    [Fact]
    public void ReadVariableValue_ShouldReplaceNonExistingVariablesWithEmptyText_WhenItIsPartOfBiggerText()
    {
        // Arrange
        _variableStorage.Command.Returns(new List<Variable>());

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{simpleKey}}abc"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("abc", result.TextValue);
    }
    [Fact]
    public void ReadVariableValue_ShouldReturnDateTimeValue_WhenKeyRefersToDateTime()
    {
        // Arrange
        var dateTimeValue = new DynamicValue(new DateTimeOffset(2023, 11, 29, 12, 0, 0, TimeSpan.Zero));
        var variable = new Variable { Key = "dateTimeKey", Value = dateTimeValue };
        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{dateTimeKey}}"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTimeValue.DateTimeValue, result.DateTimeValue);
    }

    [Fact]
    public void ReadVariableValue_ShouldReturnBoolValue_WhenKeyRefersToBoolean()
    {
        // Arrange
        var boolValue = new DynamicValue(true);
        var variable = new Variable { Key = "boolKey", Value = boolValue };
        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{boolKey}}"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boolValue.BoolValue, result.BoolValue);
    }

    [Fact]
    public void ReadVariableValue_ShouldReturnNumericValue_WhenKeyRefersToNumber()
    {
        // Arrange
        var numericValue = new DynamicValue(42L);
        var variable = new Variable { Key = "numericKey", Value = numericValue };
        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{numericKey}}"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(numericValue.NumericValue, result.NumericValue);
    }

    [Fact]
    public void ReadVariableValue_ShouldResolveNestedObject()
    {
        // Arrange
        var nestedObject = new DynamicValueObject(new Dictionary<string, DynamicValue?>
        {
            { "subProperty", new DynamicValue("NestedValue") }
        });

        var variable = new Variable
        {
            Key = "nestedKey",
            Value = new DynamicValue(nestedObject)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{nestedKey.subProperty}}"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NestedValue", result.TextValue);
    }

    [Fact]
    public void ReadVariableValue_ShouldResolveListElementByKey()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue("Value1") }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

         _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item2].value}}"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Value2", result.TextValue);
    }

    [Fact]
    public void ReadVariableValue_ShouldCombineListsAcrossStorages()
    {
        // Arrange
        var builtInList = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("BuiltInItem") },
                { "value", new DynamicValue("BuiltInValue") }
            }))
        });
    
        _variableStorage.BuiltIn.Returns(new List<Variable>
        {
            new Variable { Key = "listKey", Value = new DynamicValue(builtInList) }
        }.ToImmutableList());
      
        var sessionList = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("SessionItem") },
                { "value", new DynamicValue("SessionValue") }
            }))
        });
    
        _variableStorage.Session.Returns(new List<Variable>
        {
            new Variable { Key = "listKey", Value = new DynamicValue(sessionList) }
        });
    
        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey}}"));
    
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ListValue);
        Assert.Equal(2, result.ListValue!.Count);
        Assert.Contains(result.ListValue, item => item.ObjectValue?["key"].TextValue == "BuiltInItem");
        Assert.Contains(result.ListValue, item => item.ObjectValue?["key"].TextValue == "SessionItem");
    }

    [Fact]
    public void ReadVariableValue_ShouldWarnAndReturnEmptyValue_WhenVariableIsNull()
    {
        // Act
        var result = _reader.ReadVariableValue(null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsNull());
        _outputService.Received().Warning("Variable value is null. Returning an empty DynamicValue.");
    }
    
    [Fact]
    public void ReadVariableValue_ShouldResolveNonExistingElement()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue("Value1") }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item2].value.value2}}"));

        // Assert
        Assert.True(result.IsNull());
    }
    
    [Fact]
    public void ReadVariableValue_ShouldNotResolveFromNumericElement()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue(2) }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item1].value.value2}}"));

        // Assert
        Assert.True(result.IsNull());
    }
    
    [Fact]
    public void ReadVariableValue_ShouldResolveNumericElement()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue(2) }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item1].value}}"));

        // Assert
        Assert.Equal(2, result.NumericValue);
    }
    
    [Fact]
    public void ReadVariableValue_ShouldResolveDateElement()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue(DateTime.Now) }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item1].value}}"));

        // Assert
        Assert.NotNull(result.DateTimeValue);
    }
    
    [Fact]
    public void ReadVariableValue_ShouldResolveBoolElement()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue(true) }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item1].value}}"));

        // Assert
        Assert.True(result.BoolValue);
    }
    
    [Fact]
    public void ReadVariableValue_ShouldResolvePrecisionElement()
    {
        // Arrange
        var listValue = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item1") },
                { "value", new DynamicValue(1.2) }
            })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
            {
                { "key", new DynamicValue("Item2") },
                { "value", new DynamicValue("Value2") }
            }))
        });

        var variable = new Variable
        {
            Key = "listKey",
            Value = new DynamicValue(listValue)
        };

        _variableStorage.Command.Returns(new List<Variable> { variable });

        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{listKey[Item1].value}}"));

        // Assert
        Assert.Equal(1.2, result.PrecisionValue);
    }
    
    [Fact]
    public void ReadVariableValue_ShouldCombineListsAcrossStoragesWhenKeyIsNotPresent()
    {
        // Arrange
        var builtInList = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue("Item1"),
            new DynamicValue("Item2")
        });
    
        _variableStorage.BuiltIn.Returns(new List<Variable>
        {
            new Variable { Key = "list", Value = new DynamicValue(builtInList) }
        }.ToImmutableList());
      
        var sessionList = new DynamicValueList(new List<DynamicValue>
        {
            new DynamicValue("Item3")
        });
    
        _variableStorage.Session.Returns(new List<Variable>
        {
            new Variable { Key = "list", Value = new DynamicValue(sessionList) }
        });
    
        // Act
        var result = _reader.ReadVariableValue(new DynamicValue("{{list}}"));
    
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ListValue);
        Assert.Equal(3, result.ListValue!.Count);
    }

}
