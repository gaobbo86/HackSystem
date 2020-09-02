﻿using System.Net.Http;
using System.Net.Http.Headers;
using HackSystem.Web.Common;

namespace HackSystem.Web.Extensions
{
    public static class HttpClientExtension
    {
        public static HttpClient AddAuthorizationHeader(this HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(WebCommonSense.AuthenticationScheme, token);
            return httpClient;
        }
    }
}