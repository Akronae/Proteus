# Modern cross-platform serializer designed for simplicity and scalability

![Hello World](http://1.bp.blogspot.com/_dg0YrAzykYY/TQxf30endCI/AAAAAAAAAAw/wdk0ql7g1qo/s1600/p.jpeg)

Proteus is a reflection-based serializer that I use for my projects.

# [Download the DLL](https://github.com/Akronae/Proteus/raw/master/Proteus.Core/bin/Debug/Proteus.Core.dll)

# Documentation
## Basic serialization
```cs
// Used for serializing any object, LoadedAssembliesGenericTypesProvider will be explained later.
var serializer = new Serializer(new LoadedAssembliesGenericTypesProvider());

// Let's say we have this class:
class Person
{
    // SerializedMember tells the serializer which member must be serialized and in which order.
    [SerializedMember(0)]
    public int Age;
    [SerializedMember(1)]
    public string Name;
    [SerializedMember(2)]
    public bool Female;
    
    public override string ToString ()
    {
        return $"{nameof(Person)} {Age} {Name} {(Female ? 'F' : 'M')}";
    }
}

var serialized = serializer.Serialize(new Person {Age = 20, Female = false, Name = "Doe"});
Console.WriteLine(BitConverter.ToString(serialized)); // Prints: "01-00-14-01-03-00-00-00-44-6F-65-01-00".

var deserialized = Serializer.Deserialize<Person>(serialized)
Console.WriteLine(deserialized); // Prints: "Person 20 Doe M".
```

## Inheritance
```cs
// Let's say we have the following classes.
class Person
{
    [SerializedMember(0)]
    public int Age;
    [SerializedMember(1)]
    public string Name;
    [SerializedMember(2)]
    public bool Female;

    public override string ToString ()
    {
        return $"{nameof(Person)} {Age} {Name} {(Female ? 'F' : 'M')}";
    }
}

class Employee : Person
{
    // Note that you don't need to be aware of the last serialized member's index of the base class,
    // they are class-specific and their absolute value don't matter, only the relative order of the indexes does.
    [SerializedMember(0)]
    public int Wage;
    [SerializedMember(1)]
    public Vector2 DeskLocation;
}

class Vector2
{
    [SerializedMember(0)]
    public int X;
    [SerializedMember(1)]
    public int Y;

    public Vector2 (int x, int y)
    {
        X = x;
        Y = y;
    }
    
    // When a class has a constructor it must also have a parameterless constructor as serialization
    // uses parameterless constructor to create instances.
    public Vector2 ()
    {
    }

    public override string ToString ()
    {
        return $"({X}, {Y})";
    }
}

var serializer = new Serializer(new LoadedAssembliesGenericTypesProvider());
var person = new Employee {Age = 20, Female = false, Name = "Doe", Wage = 3500, DeskLocation = new Vector2(50,12)};
var serialized = serializer.Serialize(person);
var deserialized = serializer.Deserialize<Employee>(serialized);
Console.WriteLine(deserialized); // Prints "20 Doe M 3500 (50, 12)".
```

## Generic
```cs
// Let's say we have these classes
class Person
{
    [SerializedMember(0)]
    public int Age;
    [SerializedMember(1)]
    public string Name;

    public override string ToString ()
    {
        return $"{Age} {Name}";
    }
}

class Employee : Person
{
    [SerializedMember(0)]
    public int Wage;
    public override string ToString ()
    {
        return base.ToString() + $" {Wage}";
    }
}

// We want to serialize then deserialize a list of Person
var serializer = new Serializer(new LoadedAssembliesGenericTypesProvider());
var persons = new List<Person>() {new Employee {Age = 21, Name = "Doe", Wage = 3400}, new Person{Age = 29, Name = "John"}};
var serialized = serializer.Serialize(persons);
Console.WriteLine(BitConverter.ToString(serialized));
var deserialized = serializer.Deserialize<List<Person>>(serialized);
foreach (var person in deserialized)
{
    Console.WriteLine(person); // Prints: "21 Doe \n 29 John".
}

// Now, if we want to be able to serialize a List of several inherited classes, we could do so:

// We have to give a unique ID to the class Employee (here 10) with the SerializableAsGeneric attribute.
[SerializableAsGeneric(10)]
class Employee : Person
{
    [SerializedMember(0)]
    public int Wage;
    public override string ToString ()
    {
        return base.ToString() + $" {Wage}";
    }
}

// ...
foreach (var person in deserialized)
{
    Console.WriteLine(person); // Prints: "21 Doe 3400 \n 29 John".
}
```

Here `new LoadedAssembliesGenericTypesProvider()` given to the serialize scans all the loaded assembies for `SerializableAsGeneric` and store the types, but a custom generic type provider which inherits [IGenericTypesProvider](https://github.com/Akronae/Proteus/blob/master/Proteus.Core/IGenericTypesProvider.cs) can be used

**_Warning: This only works for List, but will be implemented for every object in future releases._**

## Data architecture
Here are the supported native types:  
* Boolean
* Byte
* Int32 (int)
* Single (float)
* String
* Enum
* Guid
* List<>
* Dictionary<,>

Nested class are serialized as well.  
You don't need to worry about the space that a number take, Proteus will figure out automatically how to encode it.  
For instance:
```cs
serializer.Serialize(1); // 01-00-01
```
The first byte `01` is a flag set on every member, it tells weather or not the value is null.  
The second byte `00` is a flag that tells on which type the number is encoded, here `00 == Byte` (see [NumberType.cs](https://github.com/Akronae/Proteus/blob/master/Proteus.Core/NumberType.cs))  
And the remaining byte is the value, 1.  
```cs
serializer.Serialize(-300); // 01-02-D4-FE
```
The first byte `01` means that the value is not null  
The second byte `02` means that the value has been encoded on `UShort`
And the remaining `D4-FE` = -300

Proteus is designed this way to be reliable, you don't have to worry about the space numbers could take and to save space as most of the time the space allocated to number is way bigger than the number really need, so the byte used to tell the value type is largely compensated.

**_Summup: Always use either int or float as value type for number fields._**
