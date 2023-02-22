// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Abstraction
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Delegate used with GetExports{T}.
    /// </summary>
    /// <param name="type">Type to create.</param>
    /// <returns>New instance of type <param>type</param>.</returns>
    public delegate object? CreationDelegateFactory(Type type);

    public interface IApplicationHost
    {
        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <value>The application version.</value>
        Version ApplicationVersion { get; }
        /// <summary>
        /// Gets the exports.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="manageLifetime">If set to <c>true</c> [manage lifetime].</param>
        /// <returns><see cref="IReadOnlyCollection{T}" />.</returns>
        IReadOnlyCollection<T> GetExports<T>(bool manageLifetime = true);

        /// <summary>
        /// Gets the exports.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="defaultFunc">Delegate function that gets called to create the object.</param>
        /// <param name="manageLifetime">If set to <c>true</c> [manage lifetime].</param>
        /// <returns><see cref="IReadOnlyCollection{T}" />.</returns>
        IReadOnlyCollection<T> GetExports<T>(CreationDelegateFactory defaultFunc, bool manageLifetime = true);

        /// <summary>
        /// Gets the export types.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>IEnumerable{Type}.</returns>
        IEnumerable<Type> GetExportTypes<T>();

        /// <summary>
        /// Resolves this instance.
        /// </summary>
        /// <typeparam name="T">The <c>Type</c>.</typeparam>
        /// <returns>``0.</returns>
        T Resolve<T>();

        /// <summary>
        /// Shuts down.
        /// </summary>
        /// <returns>A task.</returns>
        Task Shutdown();

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="serviceCollection">Instance of the <see cref="IServiceCollection"/> interface.</param>
        void Init(IServiceCollection serviceCollection);

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }
    }
}
