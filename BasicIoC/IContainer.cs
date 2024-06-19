namespace BasicIoC
{
    /// <summary>
    /// The <see cref="IContainer" /> interface defines methods for implementing a simple Inversion
    /// of Control mechanism an application.
    /// </summary>
    public interface IContainer
    {
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
        void RegisterPrototype<TDependencyType, TResolvingType>(string? key = null)
            where TDependencyType : class where TResolvingType : TDependencyType, new();

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
        void RegisterSingleton<TDependencyType, TResolvingType>(string? key = null)
            where TDependencyType : class where TResolvingType : TDependencyType, new();

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
        T? ResolveDependency<T>(string? key = null) where T : class;
    }
}