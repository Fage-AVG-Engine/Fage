using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fage.Runtime.Utility;

internal static class ForceAsync
{
	internal readonly struct YieldToThreadPoolAwaiter : ICriticalNotifyCompletion
	{
		private static readonly Action<object?> s_runActionDelegate = RunAction;

		public bool IsCompleted => false;

		public void OnCompleted(Action continuation)
		{
			UnsafeOnCompleted(continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			ThreadPool.QueueUserWorkItem(s_runActionDelegate, continuation, preferLocal: false);
		}

		public void GetResult() { }

		private static void RunAction(object? state)
		{
			Action? action = state as Action;
			Debug.Assert(action != null);

			action();
		}
	}

	internal readonly struct YieldToThreadPoolAwaitable
	{
		public YieldToThreadPoolAwaiter GetAwaiter() => default;
	}

	internal static YieldToThreadPoolAwaitable YieldToThreadPool() => default;
}
