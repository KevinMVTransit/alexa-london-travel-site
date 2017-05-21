// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// A class representing a diagnostic listener for HTTP events. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Based on <c>https://github.com/Microsoft/ApplicationInsights-dotnet-server/blob/db53106c5d85546a2508ecb9dcd6bf1106fb29fa/Src/DependencyCollector/Shared/Implementation/HttpCoreDiagnosticSourceListener.cs</c>.
    /// </remarks>
    internal sealed partial class HttpDiagnosticSourceListener : IObserver<KeyValuePair<string, object>>, IDisposable
    {
        /// <summary>
        /// The name of the diagnostic event containing an HTTP response.
        /// </summary>
        internal const string HttpOutStopEventName = "System.Net.Http.Response";

        /// <summary>
        /// The URL filter to use for Application Insights. This field is read-only.
        /// </summary>
        private readonly ApplicationInsightsUrlFilter _filter;

        /// <summary>
        /// The object to use to fetch the response for events. This field is read-only.
        /// </summary>
        private readonly PropertyFetcher _responseFetcher = new PropertyFetcher("Response");

        /// <summary>
        /// The event subscription in use. This field is read-only.
        /// </summary>
        private readonly HttpDiagnosticSourceSubscriber _subscriber;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDiagnosticSourceListener"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="TelemetryConfiguration"/> in use.</param>
        public HttpDiagnosticSourceListener(TelemetryConfiguration configuration)
        {
            _filter = new ApplicationInsightsUrlFilter(configuration);
            _subscriber = new HttpDiagnosticSourceSubscriber(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HttpDiagnosticSourceListener"/> class.
        /// </summary>
        ~HttpDiagnosticSourceListener()
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
        public void OnNext(KeyValuePair<string, object> value)
        {
            if (value.Key == HttpOutStopEventName)
            {
                var response = _responseFetcher.Fetch(value.Value) as HttpResponseMessage;
                OnResponse(response);
            }
        }

        private void OnResponse(HttpResponseMessage response)
        {
            if (response != null && !_filter.IsApplicationInsightsUrl(response.RequestMessage.RequestUri))
            {
                Activity activity = Activity.Current;

                if (activity != null)
                {
                    if (response.Headers.TryGetValues("x-ms-activity-id", out IEnumerable<string> values))
                    {
                        activity.AddTag("Activity Id", string.Join(", ", values));
                    }

                    if (response.Headers.TryGetValues("x-ms-request-charge", out values))
                    {
                        activity.AddTag("Request Charge", string.Join(", ", values));
                    }
                }
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
                    _subscriber?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
