using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace httpclient_refresh
{
    public class MyTokenHandler : DelegatingHandler
    {
        private readonly TokenStore _tokenStore;

        private TaskCompletionSource<object> taskCompletionSource;

        public MyTokenHandler(TokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpResponse = await InternalSendAsync(request, cancellationToken);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            // you can add conditions such as excluding Paths and checking response message to avoid recursion
            // you can also verify token expiary based on time
            {
                // intentionally not passing in the refresh token
                // at this point we know there is an expired token. So, we can update token regardless of the main request being cancelled
                await UpdateTokenAsync();
                httpResponse = await InternalSendAsync(request, cancellationToken);
            }

            return httpResponse;
        }

        private async Task UpdateTokenAsync(CancellationToken cancellationToken = default)
        {
            if (taskCompletionSource is null)
            {
                try
                {
                    var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/token");
                    var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);

                    _tokenStore.Token = "updated token here";
                }
                catch (Exception e)
                {
                    taskCompletionSource.TrySetException(e);
                    taskCompletionSource = null;
                    throw new Exception("Failed fetching token", e);
                }

                taskCompletionSource.TrySetResult(null);
                taskCompletionSource = null;
            }
            else
                await taskCompletionSource.Task;
        }

        private async Task<HttpResponseMessage> InternalSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            return await base.SendAsync(request, cancellationToken);
        }
        

    }
}
