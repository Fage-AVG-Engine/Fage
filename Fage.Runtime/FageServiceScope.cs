using Microsoft.Extensions.DependencyInjection;

namespace Fage.Runtime
{
	public sealed class FageServiceScope : IServiceScope, IServiceProvider, IServiceScopeFactory,
		IAsyncDisposable, IDisposable
	{
		private readonly IServiceScope? _diServiceScope;
		private readonly ServiceProvider? _rootDiServiceProvider;

		public IServiceProvider DiServiceProvider { get; }
		public GameServiceContainer GameServices { get; }

		public FageCommonResources Parent { get; }
		public IServiceProvider ServiceProvider => this;

		internal FageServiceScope(ServiceProvider rootDiProvider, GameServiceContainer gameServices, FageCommonResources parent)
		{
			_rootDiServiceProvider = rootDiProvider;
			GameServices = gameServices;
			Parent = parent;
			DiServiceProvider = rootDiProvider;
		}

		internal FageServiceScope(IServiceScope diServiceScope, GameServiceContainer gameServices, FageCommonResources parent)
		{
			_diServiceScope = diServiceScope;
			GameServices = gameServices;
			Parent = parent;
			DiServiceProvider = diServiceScope.ServiceProvider;
		}

		public FageServiceScope CreateScope()
		{
			if (_rootDiServiceProvider != null)
			{
				return new FageServiceScope(_rootDiServiceProvider.CreateScope(), GameServices, Parent);
			}
			else
			{
				return new FageServiceScope(ServiceProvider.CreateScope(), GameServices, Parent);
			}
		}

		IServiceScope IServiceScopeFactory.CreateScope() => CreateScope();

		public object? GetService(Type serviceType)
		{
			return GetSpecialServices(serviceType) ??
				GameServices.GetService(serviceType) ??
				DiServiceProvider.GetService(serviceType);
		}

		private object? GetSpecialServices(Type serviceType)
		{
			if (serviceType == typeof(IServiceProvider)
				|| serviceType == typeof(IServiceScope)
				|| serviceType == typeof(IServiceScopeFactory))
				return this;

			if (serviceType == typeof(GameServiceContainer))
				return GameServices;

			return null;
		}

		public void Dispose()
		{
			_rootDiServiceProvider?.Dispose();
			_diServiceScope?.Dispose();
		}

		public ValueTask DisposeAsync()
		{
			if (_diServiceScope is IAsyncDisposable scopeAsyncDisposable)
				return scopeAsyncDisposable.DisposeAsync();
			else if (_rootDiServiceProvider is IAsyncDisposable rootAsyncDisposable)
				return rootAsyncDisposable.DisposeAsync();

			Dispose();
			return ValueTask.CompletedTask;
		}
	}
}