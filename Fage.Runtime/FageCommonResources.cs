using FontStashSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime
{
	public class FageCommonResources
		:IServiceScopeFactory, IDisposable, IAsyncDisposable
	{
		private readonly ServiceProvider _diServices;
		private bool _disposed;

		public FageCommonResources(SpriteFont defaultFont,
			ServiceProvider diProvider,
			GameServiceContainer gameServices,
			IConfiguration configuration,
			FontSystem genericFontFamily)
		{
			_diServices = diProvider;
			
			GameServices = gameServices;
			Configuration = configuration;

			DebugFont = defaultFont;
			GenericFontFamily = genericFontFamily;
			GenericUIFont = genericFontFamily.GetFont(14f);

			Services = new(diProvider, gameServices, this);
		}

		/// <summary>
		/// 适用于调试文本的字体，包含Basic Latin和Enclosed CJK Letters and Months
		/// </summary>
		/// <remarks>
		/// 不含中文
		/// </remarks>
		public SpriteFont DebugFont { get; }

		public FontSystem GenericFontFamily { get; }

		public SpriteFontBase GenericUIFont { get; }

		public GameServiceContainer GameServices { get; }
		public ServiceProvider DiServices => _diServices;
		public IConfiguration Configuration { get; }

		public FageServiceScope Services { get; }

		IServiceScope IServiceScopeFactory.CreateScope() => CreateScope();

		public FageServiceScope CreateScope()
		{
			return Services.CreateScope();
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Services.Dispose();
					DiServices.Dispose();
				} 
			}

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual async ValueTask DisposeAsyncCore()
		{
			if (_disposed) return;

			_disposed = true;
			await Services.DisposeAsync();
			await DiServices.DisposeAsync();
		}

		public ValueTask DisposeAsync()
		{
			ValueTask disposeCoreResult = DisposeAsyncCore();
			if (disposeCoreResult.IsCompleted)
			{
				Dispose(false);
				GC.SuppressFinalize(this);
				return disposeCoreResult;
			}
			else
			{
				return Await(disposeCoreResult);
			}

			async ValueTask Await(ValueTask task)
			{
				await task.ConfigureAwait(false);
				Dispose(false);
				GC.SuppressFinalize(this);
			}
		}
	}
}