# The ***BasicIoC*** Class Library
## Overview
***BasicIoC*** is a small class library that implements a simple Inversion of Control (IoC) container. Dependencies can be registered with the
container. The container can then be used to resolve dependencies and return an instance of the resolving type. Dependencies can be registered
as "*prototypes*" where each call to resolve the dependency returns a new instance of the resolving type, or as "*singletons*" where each call
to resolve the dependency returns the same instance of the resolving type. An optional resolving key value can be used in situations where more
than one resolving type needs to be registered with the same dependency type.

### Thread Safety
Every effort has been made to make the ***BasicIoC*** class library thread safe. Each section of code in the library that updates or retrieves
internal state is wrapped in a *lock* block to prevent concurrent thread access to the data. The ***Container*** class itself is a singleton
class that implements the ***Lazy\<T>*** class to ensure thread safety.

## The ***IContainer*** Interface
The ***BasicIoC*** class library defines a single interface named ***IContainer***. This interface contains only three methods:
- ***RegisterPrototype\<TDependencyType, TResolvingType>(string? key = null)***
- ***RegisterSingleton\<TDependencyType, TResolvingType>(string? key = null)***
- ***ResolveDependency\<T>(string? key = null)***

These methods will be described in detail in the next section that covers the ***Container*** class.

## The ***Container*** Class
The ***Container*** class is the only public class accessible in the ***BasicIoC*** class library. The ***Container*** class implements the
***IContainer*** interface. The class consists of one static property and three methods which are described below.

### The ***Instance*** Property
The ***Instance*** property is a static property of the ***Container*** class. This property returns an instance of the ***Container*** class.
Since the ***Container*** class implements the *singleton* pattern, every call to the ***Instance*** property returns the same instance of the
***Container*** class. Internally, the ***Container*** class contains a single parameterless constructor that is marked *private* to prevent
the end user from instantiating more than one instance of the ***Container*** class.

The ***Instance*** property is defined as shown below:

```csharp
static Container Instance { get; }
```

Here's an example of calling the ***Instance*** property:

```csharp
IContainer container = Container.Instance;
```

### The ***RegisterPrototype*** Method
The ***RegisterPrototype*** method is used to register a dependency that should return a new instance of the resolving type every time the
dependency is resolved. The method has the following definition:

```csharp
void RegisterPrototype<TDependencyType, TResolvingType>(string? key = null)
    where TDependencyType : class
    where TResolvingType : TDependencyType, new()
```

***TDependencyType*** specifies the *Type* of the dependency. This will typically be an interface name or a base class name. The
***TDependencyType*** cannot be a *struct* or other value type. It must be a *class* type.

***TResolvingType*** specifies the *Type* of object that should be returned when the dependency is resolved. If ***TDependencyType***
specifies an interface name, then ***TResolvingType*** must be a class that implements the interface. If ***TDependencyType*** specifies
a class name, then ***TResolvingType*** must be a class that derives from the specified class. In either case, the class specified by
***TResolvingType*** must have a parameterless constructor.

The ***key*** parameter is used to specify an optional resolving key. This is useful in situations where you may have more than one
resolving type for a given dependency type. For example, say you have a dependency on the ***IBankAccount*** interface. Assume that
both the ***CheckingAccount*** class and ***SavingsAccount*** class implement this interface. You can register both resolving class
types to the dependency by using the ***key*** parameter as follows:

```csharp
string checkingKey = "checking";
string savingsKey = "savings";
IContainer container = Container.Instance;
container.RegisterPrototype<IBankAccount, CheckingAccount>(checkingKey);
container.RegisterPrototype<IBankAccount, SavingsAccount>(savingsKey);
```

Generally you will have only one resolving type registered to a given dependency type. The ***key*** parameter can be omitted in this case
as shown below:

```csharp
IContainer container = Container.Instance;
container.RegisterPrototype<ILogger, Logger>();
```

### The ***RegisterSingleton*** Method
The ***RegisterSingleton*** method is used to register a dependency that should return the same instance of the resolving type every time the
dependency is resolved. The method has the following definition:

```csharp
void RegisterSingleton<TDependencyType, TResolvingType>(string? key = null)
    where TDependencyType : class
    where TResolvingType : TDependencyType, new()
```

The description of the ***RegisterSingleton*** method is nearly identical to the description of the ***RegisterPrototype*** method described
above. The only difference is that the ***RegisterSingleton*** method is used for registering dependencies that should always resolve to a
single instance of the resolving type, whereas the ***RegisterPrototype*** method is used for registering dependencies that should always
resolve to a new instance of the resolving type. Refer to the description of the ***RegisterPrototype*** method above for further details.

> [!NOTE]
> *Every call to **RegisterPrototype** and **RegisterSingleton** must specify a unique combination of **TDependencyType**, **TResolvingType**
> and **key**. Any attempt to register a **TDependencyType**/**TResolvingType**/**key** combination more than once will be ignored.*

> [!NOTE]
> *It's possible (although unlikely) to register the same **TDependencyType**/**TResolvingType** combination as both a prototype and as a
> singleton as long as one of the two also uses a unique resolving **key** value. For example, the following is valid:*

```csharp
string resolvingKey = "singleton";
IContainer container = Container.Instance;
container.RegisterPrototype<ISampleClass, SampleClass>();
container.RegisterSingleton<ISampleClass, SampleClass>(resolvingKey);
```

### The ***ResolveDependency*** Method
The ***ResolveDependency*** method is used to resolve dependencies that have previously been registered by the ***RegisterPrototype***
or ***RegisterSingleton*** methods described above. The method has the following definition:

```csharp
T? ResolveDependency<T>(string? key = null) where T : class
```

The generic type parameter ***T*** must match a dependency type that has been registered using ***RegisterPrototype*** or
***RegisterSingleton***. The generic type parameter ***T*** on the ***ResolveDependency*** method corresponds to the ***TDependencyType***
generic type parameter on the two ***Register...*** methods.

If the dependency type was registered using a resolving key value, then that same value must be passed into the ***key*** parameter of the
***ResolveDependency*** method. The optional ***key*** parameter can be omitted if no resolving key value was used to register the dependency
type.

Assuming the dependency type has been registered, the ***ResolveDependency*** method will return an instance of the corresponding resolving
type. Or, *null* will be returned if the dependency type wasn't registered.

The following example code assumes the ***Sample*** class implements the ***ISample*** interface.

```csharp
IContainer container = Container.Instance;
container.RegisterSingleton<ISample, Sample>();
ISample? sample = container.ResolveDependency<ISample>();
```

If the dependency type was registered using the ***RegisterPrototype*** method, then a new instance of the resolving type will be returned
each time that ***ResolveDependency*** is called for that dependency type.

If the dependency type was registered using the ***RegisterSingleton*** method, the action taken by the ***ResolveDependency*** method will
depend on whether this is the first time the method has been called for that particular dependency type. On the first time,
***ResolveDependency*** will create a new instance of the resolving type and save a reference to that instance. It will then return that
instance to the caller. On the second and subsequent calls ***ResolveDependency*** will return that same instance. In other words, a new
instance of the resolving type is created only on the first call to ***ResolveDependency*** for any given dependency type.

## Using the ***BasicIoC*** Class Library
This section provides an example showing the typical usage of the ***BasicIoC*** class library. The example assumes a project that contains
the following classes:

- ***MyLogger*** - This class implements the ***ILogger*** interface. It has no dependencies on any other class.
- ***MyClass1*** - This class implements the ***IClass1*** interface. It has a dependency on ***ILogger***.
- ***MyClass2*** - This class implements the ***IClass2*** interface. It has dependencies on ***ILogger*** and ***IClass1***.

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
We need a singleton class to encapsulate the ***BasicIoC*** ***Container*** class. This singleton class is where we will register all of our
dependencies. It will also provide a method for resolving the dependencies. This singleton class must be made available to all classes in our
project where we want to be able to resolve dependencies. The following code shows one such implementation of this class.

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

To enforce the singleton pattern, the ***ServiceLocater*** class has a private parameterless default constructor that gets invoked by the
static ***Lazy\<ServiceLocater>*** class instance the first time a call is made to the static ***ServiceLocater.Current*** property. The
static ***ServiceLocater.Current*** property then returns a reference to the ***ServiceLocater*** instance to the caller.

Notice that the private constructor gets an instance of the ***Container*** class and then registers all of the dependencies for the project
in that container. Many classes within a project may use the same instance of the ***ServiceLocater*** class. You may not have any direct
control over the order that the classes get initialized. Because we are using the singleton pattern here we don't really care about the order.
The first class that calls ***ServiceLocater.Current*** will cause the private constructor to be called which in turn will get an instance of
the ***Container*** class from the ***BasicIoC*** class library. The private constructor then registers all of the properties for the project.
Subsequent calls to ***ServiceLocater.Current*** will return the same instance of the ***ServiceLocater*** object with all dependencies already
registered.

Note that the ***Get\<T>()*** method is just a wrapper around the ***ResolveDependency\<T>()*** method of the ***Container*** class.

A class that uses the ***ServiceLocater*** class would do something like this:

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
We now have a reference to the ***ServiceLocater*** class. The next order of business is to resolve the dependencies in each of our classes by
calling the ***Get\<T>*** method. For the ***MyClass1*** class this would look something like this:

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

For the ***MyClass2*** class we would have this:

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

We don't need to do anything special for the ***MyLogger*** class since that class doesn't have any dependencies on any other class.

### Allowing for Unit Testing
Although the current state of this example project will work as expected, we run into some issues if we try to create unit tests for our classes.
As it stands we are unable to substitute mock implementations of any of our dependencies. We only have a parameterless default constructor which
will always fill in the dependencies with actual implementations of their resolving class types.

To get around this issue, any class that has dependencies on other classes needs a second constructor. This second constructor should have a
parameter for each dependency. We don't want to expose this constructor to the outside world, so we should make it internal rather than public.
We can then use the ***InternalsVisibleTo*** attribute to expose this constructor to our unit test project.

Once we create this second constructor we can modify the parameterless default constructor to call the internal constructor and pass in the
resolved dependencies. For the ***MyClass1*** class this would look something like this:

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

Notice that we had to get rid of the ***_serviceLocater*** field since there is no way to reference it in the public constructor initialization.
Now our unit test project can use the internal constructor of ***MyClass1*** to pass in a mock instance of the ***ILogger*** dependency.

Another thing to notice is that we now check for *null* on the argument passed into the internal constructor. The nullable indicator has been
removed from the return type of the ***Logger*** property. If the argument passed into the internal constructor is *null* then likely the
dependency was never registered. In this case we terminate our app with an ***ArguementNullException*** since we wouldn't want the app to continue.

## A Sample Project Using ***BasicIoC***
In this section of the document we'll layout an entire sample project that demonstrates the use of the ***BasicIoC*** class library. Here are
some relevant details of this project:

- The solution name is ***MyClassLibrary***.
- The solution contains two projects:
  - ***MyClassLibrary*** is the main project that contains the actual class library.
  - ***MyClassLibrary.Tests*** is the unit test project for the class library.
- The ***MyClassLibrary*** project contains four classes:
  - The ***MyLogger*** class which implements the ***ILogger*** interface. This class doesn't have any dependencies on any other class.
  - The ***MyClass1*** class which implements the ***IClass1*** interface. This class has a dependency on ***ILogger***.
  - The ***MyClass2*** class which implements the ***IClass2*** interface. This class has dependencies on both ***ILogger*** and ***IClass1***.
  - The ***ServiceLocater*** class which implements the ***IServiceLocater*** interface. This class uses the ***BasicIoC*** class library.
    It registers all dependencies for the ***MyClassLibrary*** project and is also used to resolve dependencies in each of the other classes.
- The ***ILogger***, ***IClass1***, and ***IClass2*** interfaces all define a single method named ***GetTrace()*** which returns a list of
  strings that give trace information, specifically when each class is entered and exited, and the class that called it.
- The solution was implemented in .NET 6 and C# 10 using Visual Studio 2022 Community Edition.

### Making Internals Visible to the Test Project
The following lines must be added to the ***MyClassLibrary.csproj*** file in order to allow the ***MyClassLibrary.Tests*** project to access
the internal constructors of the ***MyClass1*** and ***MyClass2*** classes:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="MyClassLibrary.Tests" />
  </ItemGroup>
```

### The ***MyLogger*** Class
The ***ILogger*** interface and ***MyLogger*** class are implemented as follows:

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

The ***MyLogger*** class doesn't have any dependencies on any other classes and so it only has a parameterless default constructor which is
generated automatically by the compiler. There is no need to explicitly implement this constructor in the ***MyLogger*** class.

### The ***MyClass1*** Class
The ***IClass1*** interface and ***MyClass1*** class are implemented as follows:

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

The ***MyClass1*** class has a dependency on ***ILogger*** and so the class includes a private property named ***Logger*** to hold the
dependency. The class also has an internal constructor that takes an ***ILogger*** object and sets the ***Logger*** property to this value
after ensuring the value isn't *null*. And, finally, the public default constructor calls the internal constructor after retrieving an
instance of the ***ILogger*** resolving type from the ***ServiceLocater*** instance.

> [!NOTE]
> *Notice that nowhere in the **MyClass1** class do we create a new instance of the **ILogger** dependency. This is handled entirely by the
> **ServiceLocater** class. In fact, **MyClass1** has no direct knowledge that a **MyLogger** instance was passed into the **ILogger** dependency.
> Any object that implements the **ILogger** interface could have been passed into the internal constructor. This loose coupling of dependent
> classes is a key feature of **Inversion of Control**. It leads to flexible and less brittle code. For example, you could add a dependency to the
> **MyLogger** class without impacting any other class that is dependent upon **ILogger**. This feature is also what enables you to pass in mock
> instances of dependent objects in your unit tests.*

### The ***MyClass2*** Class
Next, the ***IClass2*** interface and ***MyClass2*** class are implemented as follows:

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

The ***MyClass2*** class has dependencies on ***ILogger*** and ***IClass1***. It has two private properties (***Logger*** and ***Class1***)
for holding those dependencies. The internal constructor also takes two parameters of type ***ILogger*** and ***IClass1***. The corresponding
private properties are set to the parameter values after ensuring that the parameters aren't *null*. As in ***MyClass1***, the public default
constructor of ***MyClass2*** calls the internal constructor, passing in resolved instances of the dependency objects.

### The ***ServiceLocater*** Class
The final pieces to the ***MyClassLibrary*** project are the ***IServiceLocater*** interface and ***ServiceLocater*** class. These are
implemented as follows:

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

The ***ServiceLocater*** class was described in detail earlier in this document, so we don't need to go into it any further here.

### The Unit Test Project
The ***MyClassLibrary.Tests*** project is the unit test project for the ***MyClassLibrary*** project. The following code shows a sample unit
test project using the ***xUnit*** test framework:

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

This test project contains two unit tests named ***Test1*** and ***Test2***. Technically ***Test1*** isn't strictly a unit test since it
calls the public parameterless default constructor of the ***MyClass2*** class which then gets live instances of its two dependency objects
(***MyClass1*** and ***MyLogger***). A better approach is to call the internal constructor of ***MyClass2*** passing in mock versions of
its two dependency objects. This is what was done in the ***Test2*** unit test. However, ***Test1*** is still useful since it demonstrates
the successful workings of the ***BasicIoC*** class library.

Note that in using the internal constructor ***Test2*** bypasses the ***ServiceLocater*** class and therefore also bypasses the
***BasicIoC*** class library.

> *The sample projects described above were actually implemented and tested successfully using **Visual Studio 2022**. If something doesn't
> work it may be due to a difference in the project properties or the IDE or compiler options.*

## End of README file
