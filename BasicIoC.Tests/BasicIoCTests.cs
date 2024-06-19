namespace BasicIoC
{
    using FluentAssertions;

    public class BasicIoCTests
    {
        [Fact]
        public void Instance_OnEachCall_ReturnsSameContainer()
        {
            // Arrange/Act
            Container container1 = Container.Instance;
            Container container2 = Container.Instance;

            // Assert
            container1
                .Should()
                .BeSameAs(container2);
        }

        [Fact]
        public void Instance_WhenCalled_ReturnsInitializedContainer()
        {
            // Arrange/Act
            Container container = Container.Instance;

            // Assert
            container._container
                .Should()
                .NotBeNull();
            container._container
                .Should()
                .BeEmpty();
            container._resolvingKeys
                .Should()
                .NotBeNull();
            container._resolvingKeys
                .Should()
                .BeEmpty();
            container._resolvingKeyCount
                .Should()
                .Be(0);
        }

        [Fact]
        public void Register_DependencyIsAlreadyRegistered_AdditionalRegistrationsAreIgnored()
        {
            // Arrange
            Container container = Container.TestInstance;
            string key = "Test";
            int keyValue = 1;
            KeyValuePair<string, int> keyValuePair = new(key, keyValue);
            DependencyKey firstDependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            Type firstResolvingType = typeof(SampleClass);
            ResolvingMethod firstResolvingMethod = ResolvingMethod.Prototype;
            DependencyKey secondDependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 1
            };
            Type secondResolvingType = typeof(SampleClass2);
            ResolvingMethod secondResolvingMethod = ResolvingMethod.Singleton;
            container.RegisterPrototype<ISampleClass, SampleClass>();
            container.RegisterSingleton<ISampleClass, SampleClass2>(key);

            // Act
            container.RegisterSingleton<ISampleClass, SampleClass2>();
            container.RegisterPrototype<ISampleClass, SampleClass>(key);

            // Assert
            container._container
                .Should()
                .HaveCount(2);
            container._container
                .Should()
                .ContainKey(firstDependencyKey);
            container._container[firstDependencyKey].ResolvingType
                .Should()
                .Be(firstResolvingType);
            container._container[firstDependencyKey].ResolvingMethod
                .Should()
                .Be(firstResolvingMethod);
            container._container[firstDependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._container
                .Should()
                .ContainKey(secondDependencyKey);
            container._container[secondDependencyKey].ResolvingType
                .Should()
                .Be(secondResolvingType);
            container._container[secondDependencyKey].ResolvingMethod
                .Should()
                .Be(secondResolvingMethod);
            container._container[secondDependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._resolvingKeys
                .Should()
                .ContainSingle();
            container._resolvingKeys
                .Should()
                .HaveElementAt(0, keyValuePair);
            container._resolvingKeyCount
                .Should()
                .Be(keyValue);
        }

        [Fact]
        public void Register_DependencyWithMultipleKeys_RegistersDependencyWithEachKey()
        {
            // Arrange
            Container container = Container.TestInstance;
            string firstKey = "Test1";
            int firstKeyValue = 1;
            KeyValuePair<string, int> firstKeyValuePair = new(firstKey, firstKeyValue);
            DependencyKey firstDependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = firstKeyValue
            };
            Type firstResolvingType = typeof(SampleClass);
            ResolvingMethod firstResolvingMethod = ResolvingMethod.Prototype;
            string secondKey = "Test2";
            int secondKeyValue = 2;
            KeyValuePair<string, int> secondKeyValuePair = new(secondKey, secondKeyValue);
            DependencyKey secondDependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = secondKeyValue
            };
            Type secondResolvingType = typeof(SampleClass2);
            ResolvingMethod secondResolvingMethod = ResolvingMethod.Singleton;

            // Act
            container.RegisterPrototype<ISampleClass, SampleClass>(firstKey);
            container.RegisterSingleton<ISampleClass, SampleClass2>(secondKey);

            // Assert
            container._container
                .Should()
                .HaveCount(2);
            container._container
                .Should()
                .ContainKey(firstDependencyKey);
            container._container[firstDependencyKey].ResolvingType
                .Should()
                .Be(firstResolvingType);
            container._container[firstDependencyKey].ResolvingMethod
                .Should()
                .Be(firstResolvingMethod);
            container._container[firstDependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._container
                .Should()
                .ContainKey(secondDependencyKey);
            container._container[secondDependencyKey].ResolvingType
                .Should()
                .Be(secondResolvingType);
            container._container[secondDependencyKey].ResolvingMethod
                .Should()
                .Be(secondResolvingMethod);
            container._container[secondDependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._resolvingKeys
                .Should()
                .HaveCount(2);
            container._resolvingKeys
                .Should()
                .HaveElementAt(0, firstKeyValuePair);
            container._resolvingKeys
                .Should()
                .HaveElementAt(1, secondKeyValuePair);
            container._resolvingKeyCount
                .Should()
                .Be(secondKeyValue);
        }

        [Fact]
        public void RegisterPrototype_WithoutResolvingKey_RegistersDependency()
        {
            // Arrange
            Container container = Container.TestInstance;
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            Type resolvingType = typeof(SampleClass);
            ResolvingMethod resolvingMethod = ResolvingMethod.Prototype;

            // Act
            container.RegisterPrototype<ISampleClass, SampleClass>();

            // Assert
            container._container
                .Should()
                .ContainSingle();
            container._container
                .Should()
                .ContainKey(dependencyKey);
            container._container[dependencyKey].ResolvingType
                .Should()
                .Be(resolvingType);
            container._container[dependencyKey].ResolvingMethod
                .Should()
                .Be(resolvingMethod);
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._resolvingKeys
                .Should()
                .BeEmpty();
            container._resolvingKeyCount
                .Should()
                .Be(0);
        }

        [Fact]
        public void RegisterPrototype_WithResolvingKey_RegistersDependency()
        {
            // Arrange
            Container container = Container.TestInstance;
            string key = "Test";
            int keyValue = 1;
            KeyValuePair<string, int> keyValuePair = new(key, keyValue);
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = keyValue
            };
            Type resolvingType = typeof(SampleClass);
            ResolvingMethod resolvingMethod = ResolvingMethod.Prototype;

            // Act
            container.RegisterPrototype<ISampleClass, SampleClass>(key);

            // Assert
            container._container
                .Should()
                .ContainSingle();
            container._container
                .Should()
                .ContainKey(dependencyKey);
            container._container[dependencyKey].ResolvingType
                .Should()
                .Be(resolvingType);
            container._container[dependencyKey].ResolvingMethod
                .Should()
                .Be(resolvingMethod);
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._resolvingKeys
                .Should()
                .ContainSingle();
            container._resolvingKeys
                .Should()
                .HaveElementAt(0, keyValuePair);
            container._resolvingKeyCount
                .Should()
                .Be(keyValue);
        }

        [Fact]
        public void RegisterSingleton_WithoutResolvingKey_RegistersDependency()
        {
            // Arrange
            Container container = Container.TestInstance;
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            Type resolvingType = typeof(SampleClass);
            ResolvingMethod resolvingMethod = ResolvingMethod.Singleton;

            // Act
            container.RegisterSingleton<ISampleClass, SampleClass>();

            // Assert
            container._container
                .Should()
                .ContainSingle();
            container._container
                .Should()
                .ContainKey(dependencyKey);
            container._container[dependencyKey].ResolvingType
                .Should()
                .Be(resolvingType);
            container._container[dependencyKey].ResolvingMethod
                .Should()
                .Be(resolvingMethod);
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._resolvingKeys
                .Should()
                .BeEmpty();
            container._resolvingKeyCount
                .Should()
                .Be(0);
        }

        [Fact]
        public void RegisterSingleton_WithResolvingKey_RegistersDependency()
        {
            // Arrange
            Container container = Container.TestInstance;
            string key = "Test";
            int keyValue = 1;
            KeyValuePair<string, int> keyValuePair = new(key, keyValue);
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = keyValue
            };
            Type resolvingType = typeof(SampleClass);
            ResolvingMethod resolvingMethod = ResolvingMethod.Singleton;

            // Act
            container.RegisterSingleton<ISampleClass, SampleClass>(key);

            // Assert
            container._container
                .Should()
                .ContainSingle();
            container._container
                .Should()
                .ContainKey(dependencyKey);
            container._container[dependencyKey].ResolvingType
                .Should()
                .Be(resolvingType);
            container._container[dependencyKey].ResolvingMethod
                .Should()
                .Be(resolvingMethod);
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeNull();
            container._resolvingKeys
                .Should()
                .ContainSingle();
            container._resolvingKeys
                .Should()
                .HaveElementAt(0, keyValuePair);
            container._resolvingKeyCount
                .Should()
                .Be(keyValue);
        }

        [Fact]
        public void ResolveDependency_DependencyIsNotRegistered_ReturnsNull()
        {
            // Arrange
            IContainer container = Container.TestInstance;

            // Act
            ISampleClass? sampleClass = container.ResolveDependency<ISampleClass>();

            // Assert
            sampleClass
                .Should()
                .BeNull();
        }

        [Fact]
        public void ResolveDependency_DependencyRegisteredAsPrototype_ReturnsInstanceOfResolvingType()
        {
            // Arrange
            Container container = Container.TestInstance;
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            container.RegisterPrototype<ISampleClass, SampleClass>();

            // Act
            object? sampleClass = container.ResolveDependency<ISampleClass>();

            // Assert
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeNull();
            sampleClass
                .Should()
                .NotBeNull();
            sampleClass
                .Should()
                .BeOfType<SampleClass>();
        }

        [Fact]
        public void ResolveDependency_DependencyRegisteredAsPrototype_ReturnsNewInstanceEachTime()
        {
            // Arrange
            Container container = Container.TestInstance;
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            container.RegisterPrototype<ISampleClass, SampleClass>();

            // Act
            ISampleClass? sampleClass1 = container.ResolveDependency<ISampleClass>();
            ISampleClass? sampleClass2 = container.ResolveDependency<ISampleClass>();

            // Assert
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeNull();
            sampleClass1
                .Should()
                .NotBeNull();
            sampleClass1
                .Should()
                .BeOfType<SampleClass>();
            sampleClass2
                .Should()
                .NotBeNull();
            sampleClass2
                .Should()
                .BeOfType<SampleClass>();
            sampleClass1
                .Should()
                .NotBeSameAs(sampleClass2);
        }

        [Fact]
        public void ResolveDependency_DependencyRegisteredAsPrototypeWithResolvingKey_ReturnsCorrectInstance()
        {
            // Arrange
            Container container = Container.TestInstance;
            string key1 = "Test1";
            DependencyKey dependencyKey1 = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 1
            };
            container.RegisterPrototype<ISampleClass, SampleClass>(key1);
            string key2 = "Test2";
            DependencyKey dependencyKey2 = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 2
            };
            container.RegisterPrototype<ISampleClass, SampleClass2>(key2);

            // Act
            ISampleClass? sampleClass1 = container.ResolveDependency<ISampleClass>(key1);
            ISampleClass? sampleClass2 = container.ResolveDependency<ISampleClass>(key2);

            // Assert
            container._container[dependencyKey1].SingletonInstance
                .Should()
                .BeNull();
            container._container[dependencyKey2].SingletonInstance
                .Should()
                .BeNull();
            sampleClass1
                .Should()
                .NotBeNull();
            sampleClass1
                .Should()
                .BeOfType<SampleClass>();
            sampleClass2
                .Should()
                .NotBeNull();
            sampleClass2
                .Should()
                .BeOfType<SampleClass2>();
        }

        [Fact]
        public void ResolveDependency_DependencyRegisteredAsSingleton_ReturnsInstanceOfResolvingType()
        {
            // Arrange
            Container container = Container.TestInstance;
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            container.RegisterSingleton<ISampleClass, SampleClass>();

            // Act
            object? sampleClass = container.ResolveDependency<ISampleClass>();

            // Assert
            container._container[dependencyKey].SingletonInstance
                .Should()
                .NotBeNull();
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeOfType<SampleClass>();
            sampleClass
                .Should()
                .NotBeNull();
            sampleClass
                .Should()
                .BeOfType<SampleClass>();
            ((SampleClass?)container._container[dependencyKey].SingletonInstance)
                .Should()
                .BeSameAs((SampleClass?)sampleClass);
        }

        [Fact]
        public void ResolveDependency_DependencyRegisteredAsSingleton_ReturnsSameInstanceEachTime()
        {
            // Arrange
            Container container = Container.TestInstance;
            DependencyKey dependencyKey = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 0
            };
            container.RegisterSingleton<ISampleClass, SampleClass>();

            // Act
            ISampleClass? sampleClass1 = container.ResolveDependency<ISampleClass>();
            ISampleClass? sampleClass2 = container.ResolveDependency<ISampleClass>();

            // Assert
            container._container[dependencyKey].SingletonInstance
                .Should()
                .NotBeNull();
            container._container[dependencyKey].SingletonInstance
                .Should()
                .BeOfType<SampleClass>();
            sampleClass1
                .Should()
                .NotBeNull();
            sampleClass1
                .Should()
                .BeOfType<SampleClass>();
            ((SampleClass?)container._container[dependencyKey].SingletonInstance)
                .Should()
                .BeSameAs((SampleClass?)sampleClass1);
            sampleClass2
                .Should()
                .NotBeNull();
            sampleClass2
                .Should()
                .BeOfType<SampleClass>();
            ((SampleClass?)container._container[dependencyKey].SingletonInstance)
                .Should()
                .BeSameAs((SampleClass?)sampleClass2);
            sampleClass1
                .Should()
                .BeSameAs(sampleClass2);
        }

        [Fact]
        public void ResolveDependency_DependencyRegisteredAsSingletonWithResolvingKey_ReturnsCorrectInstance()
        {
            // Arrange
            Container container = Container.TestInstance;
            string key1 = "Test1";
            DependencyKey dependencyKey1 = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 1
            };
            container.RegisterSingleton<ISampleClass, SampleClass>(key1);
            string key2 = "Test2";
            DependencyKey dependencyKey2 = new()
            {
                DependencyType = typeof(ISampleClass),
                ResolvingKey = 2
            };
            container.RegisterSingleton<ISampleClass, SampleClass2>(key2);

            // Act
            ISampleClass? sampleClass1 = container.ResolveDependency<ISampleClass>(key1);
            ISampleClass? sampleClass2 = container.ResolveDependency<ISampleClass>(key2);

            // Assert
            container._container[dependencyKey1].SingletonInstance
                .Should()
                .NotBeNull();
            container._container[dependencyKey1].SingletonInstance
                .Should()
                .BeOfType<SampleClass>();
            sampleClass1
                .Should()
                .NotBeNull();
            sampleClass1
                .Should()
                .BeOfType<SampleClass>();
            ((SampleClass?)container._container[dependencyKey1].SingletonInstance)
                .Should()
                .BeSameAs((SampleClass?)sampleClass1);
            container._container[dependencyKey2].SingletonInstance
                .Should()
                .NotBeNull();
            container._container[dependencyKey2].SingletonInstance
                .Should()
                .BeOfType<SampleClass2>();
            sampleClass2
                .Should()
                .NotBeNull();
            sampleClass2
                .Should()
                .BeOfType<SampleClass2>();
            ((SampleClass2?)container._container[dependencyKey2].SingletonInstance)
                .Should()
                .BeSameAs((SampleClass2?)sampleClass2);
        }
    }
}