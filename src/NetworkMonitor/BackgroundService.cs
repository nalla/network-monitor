using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NetworkMonitor
{
	// Copyright (c) .NET Foundation. Licensed under the Apache License, Version 2.0.
	/// <summary>
	/// Base class for implementing a long running <see cref="IHostedService"/>.
	/// </summary>
	public abstract class BackgroundService : IHostedService, IDisposable
	{
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private Task task;

		public virtual void Dispose()
		{
			cancellationTokenSource.Cancel();
		}

		public virtual Task StartAsync( CancellationToken cancellationToken )
		{
			// Store the task we're executing
			task = ExecuteAsync( cancellationTokenSource.Token );

			// If the task is completed then return it,
			// this will bubble cancellation and failure to the caller
			if( task.IsCompleted )
				return task;

			// Otherwise it's running
			return Task.CompletedTask;
		}

		public virtual async Task StopAsync( CancellationToken cancellationToken )
		{
			// Stop called without start
			if( task == null )
				return;

			try
			{
				// Signal cancellation to the executing method
				cancellationTokenSource.Cancel();
			}
			finally
			{
				// Wait until the task completes or the stop token triggers
				await Task.WhenAny( task, Task.Delay( Timeout.Infinite,
					cancellationToken ) );
			}
		}

		protected abstract Task ExecuteAsync( CancellationToken stoppingToken );
	}
}
