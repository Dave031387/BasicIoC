namespace BasicIoC
{
    /// <summary>
    /// The <see cref="Container" /> class provides a simple mechanism for implementing Inversion of
    /// Control in an application.
    /// </summary>
    public class Container : IContainer
    {
        internal readonly Dictionary<DependencyKey, ResolvingInfo> _container = new();
        internal readonly Dictionary<string, int> _resolvingKeys = new();
        internal int _resolvingKeyCount = 0;
        private static readonly Lazy<Container> _lazy = new(() => new Container());
        private readonly object _lock = new();

        /// <summary>
        /// Private parameterless constructor required for implementing the singleton pattern for
        /// the <see cref="Container" /> class.
        /// </summary>
        private Container()
        {
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="Container" /> class.
        /// </summary>
        public static Container Instance => _lazy.Value;

        /// <summary>
        /// Gets a new instance of the <see cref="Container" /> class.
        /// </summary>
        /// <remarks>
        /// This is an internal static property intended for use in unit testing.
        /// </remarks>
        internal static Container TestInstance => new();

        /// <summary>
        /// Registers the given <typeparamref name="TResolvingType" /> with the given
        /// <typeparamref name="TDependencyType" /> and sets the resolving method to "prototype".
        /// </summary>
        /// <typeparam name="TDependencyType">
        /// Specifies the type of the dependency object. This is usually an interface or base class.
        /// </typeparam>
        /// <typeparam name="TResolvingType">
        /// Specifies the type of the resolving object. This must be a class type that implements
        /// <br /> the dependency type (if <typeparamref name="TDependencyType" /> is an interface)
        /// or derives from the <br /> dependency type (if <typeparamref name="TDependencyType" />
        /// is a base class).
        /// </typeparam>
        /// <param name="key">
        /// An optional key that can be used to allow more than one resolving type <br /> to be
        /// registered to the same dependency type. Each resolving type must <br /> be paired with a
        /// unique key.
        /// </param>
        public void RegisterPrototype<TDependencyType, TResolvingType>(string? key = null)
            where TDependencyType : class
            where TResolvingType : TDependencyType, new()
            => Register<TDependencyType, TResolvingType>(ResolvingMethod.Prototype, key);

        /// <summary>
        /// Registers the given <typeparamref name="TResolvingType" /> with the given
        /// <typeparamref name="TDependencyType" /> and sets the resolving method to "singleton".
        /// </summary>
        /// <typeparam name="TDependencyType">
        /// Specifies the type of the dependency object. This is usually an interface or base class.
        /// </typeparam>
        /// <typeparam name="TResolvingType">
        /// Specifies the type of the resolving object. This must be a class type that implements
        /// <br /> the dependency type (if <typeparamref name="TDependencyType" /> is an interface)
        /// or derives from the <br /> dependency type (if <typeparamref name="TDependencyType" />
        /// is a base class).
        /// </typeparam>
        /// <param name="key">
        /// An optional key that can be used to allow more than one resolving type <br /> to be
        /// registered to the same dependency type. Each resolving type must <br /> be paired with a
        /// unique key.
        /// </param>
        public void RegisterSingleton<TDependencyType, TResolvingType>(string? key = null)
            where TDependencyType : class
            where TResolvingType : TDependencyType, new()
            => Register<TDependencyType, TResolvingType>(ResolvingMethod.Singleton, key);

        /// <summary>
        /// Resolves the dependency type <typeparamref name="T" /> and returns an instance of the
        /// corresponding resolving type that was previously registered to that dependency type.
        /// </summary>
        /// <typeparam name="T">
        /// Specifies the type of the dependency object that is to be resolved.
        /// </typeparam>
        /// <param name="key">
        /// An optional key used for determining the correct resolving type to be returned. <br />
        /// The key must match the value that was used when the resolving type was <br /> registered
        /// to the dependency type.
        /// </param>
        /// <returns>
        /// Either a singleton instance of the resolving type or a new instance of the resolving
        /// type, depending on whether the resolving type was registered as a "singleton" or
        /// "prototype", respectively.
        /// <para>
        /// A <see langword="null" /> instance of the resolving type is returned if no resolving
        /// type was ever registered to the given dependency type, or if an incorrect
        /// <paramref name="key" /> value is specified.
        /// </para>
        /// </returns>
        public T? ResolveDependency<T>(string? key = null) where T : class
        {
            T? result = null;
            Type dependencyType = typeof(T);
            int resolvingKey = GetResolvingKeyNumber(key, false);
            DependencyKey dependencyKey = new()
            {
                DependencyType = dependencyType,
                ResolvingKey = resolvingKey
            };

            lock (_lock)
            {
                if (_container.ContainsKey(dependencyKey))
                {
                    Type resolvingType = _container[dependencyKey].ResolvingType;
                    ResolvingMethod resolvingMethod = _container[dependencyKey].ResolvingMethod;

                    if (resolvingMethod is ResolvingMethod.Singleton)
                    {
                        _container[dependencyKey].SingletonInstance ??= Activator.CreateInstance(resolvingType);
                        result = (T?)_container[dependencyKey].SingletonInstance;
                    }
                    else if (resolvingMethod is ResolvingMethod.Prototype)
                    {
                        result = (T?)Activator.CreateInstance(resolvingType);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an integer value corresponding to the specified <paramref name="key" /> value.
        /// </summary>
        /// <param name="key">
        /// A key value used in registering and resolving dependency types. <br /> A
        /// <see langword="null" /> value passed into this parameter implies that the key value
        /// <br /> isn't used for registering or resolving the dependency type.
        /// </param>
        /// <param name="canRegister">
        /// An optional flag value that indicates whether the <paramref name="key" /> value should (
        /// <see langword="true" />) <br /> or should not ( <see langword="false" />) be assigned a
        /// new integer value if the key value <br /> hasn't previously been passed into this
        /// method. The default is <see langword="true" />.
        /// </param>
        /// <returns>
        /// The integer value corresponding to the specified <paramref name="key" /> value, or 0 if
        /// <paramref name="key" /> is <see langword="null" />.
        /// <para>
        /// This method will return 0 if this is the first time the <paramref name="key" /> value is
        /// passed into this method and <paramref name="canRegister" /> is set to
        /// <see langword="false" />.
        /// </para>
        /// </returns>
        private int GetResolvingKeyNumber(string? key, bool canRegister = true)
        {
            int result = 0;

            if (key is not null)
            {
                lock (_lock)
                {
                    if (canRegister && _resolvingKeys.ContainsKey(key) is false)
                    {
                        _resolvingKeys[key] = ++_resolvingKeyCount;
                    }

                    if (_resolvingKeys.ContainsKey(key))
                    {
                        result = _resolvingKeys[key];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Registers the given <typeparamref name="TResolvingType" /> with the given
        /// <typeparamref name="TDependencyType" /> and sets the resolving method to the value
        /// specified by <paramref name="method" />.
        /// </summary>
        /// <typeparam name="TDependencyType">
        /// Specifies the type of the dependency object. This is usually an interface or base class.
        /// </typeparam>
        /// <typeparam name="TResolvingType">
        /// Specifies the type of the resolving object. This must be a class type that implements
        /// <br /> the dependency type (if <typeparamref name="TDependencyType" /> is an interface)
        /// or derives from the <br /> dependency type (if <typeparamref name="TDependencyType" />
        /// is a base class).
        /// </typeparam>
        /// <param name="method">
        /// Specifies the method used for resolving the dependency. This must be either
        /// <see cref="ResolvingMethod.Singleton" /> or <see cref="ResolvingMethod.Prototype" />.
        /// </param>
        /// <param name="key">
        /// An optional key that can be used to allow more than one resolving type <br /> to be
        /// registered to the same dependency type. Each resolving type must <br /> be paired with a
        /// unique key.
        /// </param>
        private void Register<TDependencyType, TResolvingType>(ResolvingMethod method, string? key)
        {
            Type dependencyType = typeof(TDependencyType);
            Type resolvingType = typeof(TResolvingType);
            int resolvingKey = GetResolvingKeyNumber(key);

            DependencyKey dependencyKey = new()
            {
                DependencyType = dependencyType,
                ResolvingKey = resolvingKey
            };

            ResolvingInfo resolvingInfo = new()
            {
                ResolvingType = resolvingType,
                ResolvingMethod = method,
                SingletonInstance = null
            };

            lock (_lock)
            {
                if (_container.ContainsKey(dependencyKey) is false)
                {
                    _container[dependencyKey] = resolvingInfo;
                }
            }
        }
    }
}