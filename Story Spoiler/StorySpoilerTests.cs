using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using Story_Spoiler.Models;
using System.Net;
using System.Text.Json;

namespace Story_Spoiler
{
    [TestFixture]
    public class StorySpolerTests
    {
        private RestClient client;
        private static string storyId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("Angel123", "123456");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        // All tests here
        [Test, Order(1)]
        public void CreateStory_WithRequiredFields_ShouldReturnCreatedStory()
        {
            // Arrange
            var storyRequest = new StoryDTO
            {
                Title = "Test Story",
                Description = "This is a test story description.",
                Url = ""
            };

            // Act
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);
            var response = this.client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            storyId = json.GetProperty("storyId").GetString() ?? string.Empty;
            Assert.That(storyId, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnEditedStory()
        {
            //Arrane
            var editRequest = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an edited story description.",
                Url = ""

            };

            // Act
            var request = new RestRequest($"/api/Story/Edit/{storyId}", Method.Put);
            request.AddQueryParameter("storyId", storyId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStory_ShouldReturnList()
        {
            // Act
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)(HttpStatusCode.OK)));
            var storys = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(storys, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnDeleted()
        {
            // Act
            var request = new RestRequest($"/api/Story/Delete/{storyId}", Method.Delete);
            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)(HttpStatusCode.OK)));
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutReqiredFields_ShouldReturnBadRequest()
        {
            // Arrange
            var story = new
            {
                Title = "",
                Description = ""
            };

            // Act
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)(HttpStatusCode.BadRequest)));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            // Arrange
            string nonExistingStoryId = "123";
            var editRequest = new StoryDTO
            {
                Title = "Edited Non-Existing-Story",
                Description = "This is updated test story description for a non-existing story.",
                Url = ""
            };

            // Act
            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddQueryParameter("storyId", nonExistingStoryId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            // Act
            string nonExistingStoryId = "123";
            var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);
            request.AddQueryParameter("storyId", nonExistingStoryId);
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}