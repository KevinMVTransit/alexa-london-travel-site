// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A class representing a subscriber for HTTP diagnostics. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Based on <c>https://github.com/Microsoft/ApplicationInsights-dotnet-server/blob/db53106c5d85546a2508ecb9dcd6bf1106fb29fa/Src/DependencyCollector/Shared/Implementation/HttpCoreDiagnosticSourceListener.cs#L386</c>.
    /// </remarks>
    internal sealed class HttpDiagnosticSourceSubscriber : IObserver<DiagnosticListener>, IDisposable
    {
        /// <summary>
        /// The <see cref="HttpDiagnosticSourceListener"/> in use. This field is read-only.
        /// </summary>
        private readonly HttpDiagnosticSourceListener _listener;

        /// <summary>
        /// The subscription in use by the listener. This field is read-only.
        /// </summary>
        private readonly IDisposable _listenerSubscription;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The subscription in use for events, if any.
        /// </summary>
        private IDisposable _eventSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDiagnosticSourceSubscriber"/> class.
        /// </summary>
        /// <param name="listener">The <see cref="HttpDiagnosticSourceListener"/> to subscribe to.</param>
        internal HttpDiagnosticSourceSubscriber(HttpDiagnosticSourceListener listener)
        {
            _listener = listener;
            _listenerSubscription = DiagnosticListener.AllListeners.Subscribe(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HttpDiagnosticSourceSubscriber"/> class.
        /// </summary>
        ~HttpDiagnosticSourceSubscriber()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void OnCompleted()
        {
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc />
        public void OnNext(DiagnosticListener value)
        {
            if (value?.Name == "HttpHandlerDiagnosticListener")
            {
                _eventSubscription = value.Subscribe(
                    _listener,
                    (name, p1, p2) => name == HttpDiagnosticSourceListener.HttpOutStopEventName);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventSubscription?.Dispose();
                    _listenerSubscription?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
