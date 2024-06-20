<style>
   h1 { color: forestgreen }
   h2 { color: forestgreen }
   h3 { color: forestgreen }
   h4 { color: forestgreen }
   notes { color: darkcyan }
   keyword { color: dodgerblue }
   name { color: salmon }
</style>

# The ***BasicIoC*** Class Library
<!--TOC-->
  - [Overview](#overview)
    - [Thread Safety](#thread-safety)
  - [The ***IContainer*** Interface](#the-icontainer-interface)
  - [The ***Container*** Class](#the-container-class)
    - [The ***Instance*** Property](#the-instance-property)
    - [The ***RegisterPrototype*** Method](#the-registerprototype-method)
    - [The ***RegisterSingleton*** Method](#the-registersingleton-method)
    - [The ***ResolveDependency*** Method](#the-resolvedependency-method)
  - [Using the ***BasicIoC*** Class](#using-the-basicioc-class)
    - [The ***ServiceLocater*** Class](#the-servicelocater-class)
    - [Resolving Dependencies](#resolving-dependencies)
    - [Allowing for Unit Testing](#allowing-for-unit-testing)
  - [A Sample Project Using ***BasicIoC***](#a-sample-project-using-basicioc)
    - [Making Internals Visible to the Test Project](#making-internals-visible-to-the-test-project)
    - [The ***MyLogger*** Class](#the-mylogger-class)
    - [The ***MyClass1*** Class](#the-myclass1-class)
    - [The ***MyClass2*** Class](#the-myclass2-class)
    - [The ***ServiceLocater*** Class](#the-servicelocater-class)
    - [The Unit Test Project](#the-unit-test-project)
  - [End of README file](#end-of-readme-file)
<!--/TOC-->
## Overview
***BasicIoC*** is a small class library that implements a simple Inversion of Control (IoC) container. Dependencies can be registered with the
container. The container can then be used to resolve dependencies and return an instance of the resolving type. Dependencies can be registered
as "*prototypes*" where each call to resolve the dependency returns a new instance of the resolving type, or as "*singletons*" where each call
to resolve the dependency returns the same instance of the resolving type. An optional resolving key value can be used in situations where more
than one resolving type needs to be registered with the same dependency type.

### Thread Safety
Every effort has been made to make the ***BasicIoC*** class library thread safe. Each section of code in the library that updates or retrieves
internal state is wrapped in a <keyword>*lock*</keyword> block to prevent concurrent thread access to the data. The <name>***Container***</name>
class itself is a singleton class that implements the ***<name>Lazy</name>\<<name>T</name>>*** class to ensure thread safety.

## The ***IContainer*** Interface
The ***BasicIoC*** class library defines a single interface named <name>***IContainer***</name>. This interface contains only three methods:
- ***<name>RegisterPrototype</name>\<<name>TDependencyType</name>, <name>TResolvingType</name>>(<keyword>string</keyword>? <name>key</name> = <keyword>null</keyword>)***
- ***<name>RegisterSingleton</name>\<<name>TDependencyType</name>, <name>TResolvingType</name>>(<keyword>string</keyword>? <name>key</name> = <keyword>null</keyword>)***
- ***<name>ResolveDependency</name>\<<name>T</name>>(<keyword>string</keyword>? <name>key</name> = <keyword>null</keyword>)***

These methods will be described in detail in the next section that covers the <name>***Container***</name> class.

## The ***Container*** Class
The <name>***Container***</name> class is the only public class accessible in the ***BasicIoC*** class library. The <name>***Container***</name>
class implements the <name>***IContainer***</name> interface. The class consists of one static property and three methods which are described below.

### The ***Instance*** Property
The <name>***Instance***</name> property is a static property of the <name>***Container***</name> class. This property returns an instance of the
<name>***Container***</name> class. Since the <name>***Container***</name> class implements the *singleton* pattern, every call to the
<name>***Instance***</name> property returns the same instance of the <name>***Container***</name> class. Internally, the
<name>***Container***</name> class contains a single parameterless constructor that is marked <keyword>*private*</keyword> to prevent the end user
from instantiating more than one instance of the <name>***Container***</name> class.

The <name>***Instance***</name> property is defined as shown below:

```csharp
static Container Instance { get; }
```

Here's an example of calling the <name>***Instance***</name> property:

```csharp
IContainer container = Container.Instance;
```

### The ***RegisterPrototype*** Method
The <name>***RegisterPrototype***</name> method is used to register a dependency that should return a new instance of the resolving type every
time the dependency is resolved. The method has the following definition:

```csharp
void RegisterPrototype<TDependencyType, TResolvingType>(string? key = null)
    where TDependencyType : class
    where TResolvingType : TDependencyType, new()
```

<name>***TDependencyType***</name> specifies the <keyword>*Type*</keyword> of the dependency. This will typically be an interface name or a base
class name. The <name>***TDependencyType***</name> cannot be a <keyword>*struct*</keyword> or other value type. It must be a
<keyword>*class*</keyword> type.

<name>***TResolvingType***</name> specifies the <keyword>*Type*</keyword> of object that should be returned when the dependency is resolved.
If <name>***TDependencyType***</name> specifies an interface name, then <name>***TResolvingType***</name> must be a class that implements the
interface. If <name>***TDependencyType***</name> specifies a class name, then <name>***TResolvingType***</name> must be a class that derives
from the specified class. In either case, the class specified by <name>***TResolvingType***</name> must have a parameterless constructor.

The <name>***key***</name> parameter is used to specify an optional resolving key. This is useful in situations where you may have more than one
resolving type for a given dependency type. For example, say you have a dependency on the <name>***IBankAccount***</name> interface. Assume that
both the <name>***CheckingAccount***</name> class and <name>***SavingsAccount***</name> class implement this interface. You can register both
resolving class types to the dependency by using the <name>***key***</name> parameter as follows:

```csharp
string checkingKey = "checking";
string savingsKey = "savings";
IContainer container = Container.Instance;
container.RegisterPrototype<IBankAccount, CheckingAccount>(checkingKey);
container.RegisterPrototype<IBankAccount, SavingsAccount>(savingsKey);
```

Generally you will have only one resolving type registered to a given dependency type. The <name>***key***</name> parameter can be omitted in
this case as shown below:

```csharp
IContainer container = Container.Instance;
container.RegisterPrototype<ILogger, Logger>();
```

### The ***RegisterSingleton*** Method
The <name>***RegisterSingleton***</name> method is used to register a dependency that should return the same instance of the resolving type every
time the dependency is resolved. The method has the following definition:

```csharp
void RegisterSingleton<TDependencyType, TResolvingType>(string? key = null)
    where TDependencyType : class
    where TResolvingType : TDependencyType, new()
```

The description of the <name>***RegisterSingleton***</name> method is nearly identical to the description of the
<name>***RegisterPrototype***</name> method described above. The only difference is that the <name>***RegisterSingleton***</name> method is used
for registering dependencies that should always resolve to a single instance of the resolving type, whereas the <name>***RegisterPrototype***</name>
method is used for registering dependencies that should always resolve to a new instance of the resolving type. Refer to the description of the
<name>***RegisterPrototype***</name> method above for further details.

> <notes>**Note:** *Every call to **RegisterPrototype** and **RegisterSingleton** must specify a unique combination of **TDependencyType**,
> **TResolvingType** and **key**. Any attempt to register a **TDependencyType**/**TResolvingType**/**key** combination more than once will be
> ignored.*</notes>

> <notes>**Note:** *It's possible (although unlikely) to register the same **TDependencyType**/**TResolvingType** combination as both a prototype
> and as a singleton as long as one of the two also uses a unique resolving **key** value. For example, the following is valid:*</notes>

```csharp
string resolvingKey = "singleton";
IContainer container = Container.Instance;
container.RegisterPrototype<ISampleClass, SampleClass>();
container.RegisterSingleton<ISampleClass, SampleClass>(resolvingKey);
```

### The ***ResolveDependency*** Method
The <name>***ResolveDependency***</name> method is used to resolve dependencies that have previously been registered by the
<name>***RegisterPrototype***</name> or <name>***RegisterSingleton***</name> methods described above. The method has the following definition:

```csharp
T? ResolveDependency<T>(string? key = null) where T : class
```

The generic type parameter <name>***T***</name> must match a dependency type that has been registered using <name>***RegisterPrototype***</name>
or <name>***RegisterSingleton***</name>. The generic type parameter <name>***T***</name> on the <name>***ResolveDependency***</name> method
corresponds to the <name>***TDependencyType***</name> generic type parameter on the two ***<name>Register</name>...*** methods.

If the dependency type was registered using a resolving key value, then that same value must be passed into the <name>***key***</name> parameter
of the <name>***ResolveDependency***</name> method. The optional <name>***key***</name> parameter can be omitted if no resolving key value was
used to register the dependency type.

Assuming the dependency type has been registered, the <name>***ResolveDependency***</name> method will return an instance of the corresponding
resolving type. Or, <keyword>*null*</keyword> will be returned if the dependency type wasn't registered.

The following example code assumes the <name>***Sample***</name> class implements the <name>***ISample***</name> interface.

```csharp
IContainer container = Container.Instance;
container.RegisterSingleton<ISample, Sample>();
ISample? sample = container.ResolveDependency<ISample>();
```

If the dependency type was registered using the <name>***RegisterPrototype***</name> method, then a new instance of the resolving type will be
returned each time that <name>***ResolveDependency***</name> is called for that dependency type.

If the dependency type was registered using the <name>***RegisterSingleton***</name> method, the action taken by the
<name>***ResolveDependency***</name> method will depend on whether this is the first time the method has been called for that particular dependency
type. On the first time, <name>***ResolveDependency***</name> will create a new instance of the resolving type and save a reference to that
instance. It will then return that instance to the caller. On the second and subsequent calls <name>***ResolveDependency***</name> will return
that same instance. In other words, a new instance of the resolving type is created only on the first call to <name>***ResolveDependency***</name>
for any given dependency type.

## Using the ***BasicIoC*** Class
This section provides an example showing the typical usage of the ***BasicIoC*** class. The example assumes a project that contains the following
classes:

- <name>***MyLogger***</name> - This class implements the <name>***ILogger***</name> interface. It has no dependencies on any other class.
- <name>***MyClass1***</name> - This class implements the <name>***IClass1***</name> interface. It has a dependency on <name>***ILogger***</name>.
- <name>***MyClass2***</name> - This class implements the <name>***IClass2***</name> interface. It has dependencies on <name>***ILogger***</name>
  and <name>***IClass1***</name>.

All three of these classes have a parameterless default constructor. So, our project thus far looks something like this:

```csharp
public interface ILogger
{
}

public interface IClass1
{
}

public interface IClass2
{
}

public class MyLogger : ILogger
{
    public MyLogger()
    {
    }
}

public class MyClass1 : IClass1
{
    public MyClass1()
    {
    }

    ILogger? Logger { get; }
}

public class MyClass2 : IClass2
{
    public MyClass2()
    {
    }

    IClass1? Class1 { get; }

    ILogger? Logger { get; }
}
```

Notice in the above example that the dependencies are represented by read-only properties. We want the ***BasicIoC*** container to initialize
these dependency properties with the appropriate resolving class instances. This will be worked out in the following sections of this document.

### The ***ServiceLocater*** Class
We need a singleton class to encapsulate the ***BasicIoC*** <name>***Container***</name> class. This singleton class is where we will register
all of our dependencies. It will also provide a method for resolving the dependencies. This singleton class must be made available to all classes
in our project where we want to be able to resolve dependencies. The following code shows one such implementation of this class.

```csharp
public interface IServiceLocater
{
    T? Get<T>(string? key = null) where T : class;
}

public class ServiceLocater : IServiceLocater
{
    private readonly IContainer _container;

    private static readonly Lazy<ServiceLocater> _lazy
        = new(() => new ServiceLocater());

    private ServiceLocater()
    {
        _container = Container.Instance;
        _container.RegisterSingleton<ILogger, MyLogger>();
        _container.RegisterSingleton<IClass1, MyClass1>();
        _container.RegisterSingleton<IClass2, MyClass2>();
    }

    public static IServiceLocater Current => _lazy.Value;

    public T? Get<T>(string? key = null) where T : class
        => _container.ResolveDependency<T>(key);
}
```

To enforce the singleton pattern, the <name>***ServiceLocater***</name> class has a private parameterless default constructor that gets invoked by
the static ***<name>Lazy</name>\<<name>ServiceLocater</name>>*** class instance the first time a call is made to the static
<name>***ServiceLocater.Current***</name> property. The static <name>***ServiceLocater.Current***</name> property then returns a reference to the
<name>***ServiceLocater***</name> instance to the caller.

Notice that the private constructor gets an instance of the <name>***Container***</name> class and then registers all of the dependencies for the
project in that container. Many classes within a project may use the same instance of the <name>***ServiceLocater***</name> class. You may not have
any direct control over the order that the classes get initialized. Because we are using the singleton pattern here we don't really care about the
order. The first class that calls <name>***ServiceLocater.Current***</name> will cause the private constructor to be called which in turn will get
an instance of the <name>***Container***</name> class from the ***BasicIoC*** class library. The private constructor then registers all of the
properties for the project. Subsequent calls to <name>***ServiceLocater.Current***</name> will return the same instance of the
<name>***ServiceLocater***</name> object with all dependencies already registered.

Note that the ***<name>Get</name>\<<name>T</name>>()*** method is just a wrapper around the ***<name>ResolveDependency</name>\<<name>T</name>>()***
method of the <name>***Container***</name> class.

A class that uses the <name>***ServiceLocater***</name> class would do something like this:

```csharp
public class MyClass1 : IClass1
{
    private IServiceLocater _serviceLocater;

    public MyClass1()
    {
        _serviceLocater = ServiceLocater.Current;
    }
}
```

### Resolving Dependencies
We now have a reference to the <name>***ServiceLocater***</name> class. The next order of business is to resolve the dependencies in each of our
classes by calling the ***<name>Get</name>\<<name>T</name>>*** method. For the <name>***MyClass1***</name> class this would look something like
this:

```csharp
public class MyClass1 : IClass1
{
    private IServiceLocater _serviceLocater;

    public MyClass1()
    {
        _serviceLocater = ServiceLocater.Current;
        Logger = _serviceLocater.Get<ILogger>();
    }

    private ILogger? Logger { get; }
}
```

For the <name>***MyClass2***</name> class we would have this:

```csharp
public class MyClass2 : IClass2
{
    private IServiceLocater _serviceLocater;

    public MyClass2()
    {
        _serviceLocater = ServiceLocater.Current;
        Class1 = _serviceLocater.Get<IClass1>();
        Logger = _serviceLocater.Get<ILogger>();
    }

    IClass1? Class1 { get; }

    ILogger? Logger { get; }
}
```

We don't need to do anything special for the <name>***MyLogger***</name> class since that class doesn't have any dependencies on any other class.

### Allowing for Unit Testing
Although the current state of this example project will work as expected, we run into some issues if we try to create unit tests for our classes.
As it stands we are unable to substitute mock implementations of any of our dependencies. We only have a parameterless default constructor which
will always fill in the dependencies with actual implementations of their resolving class types.

To get around this issue, any class that has dependencies on other classes needs a second constructor. This second constructor should have a
parameter for each dependency. We don't want to expose this constructor to the outside world, so we should make it internal rather than public.
We can then use the <name>***InternalsVisibleTo***</name> attribute to expose this constructor to our unit test project.

Once we create this second constructor we can modify the parameterless default constructor to call the internal constructor and pass in the
resolved dependencies. For the <name>***MyClass1***</name> class this would look something like this:

```csharp
public class MyClass1 : IClass1
{
    public MyClass1()
        : this(ServiceLocater.Current.Get<ILogger>())
    {
    }

    internal MyClass1(ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        Logger = logger;
    }

    private ILogger Logger { get; }
}
```

Notice that we had to get rid of the <name>***_serviceLocater***</name> field since there is no way to reference it in the public constructor
initialization. However, now our unit test project can use the internal constructor of <name>***MyClass1***</name> to pass in a mock instance
of the <name>***ILogger***</name> dependency.

Another thing to notice is that we now check for <keyword>*null*</keyword> on the argument passed into the internal constructor. The nullable
indicator was also removed from the return type of the <name>***Logger***</name> property. If the argument passed into the internal constructor
is <keyword>*null*</keyword> then likely the dependency was never registered. In this case we terminate our app with an
<name>***ArguementNullException***</name> since we wouldn't want the app to continue.

## A Sample Project Using ***BasicIoC***
In this section of the document we'll layout an entire sample project that demonstrates the use of the ***BasicIoC*** class library. Here are
some relevant details of this project:

- The solution name is <name>***MyClassLibrary***</name>.
- The solution contains two projects:
  - <name>***MyClassLibrary***</name> is the main project that contains the actual class library.
  - <name>***MyClassLibrary.Tests***</name> is the unit test project for the class library.
- The <name>***MyClassLibrary***</name> project contains four classes:
  - The <name>***MyLogger***</name> class which implements the <name>***ILogger***</name> interface. This class doesn't have any dependencies on
    any other class.
  - The <name>***MyClass1***</name> class which implements the <name>***IClass1***</name> interface. This class has a dependency on
    <name>***ILogger***</name>.
  - The <name>***MyClass2***</name> class which implements the <name>***IClass2***</name> interface. This class has dependencies on both
    <name>***ILogger***</name> and <name>***IClass1***</name>.
  - The <name>***ServiceLocater***</name> class which implements the <name>***IServiceLocater***</name> interface. This class uses the
    ***BasicIoC*** class library. It registers all dependencies for the <name>***MyClassLibrary***</name> project and is also used to resolve
    dependencies in each of the other classes.
- The <name>***ILogger***</name>, <name>***IClass1***</name>, and <name>***IClass2***</name> interfaces all define a single method named
  ***<name>GetTrace</name>()*** which returns a list of strings that give trace information, specifically when each class is entered and exited,
  and the class that called it.
- The solution was implemented in .NET 6 and C# 10 using Visual Studio 2022 Community Edition.

### Making Internals Visible to the Test Project
The following lines must be added to the <name>***MyClassLibrary.csproj***</name> file in order to allow the <name>***MyClassLibrary.Tests***</name>
project to access the internal constructors of the <name>***MyClass1***</name> and <name>***MyClass2***</name> classes:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="MyClassLibrary.Tests" />
  </ItemGroup>
```

### The ***MyLogger*** Class
The <name>***ILogger***</name> interface and <name>***MyLogger***</name> class are implemented as follows:

```csharp
namespace MyClassLibrary
{
    using System.Collections.Generic;

    public interface ILogger
    {
        List<string> GetTrace(string from = "");
    }

    public class MyLogger : ILogger
    {
        public List<string> GetTrace(string from = "")
        {
            string caller = from == "" ? "" : $" from {from}";

            List<string> trace = new()
            {
                $"Entering MyLogger.GetTrace(){caller}",
                "Exiting MyLogger.GetTrace()"
            };
            return trace;
        }
    }
}
```

The <name>***MyLogger***</name> class doesn't have any dependencies on any other classes and so it only has a parameterless default constructor
which is generated automatically by the compiler. There is no need to explicitly implement this constructor in the <name>***MyLogger***</name>
class.

### The ***MyClass1*** Class
The <name>***IClass1***</name> interface and <name>***MyClass1***</name> class are implemented as follows:

```csharp
namespace MyClassLibrary
{
    using System.Collections.Generic;

    public interface IClass1
    {
        List<string> GetTrace(string from = "");
    }

    public class MyClass1 : IClass1
    {
        public MyClass1() : this(ServiceLocater.Current.Get<ILogger>()) {}

        internal MyClass1(ILogger? logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            Logger = logger;
        }

        private ILogger Logger { get; }

        public List<string> GetTrace(string from = "")
        {
            string caller = from == "" ? "" : $" from {from}";

            List<string> trace = new()
            {
                $"Entering MyClass1.GetTrace(){caller}"
            };
            trace.AddRange(Logger.GetTrace(nameof(MyClass1)));
            trace.Add("Exiting MyClass1.GetTrace()");
            return trace;
        }
    }
}
```

The <name>***MyClass1***</name> class has a dependency on <name>***ILogger***</name> and so the class includes a private property named
<name>***Logger***</name> to hold the dependency. The class also has an internal constructor that takes an <name>***ILogger***</name> object
and sets the <name>***Logger***</name> property to this value after ensuring the value isn't <keyword>*null*</keyword>. And, finally, the public
default constructor calls the internal constructor after retrieving an instance of the <name>***ILogger***</name> resolving type from the
<name>***ServiceLocater***</name> instance.

> <notes>*Notice that nowhere in the **MyClass1** class do we create a new instance of the **ILogger** dependency. This is handled entirely by the
> **ServiceLocater** class. In fact, **MyClass1** has no direct knowledge that a **MyLogger** instance was passed into the **ILogger** dependency.
> Any object that implements the **ILogger** interface could have been passed into the internal constructor. This loose coupling of dependent
> classes is a key feature of **Inversion of Control**. It leads to flexible and less brittle code. For example, you could add a dependency to the
> **MyLogger** class without impacting any other class that is dependent upon **ILogger**. This feature is also what enables you to pass in mock
> instances of dependent objects in your unit tests.*</notes>

### The ***MyClass2*** Class
Next, the <name>***IClass2***</name> interface and <name>***MyClass2***</name> class are implemented as follows:

```csharp
namespace MyClassLibrary
{
    using System.Collections.Generic;

    public interface IClass2
    {
        List<string> GetTrace(string from = "");
    }

    public class MyClass2 : IClass2
    {
        public MyClass2() : this(ServiceLocater.Current.Get<ILogger>(),
                                 ServiceLocater.Current.Get<IClass1>())
        {
        }

        internal MyClass2(ILogger? logger, IClass1? class1)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(class1, nameof(class1));
            Logger = logger;
            Class1 = class1;
        }

        private IClass1 Class1 { get; }

        private ILogger Logger { get; }

        public List<string> GetTrace(string from = "")
        {
            string caller = from == "" ? "" : $" from {from}";

            List<string> trace = new()
            {
                $"Entering MyClass2.GetTrace(){caller}"
            };
            trace.AddRange(Class1.GetTrace(nameof(MyClass2)));
            trace.AddRange(Logger.GetTrace(nameof(MyClass2)));
            trace.Add("Exiting MyClass2.GetTrace()");
            return trace;
        }
    }
}
```

The <name>***MyClass2***</name> class has dependencies on <name>***ILogger***</name> and <name>***IClass1***</name>. It has two private properties
(<name>***Logger***</name> and <name>***Class1***</name>) for holding those dependencies. The internal constructor also takes two parameters of
type <name>***ILogger***</name> and <name>***IClass1***</name>. The corresponding private properties are set to the parameter values after ensuring
that the parameters aren't <keyword>*null*</keyword>. As in <name>***MyClass1***</name>, the public default constructor of
<name>***MyClass2***</name> calls the internal constructor, passing in resolved instances of the dependency objects.

### The ***ServiceLocater*** Class
The final pieces to the <name>***MyClassLibrary***</name> project are the <name>***IServiceLocater***</name> interface and
<name>***ServiceLocater***</name> class. These are implemented as follows:

```csharp
namespace MyClassLibrary
{
    using BasicIoC;

    internal interface IServiceLocater
    {
        T? Get<T>(string? key = null) where T : class;
    }

    internal class ServiceLocater : IServiceLocater
    {
        private readonly IContainer _container;

        private static readonly Lazy<ServiceLocater> _lazy
            = new(() => new ServiceLocater());

        private ServiceLocater()
        {
            _container = Container.Instance;
            _container.RegisterSingleton<ILogger, MyLogger>();
            _container.RegisterSingleton<IClass1, MyClass1>();
            _container.RegisterSingleton<IClass2, MyClass2>();
        }

        public static IServiceLocater Current => _lazy.Value;

        public T? Get<T>(string? key = null) where T : class
            => _container.ResolveDependency<T>(key);
    }
}
```

The <name>***ServiceLocater***</name> class was described in detail earlier in this document, so we don't need to go into it any further here.

### The Unit Test Project
The <name>***MyClassLibrary.Tests***</name> project is the unit test project for the <name>***MyClassLibrary***</name> project. The following
code shows a sample unit test project using the ***xUnit*** test framework:

```csharp
namespace MyClassLibrary
{
    public class MyClass2Tests
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            List<string> expected = new()
            {
                "Entering MyClass2.GetTrace()",
                "Entering MyClass1.GetTrace() from MyClass2",
                "Entering MyLogger.GetTrace() from MyClass1",
                "Exiting MyLogger.GetTrace()",
                "Exiting MyClass1.GetTrace()",
                "Entering MyLogger.GetTrace() from MyClass2",
                "Exiting MyLogger.GetTrace()",
                "Exiting MyClass2.GetTrace()"
            };
            IClass2 class2 = new MyClass2();

            // Act
            List<string> actual = class2.GetTrace();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test2()
        {
            // Arrange
            List<string> expected = new()
            {
                "Entering MyClass2.GetTrace()",
                "MockClass1.GetTrace() was called",
                "MockLogger.GetTrace() was called",
                "Exiting MyClass2.GetTrace()"
            };
            IClass1 mockClass1 = new MockClass1();
            ILogger mockLogger = new MockLogger();
            IClass2 class2 = new MyClass2(mockLogger, mockClass1);

            // Act
            List<string> actual = class2.GetTrace();

            // Assert
            Assert.Equal(expected, actual);
        }

        public class MockClass1 : IClass1
        {
            public List<string> GetTrace(string from = "")
            {
                return new List<string>()
                {
                    "MockClass1.GetTrace() was called"
                };
            }
        }

        public class MockLogger : ILogger
        {
            public List<string> GetTrace(string from = "")
            {
                return new List<string>()
                {
                    "MockLogger.GetTrace() was called"
                };
            }
        }
    }
}
```

This test project contains two unit tests named <name>***Test1***</name> and <name>***Test2***</name>. Technically <name>***Test1***</name>
isn't strictly a unit test since it calls the public parameterless default constructor of the <name>***MyClass2***</name> class which then gets
live instances of its two dependency objects (<name>***MyClass1***</name> and <name>***MyLogger***</name>). A better approach is to call the
internal constructor of <name>***MyClass2***</name> passing in mock versions of its two dependency objects. This is what was done in the
<name>***Test2***</name> unit test. However, <name>***Test1***</name> is still useful since it demonstrates the successful workings of the
***BasicIoC*** class library.

Note that in using the internal constructor <name>***Test2***</name> bypasses the <name>***ServiceLocater***</name> class and therefore also
bypasses the ***BasicIoC*** class library.

> <notes>*The sample projects described above were actually implemented and tested successfully using **Visual Studio 2022**. If something doesn't
> work it may be due to a difference in the project properties or the IDE or compiler options.*</notes>

## End of README file