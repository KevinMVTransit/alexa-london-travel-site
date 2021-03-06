// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// A class containing tests for loading resources in the website.
    /// </summary>
    public class ResourceTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        public ResourceTests(HttpServerFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData("/", "text/html")]
        [InlineData("/account/register/", "text/html")]
        [InlineData("/account/sign-in/", "text/html")]
        [InlineData("/api/", "text/html")]
        [InlineData("/apple-app-site-association", "application/json")]
        [InlineData("/assets/css/site.css", "text/css")]
        [InlineData("/assets/css/site.css.map", "text/plain")]
        [InlineData("/assets/css/site.min.css", "text/css")]
        [InlineData("/assets/css/site.min.css.map", "text/plain")]
        [InlineData("/assets/js/site.js", "application/javascript")]
        [InlineData("/assets/js/site.js.map", "text/plain")]
        [InlineData("/assets/js/site.min.js", "application/javascript")]
        [InlineData("/assets/js/site.min.js.map", "text/plain")]
        [InlineData("/assets/js/site.manage.js", "application/javascript")]
        [InlineData("/assets/js/site.manage.js.map", "text/plain")]
        [InlineData("/assets/js/site.manage.min.js", "application/javascript")]
        [InlineData("/assets/js/site.manage.min.js.map", "text/plain")]
        [InlineData("/assets/js/site.preferences.js", "application/javascript")]
        [InlineData("/assets/js/site.preferences.js.map", "text/plain")]
        [InlineData("/assets/js/site.preferences.min.js", "application/javascript")]
        [InlineData("/assets/js/site.preferences.min.js.map", "text/plain")]
        [InlineData("/.well-known/apple-app-site-association", "application/json")]
        [InlineData("/.well-known/assetlinks.json", "application/json")]
        [InlineData("/BingSiteAuth.xml", "text/xml")]
        [InlineData("/browserconfig.xml", "text/xml")]
        [InlineData("/error.html", "text/html")]
        [InlineData("/favicon.ico", "image/x-icon")]
        [InlineData("/googled1107923138d0b79.html", "text/html")]
        [InlineData("/help/", "text/html")]
        [InlineData("/humans.txt", "text/plain")]
        [InlineData("/keybase.txt", "text/plain")]
        [InlineData("/manifest.webmanifest", "application/manifest+json")]
        [InlineData("/privacy-policy/", "text/html")]
        [InlineData("/robots.txt", "text/plain")]
        [InlineData("/service-worker.js", "application/javascript")]
        [InlineData("/sitemap.xml", "text/xml")]
        [InlineData("/swagger/api/swagger.json", "application/json")]
        [InlineData("/technology/", "text/html")]
        [InlineData("/terms-of-service/", "text/html")]
        public async Task Can_Load_Resource(string requestUri, string contentType)
        {
            // Act
            using (var response = await Fixture.Client.GetAsync(requestUri))
            {
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
                Assert.NotNull(response.Content.Headers.ContentLength);
                Assert.NotEqual(0, response.Content.Headers.ContentLength);
            }
        }

        [Theory]
        [InlineData("/register/", "/account/register/")]
        [InlineData("/sign-up/", "/account/register/")]
        [InlineData("/support/", "/help/")]
        public async Task Resource_Is_Redirect(string requestUri, string location)
        {
            // Act
            using (var response = await Fixture.Client.GetAsync(requestUri))
            {
                // Assert
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                Assert.Equal(location, response.Headers.Location?.OriginalString);
            }
        }

        [Theory]
        [InlineData("/alexa/authorize")]
        [InlineData("/api/_count")]
        [InlineData("/manage/")]
        public async Task Cannot_Load_Resource_Unauthenticated(string requestUri)
        {
            // Act
            using (var response = await Fixture.Client.GetAsync(requestUri))
            {
                // Assert
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                Assert.Equal($"/account/sign-in/?ReturnUrl={Uri.EscapeDataString(requestUri)}", response.Headers.Location?.PathAndQuery);
            }
        }

        [Fact]
        public async Task Manifest_Is_Valid_Json()
        {
            // Arrange
            using (var response = await Fixture.Client.GetAsync("/manifest.webmanifest"))
            {
                // Assert
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                JObject manifest = JObject.Parse(json);

                Assert.Equal("London Travel", (string)manifest["name"]);
            }
        }

        [Fact]
        public async Task Response_Headers_Contains_Expected_Headers()
        {
            // Arrange
            string[] expectedHeaders = new[]
            {
                "content-security-policy",
                "content-security-policy-report-only",
                "Referrer-Policy",
                "X-Content-Type-Options",
                "X-CSP-Nonce",
                "X-Datacenter",
                "X-Download-Options",
                "X-Frame-Options",
                "X-Instance",
                "X-Request-Id",
                "X-Revision",
                "X-XSS-Protection",
            };

            // Act
            using (var response = await Fixture.Client.GetAsync("/"))
            {
                // Assert
                foreach (string expected in expectedHeaders)
                {
                    Assert.True(response.Headers.Contains(expected), $"The '{expected}' response header was not found.");
                }
            }
        }

        [Theory]
        [InlineData("/foo/", HttpStatusCode.NotFound)]
        [InlineData("/error/", HttpStatusCode.InternalServerError)]
        [InlineData("/error/?id=200", HttpStatusCode.InternalServerError)]
        [InlineData("/error/?id=400", HttpStatusCode.BadRequest)]
        [InlineData("/error/?id=403", HttpStatusCode.Forbidden)]
        [InlineData("/error/?id=404", HttpStatusCode.NotFound)]
        [InlineData("/error/?id=405", HttpStatusCode.MethodNotAllowed)]
        [InlineData("/error/?id=408", HttpStatusCode.RequestTimeout)]
        [InlineData("/error/?id=500", HttpStatusCode.InternalServerError)]
        [InlineData("/error/?id=599", HttpStatusCode.InternalServerError)]
        [InlineData("/error/?id=600", HttpStatusCode.InternalServerError)]
        public async Task Error_Page_Returns_Correct_Status_Code(string requestUri, HttpStatusCode expected)
        {
            // Act
            using (var response = await Fixture.Client.GetAsync(requestUri))
            {
                // Assert
                response.StatusCode.ShouldBe(expected);
                response.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");
            }
        }

        [Theory]
        [InlineData("/admin.php")]
        [InlineData("/wp-admin/blah")]
        [InlineData("/xmlrpc.php")]
        public async Task Crawler_Spam_Is_Redirected_To_YouTube(string requestUri)
        {
            // Act
            using (var response = await Fixture.Client.GetAsync(requestUri))
            {
                // Assert
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                Assert.Equal("www.youtube.com", response.Headers.Location?.Host);
            }

            // Arrange
            using (var message = new HttpRequestMessage(HttpMethod.Head, requestUri))
            {
                // Act
                using (var response = await Fixture.Client.SendAsync(message))
                {
                    // Assert
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Equal("www.youtube.com", response.Headers.Location?.Host);
                }
            }

            // Arrange
            using (var content = new StringContent(string.Empty))
            {
                // Act
                using (var response = await Fixture.Client.PostAsync(requestUri, content))
                {
                    // Assert
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Equal("www.youtube.com", response.Headers.Location?.Host);
                }
            }
        }
    }
}
