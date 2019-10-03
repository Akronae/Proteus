# Modern cross-platform serializer designed for simplicity and scalability

![Hello World](http://1.bp.blogspot.com/_dg0YrAzykYY/TQxf30endCI/AAAAAAAAAAw/wdk0ql7g1qo/s1600/p.jpeg)

Proteus is a reflection-based serializer that I use for my projects.

# [Download the NuGet package](https://github.com/Akronae/Proteus/tree/master/nuget)

# Documentation
## Basic serialization
```cs
var serializer = new Serializer();

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
    // they are class-specific and their absolute value don't matter,
    // only the relative order of the indexes does.
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

var serializer = new Serializer();
var person = new Employee {Age = 20, Female = false, Name = "Doe", Wage = 3500,
                           DeskLocation = new Vector2(50, 12)};
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
var serializer = new Serializer();
var persons = new List<Person>() {new Employee {Age = 21, Name = "Doe", Wage = 3400},
                                  new Person{Age = 29, Name = "John"}};
var serialized = serializer.Serialize(persons);
var deserialized = serializer.Deserialize<List<Person>>(serialized);

foreach (var person in deserialized)
{
    Console.WriteLine(person); // Prints: "21 Doe \n 29 John".
}

// Now, if we want to be able to serialize a List of several inherited classes, we could do so:

// `LoadedAssembliesGenericTypesProvider` will scans all the assemblies
//  and tell the serializer which each Type generic ID.
var serializer = new Serializer(new LoadedAssembliesGenericTypesProvider());

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

Here `new LoadedAssembliesGenericTypesProvider()` given to the serializer scans all the loaded assemblies for `SerializableAsGeneric` and stores the types, but a custom generic type provider which inherits [IGenericTypesProvider](https://github.com/Akronae/Proteus/blob/master/Proteus.Core/IGenericTypesProvider.cs) can be used instead.

**This works everywhere (not only for `List<>`) as long as the class deserialized has a `SerializableAsGeneric` attribute for 
the serializer to retrieve the class structure during deserialization.**

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

Nested classes are serialized as well.  
You don't need to worry about the space that a number takes, Proteus will figure out automatically how to encode it.  
For instance:
```cs
serializer.Serialize(1); // 01-FF-00-00-01
```
The first byte `01` means that the number following is encoding as `SByte` see [NumberType.cs](https://github.com/Akronae/Proteus/blob/master/Proteus.Core/NumberType.cs)).  
The second byte `FF = -1.` is the generic type ID of the following serialized object, 
here there is none (`-1 = unedefined generic type`).
The third byte `00` is a flag set on every member, it tells weather or not the value is null,
`00 = value is not null`, `01 = value is null`.  
the fourth byte `00` means the the number is encoded as a `Byte`.
The fifth byte `01 = 1.`.

Proteus is designed this way to be reliable, you don't have to worry about the space numbers could take and to save space as most of the time the space allocated to numbers is way bigger than the numbers really need, so the byte used to tell the number encoding is largely compensated.

**_Summup: Always use either int or float as value type for number fields._**
