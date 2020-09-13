using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace httpclient_refresh
{
    public class MyClientService
    {
        private readonly HttpClient _httpClient;

        public MyClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

    }
}
