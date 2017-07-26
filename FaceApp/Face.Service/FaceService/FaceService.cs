using Face.Core.Settings;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Face.Service.Models;
using System.Linq;

namespace Face.Service.FaceService
{
    public class FaceService : IFaceService
    {
        #region Fields

        private readonly HttpClient _client;

        #endregion

        #region constructors

        public FaceService(IOptions<GlobalAppSetting> globalAppSetting)
        {
            var faceSetting = globalAppSetting.Value.FaceSetting;

            //config eltoro enpoint
            _client = new HttpClient { BaseAddress = new Uri(faceSetting.ApiEndpoint) };
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceSetting.SubscriptionKey);
        }

        #endregion

        #region utilities
        private async Task<(bool success, VerifyResult result, string errorMessage)> VerifyFace(string json)
        {
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var uri = "verify";
            var response = await _client.PostAsync(uri, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, JsonConvert.DeserializeObject<VerifyResult>(responseJson), string.Empty);

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }
        #endregion

        #region group management methods

        public async Task<(bool success, List<(string personGroupId, string name, string userData)> data, string errorMessage)> ListGroup()
        {
            var response = await _client.GetAsync("persongroups");
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(responseJson);
                var groups = new List<(string personGroupId, string name, string userData)>();
                foreach (var item in data)
                    groups.Add((item["personGroupId"], item["name"], item["userData"]));

                return (true, groups, null);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, null, errors.Error.Message);
        }

        public async Task<(bool success, (string personGroupId, string name, string userData) data, string errorMessage)> GetGroup(string personGroupId)
        {
            var uri = $"persongroups/{personGroupId}";
            var response = await _client.GetAsync(uri);

            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
                return (true, (data["personGroupId"], data["name"], data["userData"]), string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, ("", "", ""), errors.Error.Message);
        }

        public async Task<(bool success, string data)> CreateGroup(string personGroupId, string groupName, string userData)
        {
            string json = "{\"name\":\"" + groupName + "\", \"userData\":\"" + userData + "\"}";
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var uri = $"persongroups/{personGroupId}";
            var response = await _client.PutAsync(uri, content);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, personGroupId);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> UpdateGroup(string personGroupId, string groupName, string userData)
        {
            string json = "{\"name\":\"" + groupName + "\", \"userData\":\"" + userData + "\"}";
            var method = new HttpMethod("PATCH");
            var uri = $"persongroups/{personGroupId}";
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(request);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, personGroupId);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> DeleteGroup(string personGroupId)
        {
            var uri = $"persongroups/{personGroupId}";
            var response = await _client.DeleteAsync(uri);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> TrainGroup(string personGroupId)
        {
            var uri = $"persongroups/{personGroupId}/train";
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _client.PostAsync(uri, content);

            //success
            if (response.StatusCode == HttpStatusCode.Accepted)
                return (true, personGroupId);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, (string status, string createdDateTime, string lastActionDateTime, string message) data, string errorMessage)> TrainingStatus(string personGroupId)
        {
            var uri = $"persongroups/{personGroupId}/training";
            var response = await _client.GetAsync(uri);

            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
                return (true, (data["status"], data["createdDateTime"], data["lastActionDateTime"], data["message"]), string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, ("", "", "", ""), errors.Error.Message);
        }

        #endregion

        #region person management methods

        public async Task<(bool success, List<(string personId, string name, string userData, List<string> persistedFaceIds)> data, string errorMessage)> PersonsInGroup(string personGroupId)
        {
            var uri = $"persongroups/{personGroupId}/persons";
            var response = await _client.GetAsync(uri);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseJson);
                var persons = new List<(string personId, string name, string userData, List<string> persistedFaceIds)>();
                foreach (var item in data)
                    persons.Add((item["personId"] as string, item["name"] as string, item["userData"] as string, JsonConvert.DeserializeObject<List<string>>(item["persistedFaceIds"].ToString())));

                return (true, persons, null);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }

        public async Task<(bool success, (string personId, string name, string userData, List<string> persistedFaceIds) data, string errorMessage)> GetPerson(string personGroupId, string personId)
        {
            var uri = $"persongroups/{personGroupId}/persons/{personId}";
            var response = await _client.GetAsync(uri);

            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson);
                return (true,
                    (data["personId"] as string,
                    data["name"] as string,
                    data["userData"] as string,
                    JsonConvert.DeserializeObject<List<string>>(data["persistedFaceIds"].ToString())),
                    string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, ("", "", "", null), errors.Error.Message);
        }

        public async Task<(bool success, string data)> AddPersonToGroup(string personGroupId, string name, string userData)
        {
            string json = "{\"name\":\"" + name + "\", \"userData\":\"" + userData + "\"}";
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var uri = $"persongroups/{personGroupId}/persons";
            var response = await _client.PostAsync(uri, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
                return (true, data["personId"]);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> UpdatePerson(string personGroupId, string personId, string name, string userData)
        {

            string json = "{\"name\":\"" + name + "\", \"userData\":\"" + userData + "\"}";
            var method = new HttpMethod("PATCH");
            var uri = $"persongroups/{personGroupId}/persons/{personId}";
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(request);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, personId);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> DeletePerson(string personGroupId, string personId)
        {
            var uri = $"persongroups/{personGroupId}/persons/{personId}";
            var response = await _client.DeleteAsync(uri);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        #endregion

        #region person face management

        public async Task<(bool success, string data)> AddPersonFace(string personGroupId, string personId, byte[] fileContents, string userData = "", string targetFace = "")
        {
            var byteContent = new ByteArrayContent(fileContents);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var uri = $"persongroups/{personGroupId}/persons/{personId}/persistedFaces?userData={userData}&targetFace={targetFace}";
            var response = await _client.PostAsync(uri, byteContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
                return (true, data["persistedFaceId"]);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> UpdatePersonFace(string personGroupId, string personId, string persistedFaceId, string userData)
        {
            string json = "{\"userData\":\"" + userData + "\"}";
            var method = new HttpMethod("PATCH");
            var uri = $"persongroups/{personGroupId}/persons/{personId}/persistedFaces/{persistedFaceId}";
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(request);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> DeletePersonFace(string personGroupId, string personId, string persistedFaceId)
        {
            var uri = $"persongroups/{personGroupId}/persons/{personId}/persistedFaces/{persistedFaceId}";
            var response = await _client.DeleteAsync(uri);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data, string errorMessage)> GetPersistedFace(string personGroupId, string personId, string persistedFaceId)
        {
            var uri = $"persongroups/{personGroupId}/persons/{personId}/persistedFaces/{persistedFaceId}";
            var response = await _client.GetAsync(uri);

            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson);
                return (true, data["userData"] as string, string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, null, errors.Error.Message);
        }

        #endregion

        #region face list management

        public async Task<(bool success, string data)> CreateFaceList(string faceListId, string name, string userData = "")
        {
            string json = "{\"name\":\"" + name + "\", \"userData\":\"" + userData + "\"}";
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var uri = $"facelists/{faceListId}";
            var response = await _client.PutAsync(uri, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> UpdateFaceList(string faceListId, string name, string userData)
        {
            string json = "{\"name\":\"" + name + "\", \"userData\":\"" + userData + "\"}";
            var method = new HttpMethod("PATCH");
            var uri = $"facelists/{faceListId}";
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(request);

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, faceListId);

            //error
            var responseJson = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> DeleteFaceList(string faceListId)
        {
            var uri = $"facelists/{faceListId}";
            var response = await _client.DeleteAsync(uri);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, (string faceListId, string name, string userData, List<string> persistedFaces) data, string errorMessage)> GetFaceList(string faceListId)
        {
            var uri = $"facelists/{faceListId}";
            var response = await _client.GetAsync(uri);

            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson);
                return (true,
                    (data["faceListId"] as string,
                    data["name"] as string,
                    data["userData"] as string,
                    JsonConvert.DeserializeObject<List<string>>(data["persistedFaces"].ToString())),
                    string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);

            return (false, ("", "", "", null), errors.Error.Message);
        }

        public async Task<(bool success, List<(string faceListId, string name, string userData)> data, string errorMessage)> GetFaceLists()
        {
            var uri = "facelists";
            var response = await _client.GetAsync(uri);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseJson);
                var faceLists = new List<(string faceListId, string name, string userData)>();
                foreach (var item in data)
                    faceLists.Add((item["faceListId"] as string, item["name"] as string, item["userData"] as string));

                return (true, faceLists, null);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }

        public async Task<(bool success, string data)> AddFaceToFaceList(string faceListId, string faceUrl, string userData = "", string targetFace = "")
        {
            string json = "{\"url\":\"" + faceUrl + "\"}";
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var uri = $"facelists/{faceListId}/persistedFaces?userData={userData}&targetFace={targetFace}";
            var response = await _client.PostAsync(uri, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
                return (true, data["persistedFaceId"]);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        public async Task<(bool success, string data)> DeleteFaceFromFaceList(string faceListId, string persistedFaceId)
        {
            var uri = $"facelists/{faceListId}/persistedFaces/{persistedFaceId}";
            var response = await _client.DeleteAsync(uri);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, string.Empty);

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, errors.Error.Message);
        }

        #endregion

        #region face group

        public async Task<(bool success, GroupResult data, string errorMessage)> GroupFaces(List<string> faceListId)
        {
            string json = "{\"faceIds\":[";
            foreach (var faceId in faceListId)
                json += "\"" + faceId + "\",";
            // remove last comma
            json = json.Remove(json.Length - 1);
            json += "]}";

            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var uri = "group";
            var response = await _client.PostAsync(uri, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
                return (true, JsonConvert.DeserializeObject<GroupResult>(responseJson), null);

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }

        #endregion

        #region main

        public async Task<(bool success, List<DetectResult> data, string errorMessage)> DetectFace(byte[] fileContents)
        {
            var byteContent = new ByteArrayContent(fileContents);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await _client.PostAsync("detect?returnFaceId=true&returnFaceAttributes=age,gender,smile,facialHair,glasses,headPose,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise", byteContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<DetectResult>>(responseJson);
                return (true, data, string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }

        public async Task<(bool success, List<IdentifyResult> data, string errorMessage)> Identify(string personGroupId, string[] faceIds, int maxNumOfCandidatesReturned = 1, double? confidenceThreshold = null)
        {
            var requestJson = new
            {
                personGroupId = personGroupId,
                faceIds = faceIds,
                maxNumOfCandidatesReturned = maxNumOfCandidatesReturned,
                confidenceThreshold = confidenceThreshold
            };

            var response = await _client.PostAsync("identify", new StringContent(JsonConvert.SerializeObject(requestJson), Encoding.UTF8, "application/json"));
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<IdentifyResult>>(responseJson);
                return (true, data, string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }

        public async Task<(bool success, VerifyResult result, string errorMessage)> VerifyFaceToFace(List<string> faceIds)
        {
            string json = "{\"faceId1\":\"" + faceIds.First() + "\", \"faceId2\":\"" + faceIds.Last() + "\"}";
            return await VerifyFace(json);
        }

        public async Task<(bool success, VerifyResult result, string errorMessage)> VerifyFaceToPerson(string groupId, string personId, string faceId)
        {
            string json = "{\"faceId\":\"" + faceId + "\", \"personId\":\"" + personId + "\", \"personGroupId\":\"" + groupId + "\"}";
            return await VerifyFace(json);
        }

        public async Task<(bool success, List<FindSimilarResult> data, string errorMessage)> FindSimilar(string mode, string faceId, List<string> faceIds, int? maxNumOfCandidates = 20)
        {
            var requestJson = new
            {
                faceId = faceId,
                faceIds = faceIds,
                maxNumOfCandidatesReturned = maxNumOfCandidates,
                mode = mode
            };

            var response = await _client.PostAsync("findsimilars", new StringContent(JsonConvert.SerializeObject(requestJson), Encoding.UTF8, "application/json"));
            var responseJson = await response.Content.ReadAsStringAsync();

            //success
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = JsonConvert.DeserializeObject<List<FindSimilarResult>>(responseJson);
                return (true, data, string.Empty);
            }

            //error
            var errors = JsonConvert.DeserializeObject<FaceError>(responseJson);
            return (false, null, errors.Error.Message);
        }
        #endregion
    }

    public class FaceError
    {
        public Error Error { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
