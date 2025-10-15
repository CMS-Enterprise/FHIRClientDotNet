using FHIRClientAuthenticationAPI;
using FHIRClientAuthenticationAPI.Models;
using FHIRClientBundleSubmission;
using FHIRClientBundleSubmission.Models;
using FHIRClientDocumentRetrievalAPI;
using FHIRClientDocumentRetrievalAPI.Models;
using FHIRClientNotificationRetrieval;
using FHIRClientNotificationRetrieval.Models;
using FHIRClientPractitionerAPI;
using FHIRClientPractitionerAPI.Models;
using FHIRClientPresignedUrlAPI;
using FHIRClientPresignedUrlAPI.Models;
using FHIRClientUploadClinicalDocumentAPI;
using FHIRClientUploadClinicalDocumentAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FHIRClientUnitTests
{


    public class FHIRClientAPITests
    {
        private readonly AppSettings settings;

        public FHIRClientAPITests()
        {
            Environment.SetEnvironmentVariable("CLIENT_ID", "MyValue");
            Environment.SetEnvironmentVariable("CLIENT_SECRET", "MyValue");
            settings = SettingsUtil.GetSettings();

        }
        private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object? responseBody = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            var message = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseBody != null
            ? new StringContent(JsonSerializer.Serialize(responseBody), Encoding.UTF8, "application/json")
            : new StringContent("", Encoding.UTF8, "application/json")
            };

            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(message);

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task AuthenticationAPIHandler_GetTokenAsync_ReturnsResponse_Success()
        {

            ILogger<AuthenticationAPIHandler> logger = NullLogger<AuthenticationAPIHandler>.Instance;

            var expectedResponse = new Token { Value = "eyJraWQiOiJQbUJ...", ExpiresIn = 1800, Type = "Bearer" };
            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new AuthenticationAPIHandler(client, logger);

            // Act
            var result = await service.GetTokenAsync(clientId: settings.AuthenticationAPI?.ClientId!,
              clientSecret: settings.AuthenticationAPI?.ClientSecret!,
              scope: settings.AuthenticationAPI?.Scope!,
              tokenUrl: settings.AuthenticationAPI?.EndpointURL!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("eyJraWQiOiJQbUJ...", result!.Value);

        }

        [Fact]
        public async Task AuthenticationAPIHandler_GetTokenAsync_ReturnsResponse_Failed_401()
        {

            ILogger<AuthenticationAPIHandler> logger = NullLogger<AuthenticationAPIHandler>.Instance;

            var expectedResponse = new Token { Error = "IDP ERROR: invalid_client", StatusCodeValue = 401 };
            var client = CreateMockHttpClient(HttpStatusCode.Unauthorized, expectedResponse);
            var service = new AuthenticationAPIHandler(client, logger);

            // Act
            var result = await service.GetTokenAsync(clientId: "BAD_CLIENT_ID",
              clientSecret: settings.AuthenticationAPI?.ClientSecret!,
              scope: settings.AuthenticationAPI?.Scope!,
              tokenUrl: settings.AuthenticationAPI?.EndpointURL!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IDP ERROR: invalid_client", result.Error);
            Assert.Equal(401, result.StatusCodeValue);


        }

        [Fact]
        public async Task AuthenticationAPIHandler_GetTokenAsync_ReturnsResponse_Failed_400_Missing_Scope()
        {

            ILogger<AuthenticationAPIHandler> logger = NullLogger<AuthenticationAPIHandler>.Instance;

            var expectedResponse = new Token { Message = "Bad request: Required header 'scope' not specified", StatusCodeValue = 400 };
            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new AuthenticationAPIHandler(client, logger);

            // Act
            var result = await service.GetTokenAsync(clientId: settings.AuthenticationAPI?.ClientId!,
              clientSecret: settings.AuthenticationAPI?.ClientSecret!,
              scope: "",
              tokenUrl: settings.AuthenticationAPI?.EndpointURL!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Bad request: Required header 'scope' not specified", result.Message);
            Assert.Equal(400, result.StatusCodeValue);


        }

        [Fact]
        public async Task AuthenticationAPIHandler_GetTokenAsync_ReturnsResponse_Failed_400_Invalid_Scope()
        {

            ILogger<AuthenticationAPIHandler> logger = NullLogger<AuthenticationAPIHandler>.Instance;

            var expectedResponse = new Token { Error = "IDP ERROR: invalid_scope", StatusCodeValue = 400 };
            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new AuthenticationAPIHandler(client, logger);

            // Act
            var result = await service.GetTokenAsync(clientId: settings.AuthenticationAPI?.ClientId!,
              clientSecret: settings.AuthenticationAPI?.ClientSecret!,
              scope: settings.AuthenticationAPI?.Scope!,
              tokenUrl: settings.AuthenticationAPI?.EndpointURL!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IDP ERROR: invalid_scope", result.Error);
            Assert.Equal(400, result.StatusCodeValue);


        }

        [Fact]
        public async Task AuthenticationAPIHandler_GetTokenAsync_ReturnsResponse_Failed_400_Missing_CLientSecret()
        {

            ILogger<AuthenticationAPIHandler> logger = NullLogger<AuthenticationAPIHandler>.Instance;

            var msg = "Bad request: Required header 'clientsecret' not specified";

            var expectedResponse = new Token { Message = msg, StatusCodeValue = 400 };
            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new AuthenticationAPIHandler(client, logger);

            // Act
            var result = await service.GetTokenAsync(clientId: settings.AuthenticationAPI?.ClientId!,
              clientSecret: null,
             scope: settings.AuthenticationAPI?.Scope!,
              tokenUrl: settings.AuthenticationAPI?.EndpointURL!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(msg, result.Message);
            Assert.Equal(400, result.StatusCodeValue);


        }

        [Fact]
        public async Task AuthenticationAPIHandler_GetTokenAsync_ReturnsResponse_Failed_400_Missing_CLientId()
        {

            ILogger<AuthenticationAPIHandler> logger = NullLogger<AuthenticationAPIHandler>.Instance;

            var msg = "Bad request: Required header 'clientid' not specified";

            var expectedResponse = new Token { Message = msg, StatusCodeValue = 400 };
            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new AuthenticationAPIHandler(client, logger);

            // Act
            string? clientId = null;
            var result = await service.GetTokenAsync(clientId: clientId,
               clientSecret: settings.AuthenticationAPI?.ClientSecret!,
              scope: settings.AuthenticationAPI?.Scope!,
              tokenUrl: settings.AuthenticationAPI?.EndpointURL!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(msg, result.Message);
            Assert.Equal(400, result.StatusCodeValue);


        }


        [Fact]
        public async Task PresignedUrlAPIHandler_GetPresignedURLInfoListAsync_ReturnsResponse_Success_200()
        {

            ILogger<PresignedUrlAPIHandler> logger = NullLogger<PresignedUrlAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"Parameters\",\r\n    \"id\": \"6ce2bb15-cc4e-4d2d-979c-fd4df85d26f9\",\r\n    \"parameter\": [\r\n        {\r\n            \"name\": \"presignedUrls\",\r\n            \"part\": [\r\n                {\r\n                    \"name\": \"file\",\r\n                    \"part\": [\r\n                        {\r\n                            \"name\": \"filename\",\r\n                            \"valueString\": \"6ce2bb15-cc4e-4d2d-979c-fd4df85d26f9_1.xml\"\r\n                        },\r\n                        {\r\n                            \"name\": \"uploadUrl\",\r\n                            \"valueUrl\": \"https://esmdcloud2-uat.cms.hhs.gov:9013/6ce2bb15-cc4e-4d2d-979c-fd4df85d26f9/6ce2bb15-cc4e-4d2d-979c-fd4df85d26f9_1.xml?X-Amz-Security-Token=IQoJb3JpZ2luX2VjEOj%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FwEaCXVzLWVhc3QtMSJHMEUCIB1D1B7I9jL6A4mPrINgSqKgzeUPwS0DLxUFwkco88h0AiEAkQzVIeA%2BJHlxxmfGgm7rVgAZPSfFaN4r5Ffy6dpf0GgqxgUIwf%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FARABGgw4MTgzNDU1MDMwMjkiDOOS1ChZE6ZaMRd4gSqaBb4gUJCa5UXxpXxZnKR%2BxL1hZhyfmAsOfMmk6XMXdaIq825WGSe4nH5GcO3YNZ1AeIIXW%2F7Dq%2BZWqjLTmFdeKTv3EBmMRfxA8r3BKE7DIQ9I4JVZJ9sgpGmiU%2BQymF2z%2FQ9NaMzzGe1uEKPM8nb7CJJ0T5GLPpDOE7dBTNqfTSnn31LIBTrTVzlRJ7z5%2Bg5ivqzQjkMIeNuxl%2F4GlSYieXkYpV%2BTTa1nVL1GFfALKPItlI10JkdBTkMzF%2Bp3w6In9JXzA7%2BtGgEEmxdugEfinAoyfPudDYR7yNIzAMqx9YHQsDBYZM%2BmANxavunkP9huEh5azXVJv7hGPQ5imK%2FavVvkfDII0XOZJQubWMcdiv9oYTvWe1AofItGuOYbXOVEjs1SP6EeWHAMoOMkkYseD0VAql2%2BEFgK%2FqkMNytCvNzCIAOmxmAwqYFLBg0DpqCLM8%2BuRsIjOI%2FHnWNnCi9uI%2FBWsn8390%2FY7zUGuIg2GreTUCm9HtmB3vg04S1AovkZHkyOk9Pwr1fEXoOSRqn2HoYQ18WdafXO1RDMcCiXiaaopRRVt8XA4YmuKQipHpe%2BFoDjrd1BvvBeVcIfoUl%2BbIXFL0Gt4Xn7WDoaiLdCU5ezMvhlYmF%2FXv0D2lctyZwWBmYozR59cvEEXmgwisGb96Vd24Ce6xhgYUoIiQJVNEC1OiZHWTUepHeAodM7z5CImJm24x2x55nU7Tmm8J8Zc%2FIVL10N%2FBYVL7kewBwGEqTeSy3D4VUc93SZ%2Fon7zCnm9e%2BZbhIq%2FUesFpOz%2BkkmMV8308r%2FBBdKQB%2BejO9ZSzdn5U76CWvJOLf8tUSiPkWLnPbvxXH3FxumTzewb2DDMAQMWegNvE2yrzzGlFQ2m%2FzF2fSrEwlIhm4pHDCDo6HCBjqxAbZnmyQ2Xa2iqLkCLC8V6tSB%2B449A5OuXMVllyN%2B5IJWKx0LnKGAwfaQXwX2e3vfyaa71aZMQT%2BsBJFSOf3db%2FResNtMNQBiPv3T%2BlhB4NHUw6QvDOGaDPxQJjy8uIuko6bNjwtV3bdv1u%2BzSc1HClJfwJECm9zoz2NUaliopKvC5rsTPZgPkMsrnlm5X9cbcteOw2pezw6kNN9iW84WHD1s%2BHV4qLFKNMQ8Px6MXo9F%2FA%3D%3D&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Date=20250610T155129Z&X-Amz-SignedHeaders=content-md5%3Bhost&X-Amz-Expires=899&X-Amz-Credential=ASIA35CJRNE2SIENK3S3%2F20250610%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Signature=cf5e5d1d5cc1ba87b442bb41a9e715c626716922b0e0573826c74df29410726c\"\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"expiresIn\",\r\n            \"valueDuration\": {\r\n                \"value\": 900,\r\n                \"unit\": \"seconds\",\r\n                \"system\": \"http://unitsofmeasure.org\",\r\n                \"code\": \"s\"\r\n            }\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<PresignedURLAPIResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new PresignedUrlAPIHandler(client, logger);

            // Act

            var result = await service.GetPresignedURLResponseAsync(url: settings.PresignedURLAPI?.EndpointURL!,
                            token: "eyJraWQiOiJQbUJ...",
                            request: settings.PresignedURLAPI?.Request!,
                            baseFileLocationFolder: settings.BaseFileLocationFolder!,
                            headerAccept: settings.PresignedURLAPI?.Accept!,
                            contentType: settings.PresignedURLAPI?.ContentType!
                           );

            var res = (PresignedURLAPIResponse)result.Value;

            // Assert
            Assert.NotNull(res);

            string? uploadUrl = res.Parameter?
            .ToArray()
            .First(p => p.Name!.ToString() == "presignedUrls").Part?
            .ToArray()
            .First(p => p.Name!.ToString() == "file").PartItem?
            .ToArray()
            .First(p => p.Name!.ToString() == "uploadUrl").ValueUrl?
            .ToString();

            Assert.True(res.Parameter?.Count > 0);
            Assert.Equal("https://esmdcloud2-uat.cms.hhs.gov:9013/6ce2bb15-cc4e-4d2d-979c-fd4df85d26f9/6ce2bb15-cc4e-4d2d-979c-fd4df85d26f9_1.xml?X-Amz-Security-Token=IQoJb3JpZ2luX2VjEOj%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FwEaCXVzLWVhc3QtMSJHMEUCIB1D1B7I9jL6A4mPrINgSqKgzeUPwS0DLxUFwkco88h0AiEAkQzVIeA%2BJHlxxmfGgm7rVgAZPSfFaN4r5Ffy6dpf0GgqxgUIwf%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FARABGgw4MTgzNDU1MDMwMjkiDOOS1ChZE6ZaMRd4gSqaBb4gUJCa5UXxpXxZnKR%2BxL1hZhyfmAsOfMmk6XMXdaIq825WGSe4nH5GcO3YNZ1AeIIXW%2F7Dq%2BZWqjLTmFdeKTv3EBmMRfxA8r3BKE7DIQ9I4JVZJ9sgpGmiU%2BQymF2z%2FQ9NaMzzGe1uEKPM8nb7CJJ0T5GLPpDOE7dBTNqfTSnn31LIBTrTVzlRJ7z5%2Bg5ivqzQjkMIeNuxl%2F4GlSYieXkYpV%2BTTa1nVL1GFfALKPItlI10JkdBTkMzF%2Bp3w6In9JXzA7%2BtGgEEmxdugEfinAoyfPudDYR7yNIzAMqx9YHQsDBYZM%2BmANxavunkP9huEh5azXVJv7hGPQ5imK%2FavVvkfDII0XOZJQubWMcdiv9oYTvWe1AofItGuOYbXOVEjs1SP6EeWHAMoOMkkYseD0VAql2%2BEFgK%2FqkMNytCvNzCIAOmxmAwqYFLBg0DpqCLM8%2BuRsIjOI%2FHnWNnCi9uI%2FBWsn8390%2FY7zUGuIg2GreTUCm9HtmB3vg04S1AovkZHkyOk9Pwr1fEXoOSRqn2HoYQ18WdafXO1RDMcCiXiaaopRRVt8XA4YmuKQipHpe%2BFoDjrd1BvvBeVcIfoUl%2BbIXFL0Gt4Xn7WDoaiLdCU5ezMvhlYmF%2FXv0D2lctyZwWBmYozR59cvEEXmgwisGb96Vd24Ce6xhgYUoIiQJVNEC1OiZHWTUepHeAodM7z5CImJm24x2x55nU7Tmm8J8Zc%2FIVL10N%2FBYVL7kewBwGEqTeSy3D4VUc93SZ%2Fon7zCnm9e%2BZbhIq%2FUesFpOz%2BkkmMV8308r%2FBBdKQB%2BejO9ZSzdn5U76CWvJOLf8tUSiPkWLnPbvxXH3FxumTzewb2DDMAQMWegNvE2yrzzGlFQ2m%2FzF2fSrEwlIhm4pHDCDo6HCBjqxAbZnmyQ2Xa2iqLkCLC8V6tSB%2B449A5OuXMVllyN%2B5IJWKx0LnKGAwfaQXwX2e3vfyaa71aZMQT%2BsBJFSOf3db%2FResNtMNQBiPv3T%2BlhB4NHUw6QvDOGaDPxQJjy8uIuko6bNjwtV3bdv1u%2BzSc1HClJfwJECm9zoz2NUaliopKvC5rsTPZgPkMsrnlm5X9cbcteOw2pezw6kNN9iW84WHD1s%2BHV4qLFKNMQ8Px6MXo9F%2FA%3D%3D&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Date=20250610T155129Z&X-Amz-SignedHeaders=content-md5%3Bhost&X-Amz-Expires=899&X-Amz-Credential=ASIA35CJRNE2SIENK3S3%2F20250610%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Signature=cf5e5d1d5cc1ba87b442bb41a9e715c626716922b0e0573826c74df29410726c"
                , uploadUrl);

        }

        [Fact]
        public async Task PresignedUrlAPIHandler_GetPresignedURLInfoListAsync_ReturnsResponse_Failed_400()
        {

            ILogger<PresignedUrlAPIHandler> logger = NullLogger<PresignedUrlAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"OperationOutcome\",\r\n\t\"meta\": {\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-BundleSubmissionResponse\"\r\n        ]\r\n    },\r\n    \"issue\": [\r\n        {\r\n            \"severity\": \"error\",\r\n            \"code\": \"processing\",\r\n            \"details\": {\r\n                \"coding\": [\r\n                    {\r\n                        \"system\": \"https://terminology.esmduat.cms.gov:8099/fhir/CodeSystem/Esmd-CS-ErrorOrWarningCodes\",\r\n                        \"code\": \"DUPLICATE_UNIQUE_ID\",\r\n                        \"display\": \"Duplicate Unique ID submitted in Parameters resource testpresigneduniqueid-test1; the submission is rejected.\"\r\n                    }\r\n                ]\r\n            }\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<PresignedURLAPIFailureResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new PresignedUrlAPIHandler(client, logger);

            // Act

            var result = await service.GetPresignedURLResponseAsync(url: settings.PresignedURLAPI?.EndpointURL!,
                            token: "eyJraWQiOiJQbUJ...",
                            request: settings.PresignedURLAPI?.Request!,
                            baseFileLocationFolder: settings.BaseFileLocationFolder!,
                            headerAccept: settings.PresignedURLAPI?.Accept!,
                            contentType: settings.PresignedURLAPI?.ContentType!
                           );

            var res = (PresignedURLAPIFailureResponse)result.Value;

            // Assert
            Assert.NotNull(res);

            var errorMsg = res.Issue?
            .ToArray()
            .First(p => p.Severity!.ToString() == "error").Details?.Coding?
            .ToArray();

            // Assert
            Assert.NotNull(errorMsg);

            var errorCode = errorMsg?.FirstOrDefault()?.Code;

            Assert.Equal("DUPLICATE_UNIQUE_ID", errorCode);


        }

        [Fact]
        public async Task UploadClinicalDocumentAPIHandler_UploadXMLAsync_ReturnsResponse_Success_200()
        {

            ILogger<UploadClinicalDocumentAPIHandler> logger = NullLogger<UploadClinicalDocumentAPIHandler>.Instance;

            var responseStr = "{\r\n  \"status\": \"SUCCESS\",\r\n  \"message\": \"FILE UPLOADED SUCCESSFULLY\",\r\n  \"filename\": \"6ce2bb15-cc4e-4d2d-979c-fd4df85d26f8_1.xml\",\r\n  \"s3uri\": \"s3://uat-esmd-qurantine/6ce2bb15-cc4e-4d2d-979c-fd4df85d26f8/6ce2bb15-cc4e-4d2d-979c-fd4df85d26f8_1.xml\"\r\n}";

            var expectedResponse = JsonUtil.FromJson<UploadClinicalDocumentSuccessResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new UploadClinicalDocumentAPIHandler(client, logger);

            // Act

            var result = await service.UploadClinicalDocumentAsync(presignedUrl: "https://presignedURL.value",
                            token: "eyJraWQiOiJQbUJ...",
                            fileName: settings.UploadClinicalDocumentAPI?.FileName!,
                            baseFileLocationFolder: settings.BaseFileLocationFolder!,
                            contentMD5: settings.UploadClinicalDocumentAPI?.ContentMD5!);

            var res = (UploadClinicalDocumentSuccessResponse)result.Value;

            // Assert
            Assert.NotNull(res);



            Assert.True(res.Status == "SUCCESS");


        }

        [Fact]
        public async Task UploadClinicalDocumentAPIHandler_UploadXMLAsync_ReturnsResponse_Failed_400()
        {

            ILogger<UploadClinicalDocumentAPIHandler> logger = NullLogger<UploadClinicalDocumentAPIHandler>.Instance;

            var responseStr = "{\r\n    \"Error\": {\r\n        \"Code\": \"AccessDenied\",\r\n        \"Message\": \"Request has expired\",\r\n        \"X-Amz-Expires\": \"900\",\r\n        \"Expires\": \"2025-02-10T22:14:44Z\",\r\n        \"ServerTime\": \"2025-02-11T20:21:01Z\",\r\n        \"RequestId\": \"FBAZC9P4KJ9PHMSF\",\r\n        \"HostId\": \"m6A6+q0AWptHSdS/qNqHIev1SfTUUML9E59A8b3W6cRUIcQM6nXXqLB+gTApG7M35AnAKwDZt/dHwJL1VsAMwQ==\"\r\n    }\r\n}";

            var expectedResponse = JsonUtil.FromJson<UploadClinicalDocumentFailedResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new UploadClinicalDocumentAPIHandler(client, logger);

            // Act

            var result = await service.UploadClinicalDocumentAsync(presignedUrl: "https://presignedURL.value",
                          token: "eyJraWQiOiJQbUJ...",
                          fileName: settings.UploadClinicalDocumentAPI?.FileName!,
                          baseFileLocationFolder: settings.BaseFileLocationFolder!,
                          contentMD5: settings.UploadClinicalDocumentAPI?.ContentMD5!);

            var res = (UploadClinicalDocumentFailedResponse)result.Value;

            // Assert
            Assert.NotNull(res);



            Assert.True(res.Error.Message == "Request has expired");


        }

        [Fact]
        public async Task BundleSubmissionAPIHandler_SubmitBundleAsync_ReturnsResponse_Success_200()
        {

            ILogger<BundleSubmissionAPIHandler> logger = NullLogger<BundleSubmissionAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"Bundle\",\r\n    \"id\": \"d6e86a93-aa30-4e62-a7bb-a07af8a9e2e1\",\r\n    \"meta\": {\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-BundleSubmissionResponse\"\r\n        ],\r\n        \"security\": [\r\n            {\r\n                \"system\": \"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                \"code\": \"V\",\r\n                \"display\": \"very restricted\"\r\n            }\r\n        ]\r\n    },\r\n    \"identifier\": {\r\n        \"system\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n        \"value\": \"IMQ0001235989EC\"\r\n    },\r\n    \"type\": \"transaction-response\",\r\n    \"link\": [\r\n        {\r\n            \"relation\": \"self\",\r\n            \"url\": \"http://terminology.esmduat.cms.gov:8099/fhir\"\r\n        }\r\n    ],\r\n    \"entry\": [\r\n        {\r\n            \"response\": {\r\n                \"status\": \"201 Created\",\r\n                \"location\": \"List/192428/_history/1\",\r\n                \"etag\": \"1\",\r\n                \"lastModified\": \"2025-05-01T14:35:29.960-04:00\",\r\n                \"outcome\": {\r\n                    \"resourceType\": \"OperationOutcome\",\r\n                    \"issue\": [\r\n                        {\r\n                            \"severity\": \"information\",\r\n                            \"code\": \"informational\",\r\n                            \"details\": {\r\n                                \"coding\": [\r\n                                    {\r\n                                        \"system\": \"https://hapifhir.io/fhir/CodeSystem/hapi-fhir-storage-response-code\",\r\n                                        \"code\": \"SUCCESSFUL_CREATE\",\r\n                                        \"display\": \"Create succeeded.\"\r\n                                    }\r\n                                ]\r\n                            },\r\n                            \"diagnostics\": \"Successfully created resource \\\"List/192428/_history/1\\\". Took 74ms.\"\r\n                        }\r\n                    ]\r\n                }\r\n            }\r\n        },\r\n        {\r\n            \"response\": {\r\n                \"status\": \"201 Created\",\r\n                \"location\": \"DocumentReference/192429/_history/1\",\r\n                \"etag\": \"1\",\r\n                \"lastModified\": \"2025-05-01T14:35:29.960-04:00\",\r\n                \"outcome\": {\r\n                    \"resourceType\": \"OperationOutcome\",\r\n                    \"issue\": [\r\n                        {\r\n                            \"severity\": \"information\",\r\n                            \"code\": \"informational\",\r\n                            \"details\": {\r\n                                \"coding\": [\r\n                                    {\r\n                                        \"system\": \"https://hapifhir.io/fhir/CodeSystem/hapi-fhir-storage-response-code\",\r\n                                        \"code\": \"SUCCESSFUL_CREATE\",\r\n                                        \"display\": \"Create succeeded.\"\r\n                                    }\r\n                                ]\r\n                            },\r\n                            \"diagnostics\": \"Successfully created resource \\\"DocumentReference/192429/_history/1\\\". Took 44ms.\"\r\n                        }\r\n                    ]\r\n                }\r\n            }\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<BundleSubmissionAPISuccessResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new BundleSubmissionAPIHandler(client, logger);

            // Act

            var result = await service.SubmitBundleAsync(url: settings.BundleSubmissionAPI?.EndpointURL!,
                      token: "eyJraWQiOiJQbUJ...",
                      request: settings.BundleSubmissionAPI?.Request!,
                      headerAccept: settings.BundleSubmissionAPI?.Accept!, contentType: settings.BundleSubmissionAPI?.ContentType!,
                      httpClientRequestTimeOutSeconds: settings.BundleSubmissionAPI!.HttpClientRequestTimeOutSeconds!);

            var res = (BundleSubmissionAPISuccessResponse)result.Value;

            // Assert
            Assert.NotNull(res);



            Assert.True(res.Entry![0].Response!.Status == "201 Created");


        }

        [Fact]
        public async Task BundleSubmissionAPIHandler_SubmitBundleAsync_ReturnsResponse_ReturnsResponse_Failed_400()
        {

            ILogger<BundleSubmissionAPIHandler> logger = NullLogger<BundleSubmissionAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"OperationOutcome\",\r\n    \"meta\": {\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-BundleSubmissionResponse\"\r\n        ]\r\n    },\r\n    \"extension\": [\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n            \"valueString\": \"HJG0007228027EC\"\r\n        }\r\n    ],\r\n    \"issue\": [\r\n        {\r\n            \"severity\": \"error\",\r\n            \"code\": \"processing\",\r\n            \"details\": {\r\n                \"coding\": [\r\n                    {\r\n                        \"system\": \"https://terminology.esmduat.cms.gov:8099/fhir/CodeSystem/Esmd-CS-ErrorOrWarningCodes\",\r\n                        \"code\": \"UNIQUE_ID_NOT_MATCHING\",\r\n                        \"display\": \"Unique ID received in the bundle request do no match with the Unique ID sent in the PreSigned URL.\"\r\n                    }\r\n                ]\r\n            }\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<BundleSubmissionAPIFailedResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.BadRequest, expectedResponse);
            var service = new BundleSubmissionAPIHandler(client, logger);

            // Act
            var result = await service.SubmitBundleAsync(url: settings.BundleSubmissionAPI?.EndpointURL!,
                                 token: "eyJraWQiOiJQbUJ...",
                                 request: settings.BundleSubmissionAPI?.Request!,
                                 headerAccept: settings.BundleSubmissionAPI?.Accept!, contentType: settings.BundleSubmissionAPI?.ContentType!,
                                 httpClientRequestTimeOutSeconds: settings.BundleSubmissionAPI!.HttpClientRequestTimeOutSeconds!);

            var res = (BundleSubmissionAPIFailedResponse)result.Value;

            // Assert
            Assert.NotNull(res);



            Assert.True(res?.Issue![0].Severity == "error");


        }

        [Fact]
        public async Task NotficationRetrievalAPIHandler_GetNotificationsAsync_ReturnsResponse_Success_200()
        {

            ILogger<NotficationRetrievalAPIHandler> logger = NullLogger<NotficationRetrievalAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"Bundle\",\r\n    \"id\": \"fa74b960-a1fe-4271-91c7-bd24d5e54972\",\r\n    \"meta\": {\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-FindBundleListNotifications\"\r\n        ],\r\n        \"security\": [\r\n            {\r\n                \"coding\": [\r\n                    {\r\n                        \"system\": \"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                        \"code\": \"V\",\r\n                        \"display\": \"very restricted\"\r\n                    }\r\n                ]\r\n            }\r\n        ]\r\n    },\r\n    \"type\": \"searchset\",\r\n    \"link\": [\r\n        {\r\n            \"relation\": \"self\",\r\n            \"url\": \"https://dev.cpiapigateway.cms.gov/api/esmdf/v1/fhir/List?transaction-status-type=processed&uniqueid=AWSAUTO8T-60e40016-97ab-48ec-bbc4-4d5cd9120dea\"\r\n        }\r\n    ],\r\n    \"entry\": [\r\n        {\r\n            \"fullUrl\": \"https://terminology.esmduat.cms.gov:8099/fhir/List/ZKG0007233150EC-ADMIN-ERROR\",\r\n            \"resource\": {\r\n                \"resourceType\": \"List\",\r\n                \"id\": \"ZKG0007233150EC-ADMIN-ERROR\",\r\n                \"meta\": {\r\n                    \"profile\": [\r\n                        \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-FindListTransactionNotification\"\r\n                    ],\r\n                    \"security\": [\r\n                        {\r\n                            \"coding\": [\r\n                                {\r\n                                    \"system\": \"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                                    \"code\": \"V\",\r\n                                    \"display\": \"very restricted\"\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"contained\": [\r\n                    {\r\n                        \"resourceType\": \"OperationOutcome\",\r\n                        \"id\": \"ZKG0007233150EC-ADMIN-ERROR\",\r\n                        \"issue\": [\r\n                            {\r\n                                \"severity\": \"error\",\r\n                                \"code\": \"processing\",\r\n                                \"details\": {\r\n                                    \"coding\": [\r\n                                        {\r\n                                            \"system\": \"https://terminology.esmduat.cms.gov:8099/fhir/CodeSystem/Esmd-CS-AdminErrors\",\r\n                                            \"code\": \"ESMD_410\",\r\n                                            \"display\": \"Cannot Read Files/Corrupt Files\"\r\n                                        }\r\n                                    ]\r\n                                },\r\n                                \"diagnostics\": \"RC sent administrative error response\"\r\n                            }\r\n                        ]\r\n                    }\r\n                ],\r\n                \"status\": \"current\",\r\n                \"mode\": \"working\",\r\n                \"title\": \"response-notification-admin-error\",\r\n                \"date\": \"2025-03-06T14:24:01.954-05:00\",\r\n                \"extension\": [\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n                        \"valueString\": \"ZKG0007233150EC\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-UniqueId\",\r\n                        \"valueString\": \"AWSAUTO8T-60e40016-97ab-48ec-bbc4-4d5cd9120dea\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-OrganizationId\",\r\n                        \"valueString\": \"urn:oid:123.456.657.127\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ReviewContractorOid\",\r\n                        \"valueCode\": \"urn:oid:2.16.840.1.113883.13.34.110.1.999.1\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-LinesOfBusinessId\",\r\n                        \"valueCode\": \"10\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-RequestId\",\r\n                        \"valueString\": \"esMD-Administrative Response\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-StatusCode\",\r\n                        \"valueCode\": \"ready-to-download\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-RCCreationDateTime\",\r\n                        \"valueDateTime\": \"2025-03-06T14:24:01.955-05:00\"\r\n                    },\r\n                    {\r\n                        \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-RCSubmissionDateTime\",\r\n                        \"valueDateTime\": \"2025-03-06T14:24:01.955-05:00\"\r\n                    }\r\n                ],\r\n                \"entry\": [\r\n                    {\r\n                        \"item\": {\r\n                            \"reference\": \"#ZKG0007233150EC-ADMIN-ERROR\"\r\n                        }\r\n                    }\r\n                ]\r\n            }\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<NotificationRetrievalSuccessResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new NotficationRetrievalAPIHandler(client, logger);

            // Act

            // Act
            var result = await service.GetNotificationsAsync(token: "eyJraWQiOiJQbUJ...",
                                                                    notificationRetrievalAPI: settings.NotificationRetrievalAPI!);

            var res = (NotificationRetrievalSuccessResponse)result.Value;

            // Assert
            Assert.NotNull(res);



            Assert.True(res.Entry![0].Resource!.Id == "ZKG0007233150EC-ADMIN-ERROR");


        }

        [Fact]
        public async Task NotficationRetrievalAPIHandler_GetNotificationsAsync_ReturnsResponse_ReturnsResponse_Failed_401()
        {

            ILogger<NotficationRetrievalAPIHandler> logger = NullLogger<NotficationRetrievalAPIHandler>.Instance;

            var responseStr = "{\r\n  \"resourceType\": \"OperationOutcome\",\r\n  \"id\": \"3-3d197c01-7881-11f0-b7f4-021f554e4299\",\r\n  \"issue\": [\r\n    {\r\n      \"severity\": \"error\",\r\n      \"code\": \"REQUEST_UNMATCHED\",\r\n      \"diagnostics\": \"invalid token\"\r\n    }\r\n  ]\r\n}\r\n";

            var expectedResponse = JsonUtil.FromJson<NotificationRetrievalFailedResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.Unauthorized, expectedResponse);
            var service = new NotficationRetrievalAPIHandler(client, logger);

            // Act
            var result = await service.GetNotificationsAsync(token: "eyJraWQiOiJQbUJ...",
                                                                    notificationRetrievalAPI: settings.NotificationRetrievalAPI!);

            var res = (NotificationRetrievalFailedResponse)result.Value;

            // Assert
            Assert.NotNull(res);



            Assert.True(res?.Issue![0].Severity == "error");


        }

        [Fact]
        public async Task DocumentRetrievalAPIHandler_RetrieveDocumentAsync_ReturnsResponse_Success_200()
        {

            ILogger<DocumentRetrievalAPIHandler> logger = NullLogger<DocumentRetrievalAPIHandler>.Instance;

            var responseStr = "{\r\n   \"resourceType\":\"Bundle\",\r\n   \"id\":\"3f187b67-468e-4227-b2d8-1177dec8df78\",\r\n   \"meta\":{\r\n      \"profile\":[\r\n         \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-FindBundleDocumentReferences\"\r\n      ],\r\n      \"security\":[\r\n         {\r\n                  \"system\":\"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                  \"code\":\"V\",\r\n                  \"display\":\"very restricted\"\r\n               }\r\n            \r\n      ]\r\n   },\r\n   \"type\":\"searchset\",\r\n   \"total\":1,\r\n   \"link\":[\r\n      {\r\n         \"relation\":\"self\",\r\n         \"url\":\"https://dev.cpiapigateway.cms.gov/api/esmdf/v1/fhir/DocumentReference?transaction-status-type=ready-to-download\"\r\n      }\r\n   ],\r\n   \"entry\":[\r\n      {\r\n         \"fullUrl\":\"https://terminology.esmduat.cms.gov:8099/fhir/DocumentReference/TXE0007232564EC-LETTERS\",\r\n         \"resource\":{\r\n            \"resourceType\":\"DocumentReference\",\r\n            \"id\":\"TXE0007232564EC-LETTERS\",\r\n            \"meta\":{\r\n               \"profile\":[\r\n                  \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-FindDocumentReference\"\r\n               ]\r\n            },\r\n            \"extension\":[\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n                  \"valueString\":\"TXE0007232564EC\"\r\n               },\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-OrganizationId\",\r\n                  \"valueString\":\"urn:oid:123.456.657.126\"\r\n               },\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ReviewContractorOid\",\r\n                  \"valueCode\":\"urn:oid:2.16.840.1.113883.13.34.110.1.999.1\"\r\n               },\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-NPI\",\r\n                  \"valueString\":\"1992853964\"\r\n               },\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-RequestType\",\r\n                  \"valueString\":\"LETTERS\"\r\n               },\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-LinesOfBusinessId\",\r\n                  \"valueCode\":\"20\"\r\n               },\r\n               {\r\n                  \"url\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-StatusCode\",\r\n                  \"valueCode\":\"ready-to-download\"\r\n               }\r\n            ],\r\n            \"identifier\":[\r\n               {\r\n                  \"system\":\"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Idn-LetterId\",\r\n                  \"value\":\"AWSAUTO2T_10542720250305\"\r\n               }\r\n            ],\r\n            \"status\":\"current\",\r\n            \"date\":\"2025-03-05T10:54:31.053-05:00\",\r\n            \"securityLabel\":[\r\n               {\r\n                  \"coding\":[\r\n                     {\r\n                        \"system\":\"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                        \"code\":\"V\",\r\n                        \"display\":\"very restricted\"\r\n                     }\r\n                  ]\r\n               }\r\n            ],\r\n            \"content\":[\r\n               {\r\n                  \"attachment\":{\r\n                     \"id\":\"TXE0007232564EC_20250305105429_1\",\r\n                     \"contentType\":\"application/pdf\",\r\n                     \"language\":\"en-US\",\r\n                     \"url\":\"/Binary/TXE0007232564EC_20250305105429_1\",\r\n                     \"size\":17033,\r\n                     \"hash\":\"df3c2fab54ff1ac8c0bd2bb602e7c39d916b0d916e3c06ebc0ae7f99058910b9\",\r\n                     \"title\":\"LETTERS\",\r\n                     \"creation\":\"2025-03-05T10:54:29.625-05:00\"\r\n                  },\r\n                  \"format\":{\r\n                     \"system\":\"https://terminology.esmduat.cms.gov:8099/fhir/ValueSet/Esmd-VS-FormatCode\",\r\n                     \"code\":\"urn:ihe:iti:xds-sd:pdf:2008\",\r\n                     \"display\":\"Unstructured Clinical Document (PDF)\"\r\n                  }\r\n               },\r\n               {\r\n                  \"attachment\":{\r\n                     \"id\":\"TXE0007232564EC_20250305105429_2\",\r\n                     \"contentType\":\"application/json\",\r\n                     \"language\":\"en-US\",\r\n                     \"url\":\"/Binary/TXE0007232564EC_20250305105429_2\",\r\n                     \"size\":9198,\r\n                     \"hash\":\"81d68f49eae77c3473e217879c7c403e7c96829b4cc31f5b13a999fbc5b93802\",\r\n                     \"title\":\"LETTERS\",\r\n                     \"creation\":\"2025-03-05T10:54:29.695-05:00\"\r\n                  },\r\n                  \"format\":{\r\n                     \"system\":\"https://terminology.esmduat.cms.gov:8099/fhir/ValueSet/Esmd-VS-FormatCode\",\r\n                     \"code\":\"urn:ihe:iti:xds-sd:text:2008\",\r\n                     \"display\":\"Unstructured Clinical Document (Text)\"\r\n                  }\r\n               }\r\n            ],\r\n            \"context\":{\r\n               \"facilityType\":{\r\n                  \"coding\":[\r\n                     {\r\n                        \"system\":\"https://terminology.esmduat.cms.gov:8099/fhir/ValueSet/Esmd-VS-FacilityTypeCode\",\r\n                        \"code\":\"cms-rc\",\r\n                        \"display\":\"CMS Review Contractor\"\r\n                     }\r\n                  ]\r\n               }\r\n            }\r\n         }\r\n      }\r\n   ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<DocumentRetrievalAPISuccessResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new DocumentRetrievalAPIHandler(client, logger);

            // Act

            var result = await service.RetrieveDocumentAsync(token: "eyJraWQiOiJQbUJ...",
                                                                   documentRetrievalAPI: settings?.DocumentRetrievalAPI!);

            var res = (DocumentRetrievalAPISuccessResponse)result.Value;

            // Assert
            Assert.NotNull(res);


            Assert.True(res.Entry![0].Resource!.ResourceType == "DocumentReference");

            Assert.NotNull(res.Entry![0].Resource!.Id);


        }

        [Fact]
        public async Task DeliveryConfirmationAPIHandler_ProcessDeliveryConfirmationAsync_ReturnsResponse_Success_200()
        {

            ILogger<DeliveryConfirmationAPIHandler> logger = NullLogger<DeliveryConfirmationAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"List\",\r\n    \"id\": \"24753\",\r\n    \"meta\": {\r\n        \"versionId\": \"1\",\r\n        \"lastUpdated\": \"2025-03-21T12:04:07.449-04:00\",\r\n        \"source\": \"#NizRfvfyZLFphTyD\",\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-ListDeliveryConfirmation\"\r\n        ],\r\n        \"security\": [\r\n            {\r\n                \"system\": \"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                \"code\": \"V\",\r\n                \"display\": \"very restricted\"\r\n            }\r\n        ]\r\n    },\r\n    \"contained\": [\r\n        {\r\n            \"resourceType\": \"OperationOutcome\",\r\n            \"id\": \"WBS0007236864EC-OperationOutcome\",\r\n            \"meta\": {\r\n                \"profile\": [\r\n                    \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-OperationOutcome\"\r\n                ]\r\n            },\r\n            \"issue\": [\r\n                {\r\n                    \"severity\": \"information\",\r\n                    \"code\": \"informational\",\r\n                    \"diagnostics\": \"Successful Provider delivery ack\"\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"extension\": [\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n            \"valueString\": \"WBS0007236864EC\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-OrganizationId\",\r\n            \"valueString\": \"urn:oid:123.456.657.127\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-DeliveryDateTime\",\r\n            \"valueDateTime\": \"2025-03-21T11:25:40.111-04:00\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-NPI\",\r\n            \"valueString\": \"1568425007\"\r\n        }\r\n    ],\r\n    \"status\": \"current\",\r\n    \"mode\": \"working\",\r\n    \"title\": \"Submission Set Title\",\r\n    \"date\": \"2025-03-21T11:25:40-04:00\",\r\n    \"entry\": [\r\n        {\r\n            \"item\": {\r\n                \"reference\": \"#WBS0007236864EC-OperationOutcome\"\r\n            }\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<DeliveryConfirmationAPIResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new DeliveryConfirmationAPIHandler(client, logger);

            // Act


            var result = await service.ProcessDeliveryConfirmationAsync(token: "eyJraWQiOiJQbUJ...",
                                                                    deliveryConfirmationAPI: settings?.DeliveryConfirmationAPI!);

            var res = (DeliveryConfirmationAPIResponse)result.Value;

            // Assert
            Assert.NotNull(res);


            Assert.True(res.Contained![0].Id == "WBS0007236864EC-OperationOutcome");


        }

        [Fact]
        public async Task DeliveryConfirmationAPIHandler_ProcessDeliveryConfirmationAsync_ReturnsResponse_422()
        {

            ILogger<DeliveryConfirmationAPIHandler> logger = NullLogger<DeliveryConfirmationAPIHandler>.Instance;

            var responseStr = "{\r\n  \"resourceType\": \"OperationOutcome\",\r\n  \"meta\": {\r\n    \"profile\": [\r\n      \"https://terminology.esmdval.cms.gov:8099/fhir/StructureDefinition/Esmd-OperationOutcome\"\r\n    ]\r\n  },\r\n  \"issue\": [\r\n    {\r\n      \"severity\": \"error\",\r\n      \"code\": \"processing\",\r\n      \"details\": {\r\n        \"coding\": [\r\n          {\r\n            \"system\": \"https://terminology.esmdval.cms.gov:8099/fhir/CodeSystem/Esmd-CS-ErrorOrWarningCodes\",\r\n            \"code\": \"INVALID_COMBINATION_ORGANIZATION_ID_CLIENT_ID\",\r\n            \"display\": \"ESMD could not extract the data due to invalid combination of Organization ID and Client ID .\"\r\n          }\r\n        ]\r\n      }\r\n    }\r\n  ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<DeliveryConfirmationAPIFailedResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.UnprocessableEntity, expectedResponse);
            var service = new DeliveryConfirmationAPIHandler(client, logger);

            // Act


            var result = await service.ProcessDeliveryConfirmationAsync(token: "eyJraWQiOiJQbUJ...",
                                                                    deliveryConfirmationAPI: settings?.DeliveryConfirmationAPI!);

            var res = (DeliveryConfirmationAPIFailedResponse)result.Value;

            // Assert
            Assert.NotNull(res);


            Assert.True(res.Issue![0].Severity == "error");



        }

        [Fact]
        public async Task PractitionerAPIHandler_ProcessPractitionerRequestAsync_ReturnsResponse_Success_200()
        {

            ILogger<PractitionerAPIHandler> logger = NullLogger<PractitionerAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"Practitioner\",\r\n    \"id\": \"PRACT123\",\r\n    \"identifier\": {\r\n        \"system\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n        \"value\": \"NXG0001235991EC\"\r\n    },\r\n    \"meta\": {\r\n        \"versionId\": \"4554\",\r\n        \"lastUpdated\": \"2025-05-01T14:39:12.151-04:00\",\r\n        \"source\": \"#VX69I8RmIuawJR5b\",\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Practitioner\"\r\n        ],\r\n        \"security\": [\r\n            {\r\n                \"system\": \"http://terminology.hl7.org/CodeSystem/v3-Confidentiality\",\r\n                \"code\": \"V\",\r\n                \"display\": \"very restricted\"\r\n            }\r\n        ]\r\n    },\r\n    \"name\": [\r\n        {\r\n            \"family\": \"Gaven\",\r\n            \"given\": [\r\n                \"Radems\"\r\n            ],\r\n            \"prefix\": [\r\n                \"Dr\"\r\n            ],\r\n            \"suffix\": [\r\n                \"Ph.D\"\r\n            ]\r\n        }\r\n    ],\r\n    \"telecom\": [\r\n        {\r\n            \"system\": \"phone\",\r\n            \"value\": \"551-333-6141\",\r\n            \"use\": \"work\"\r\n        }\r\n    ],\r\n    \"address\": [\r\n        {\r\n            \"use\": \"work\",\r\n            \"line\": [\r\n                \"1003 MILLERS DR\"\r\n            ],\r\n            \"city\": \"ELLICOTT CITY\",\r\n            \"state\": \"MD\",\r\n            \"postalCode\": \"21043\"\r\n        }\r\n    ],\r\n    \"gender\": \"female\",\r\n    \"active\": true,\r\n    \"extension\": [\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-NPI\",\r\n            \"valueString\": \"1700962487\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ProviderTaxId\",\r\n            \"valueString\": \"101-56-55\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ActionRequestedCode\",\r\n            \"valueCode\": \"A\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ServiceCode\",\r\n            \"valueCode\": \"EEP\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ServiceStartDate\",\r\n            \"valueDate\": \"2025-10-01\"\r\n        },\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-ServiceEndDate\",\r\n            \"valueDate\": \"2026-12-30\"\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<PractitionerAPISuccessResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var service = new PractitionerAPIHandler(client, logger);

            // Act


            var practitionerId = settings?.PractitionerAPI?.Request?.Id!;

            


            var result = await service.ProcessPractitionerRequestAsync(token: "eyJraWQiOiJQbUJ...",
                                                                     practitionerId: practitionerId,
                                                                practitionerSettingAPI: settings?.PractitionerAPI!);

            var res = (PractitionerAPISuccessResponse)result.Value;

            // Assert
            Assert.NotNull(res);


            Assert.True(res.Id == "PRACT123");


        }

        public async Task PractitionerAPIHandler_ProcessPractitionerRequestAsync__ReturnsResponse_422()
        {

            ILogger<PractitionerAPIHandler> logger = NullLogger<PractitionerAPIHandler>.Instance;

            var responseStr = "{\r\n    \"resourceType\": \"OperationOutcome\",\r\n    \"meta\": {\r\n        \"profile\": [\r\n            \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-BundleSubmissionResponse\"\r\n        ]\r\n    },\r\n    \"extension\": [\r\n        {\r\n            \"url\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Ext-TransactionId\",\r\n            \"valueString\": \"DLU0007230900EC\"\r\n        }\r\n    ],\r\n    \"issue\": [\r\n        {\r\n            \"extension\": [\r\n                {\r\n                    \"url\": \"http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-line\",\r\n                    \"valueInteger\": 1\r\n                },\r\n                {\r\n                    \"url\": \"http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-col\",\r\n                    \"valueInteger\": 476\r\n                },\r\n                {\r\n                    \"url\": \"http://hl7.org/fhir/StructureDefinition/operationoutcome-message-id\",\r\n                    \"valueString\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Practitioner#esmd-npi-1\"\r\n                }\r\n            ],\r\n            \"severity\": \"error\",\r\n            \"code\": \"processing\",\r\n            \"details\": {\r\n                \"coding\": [\r\n                    {\r\n                        \"system\": \"http://hl7.org/fhir/java-core-messageId\",\r\n                        \"code\": \"https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Practitioner#esmd-npi-1\"\r\n                    }\r\n                ]\r\n            },\r\n            \"diagnostics\": \"Constraint failed: esmd-npi-1: 'NPI should be 10 numeric' (defined in https://terminology.esmduat.cms.gov:8099/fhir/StructureDefinition/Esmd-Practitioner)\",\r\n            \"location\": [\r\n                \"Practitioner.extension[0].value.ofType(string)\",\r\n                \"Line[1] Col[476]\"\r\n            ]\r\n        }\r\n    ]\r\n}";

            var expectedResponse = JsonUtil.FromJson<PractitionerAPIFailedResponse>(responseStr);

            var client = CreateMockHttpClient(HttpStatusCode.UnprocessableEntity, expectedResponse);
            var service = new PractitionerAPIHandler(client, logger);

            // Act


            var practitionerId = settings?.PractitionerAPI?.Request?.Id!;




            var result = await service.ProcessPractitionerRequestAsync(token: "eyJraWQiOiJQbUJ...",
                                                                     practitionerId: practitionerId,
                                                                practitionerSettingAPI: settings?.PractitionerAPI!);

            var res = (PractitionerAPIFailedResponse)result.Value;

            // Assert
            Assert.NotNull(res);


            Assert.True(res?.Issue?[0].Severity == "error");


        }



    }

}