using Face.Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Face.Service.FaceService
{
    public interface IFaceService
    {
        #region group management

        /// <summary>
        /// list group
        /// </summary>
        /// <returns></returns>
        Task<(bool success, List<(string personGroupId, string name, string userData)> data, string errorMessage)> ListGroup();

        /// <summary>
        /// get a person group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <returns></returns>
        Task<(bool success, (string personGroupId, string name, string userData) data, string errorMessage)> GetGroup(string personGroupId);

        /// <summary>
        /// new person group
        /// </summary>
        /// <param name="personGroupId">Id of group</param>
        /// <param name="groupName">Name of group</param>
        /// <param name="userData">Some data related to group</param>
        /// <returns></returns>
        Task<(bool success, string data)> CreateGroup(string personGroupId, string groupName, string userData);

        /// <summary>
        /// update group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="groupName"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        Task<(bool success, string data)> UpdateGroup(string personGroupId, string groupName, string userData);

        /// <summary>
        /// delete group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <returns></returns>
        Task<(bool success, string data)> DeleteGroup(string personGroupId);

        /// <summary>
        /// train person group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <returns></returns>
        Task<(bool success, string data)> TrainGroup(string personGroupId);

        /// <summary>
        /// get person group training status
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <returns></returns>
        Task<(bool success, (string status, string createdDateTime, string lastActionDateTime, string message) data, string errorMessage)> TrainingStatus(string personGroupId);

        #endregion

        #region person management

        /// <summary>
        /// list persons in a specified group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <returns></returns>
        Task<(bool success, List<(string personId, string name, string userData, List<string> persistedFaceIds)> data, string errorMessage)> PersonsInGroup(string personGroupId);

        Task<(bool success, (string personId, string name, string userData, List<string> persistedFaceIds) data, string errorMessage)> GetPerson(string personGroupId, string personId);

        /// <summary>
        /// add a person to a specified group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="name"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        Task<(bool success, string data)> AddPersonToGroup(string personGroupId, string name, string userData);

        /// <summary>
        /// update person in group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="personId"></param>
        /// <param name="name"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        Task<(bool success, string data)> UpdatePerson(string personGroupId, string personId, string name, string userData);

        /// <summary>
        /// delete person from group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        Task<(bool success, string data)> DeletePerson(string personGroupId, string personId);

        #endregion

        #region Person face management

        /// <summary>
        /// add person face to person in group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="personId"></param>
        /// <param name="fileContents"></param>
        /// <param name="userData"></param>
        /// <param name="targetFace"></param>
        /// <returns></returns>
        Task<(bool success, string data)> AddPersonFace(string personGroupId, string personId, byte[] fileContents, string userData = "", string targetFace = "");

        /// <summary>
        /// update person face for person in group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="personId"></param>
        /// <param name="persistedFaceId"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        Task<(bool success, string data)> UpdatePersonFace(string personGroupId, string personId, string persistedFaceId, string userData);

        /// <summary>
        /// delete person face from person in group
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="personId"></param>
        /// <param name="persistedFaceId"></param>
        /// <returns></returns>
        Task<(bool success, string data)> DeletePersonFace(string personGroupId, string personId, string persistedFaceId);

        /// <summary>
        /// get person face
        /// </summary>
        /// <param name="personGroupId"></param>
        /// <param name="personId"></param>
        /// <param name="persistedFaceId"></param>
        /// <returns></returns>
        Task<(bool success, string data, string errorMessage)> GetPersistedFace(string personGroupId, string personId, string persistedFaceId);

        #endregion

        #region face list management

        /// <summary>
        /// create face list
        /// </summary>
        /// <param name="faceListId"></param>
        /// <param name="name"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        Task<(bool success, string data)> CreateFaceList(string faceListId, string name, string userData = "");

        /// <summary>
        /// update face list
        /// </summary>
        /// <param name="faceListId"></param>
        /// <param name="name"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        Task<(bool success, string data)> UpdateFaceList(string faceListId, string name, string userData);

        /// <summary>
        /// Delete face list
        /// </summary>
        /// <param name="faceListId"></param>
        /// <returns></returns>
        Task<(bool success, string data)> DeleteFaceList(string faceListId);

        /// <summary>
        /// get face list
        /// </summary>
        /// <param name="faceListId"></param>
        /// <returns></returns>

        Task<(bool success, (string faceListId, string name, string userData, List<string> persistedFaces) data, string errorMessage)> GetFaceList(string faceListId);

        /// <summary>
        /// get all face lists
        /// </summary>
        /// <returns></returns>
        Task<(bool success, List<(string faceListId, string name, string userData)> data, string errorMessage)> GetFaceLists();

        /// <summary>
        /// add face to face list
        /// </summary>
        /// <param name="faceListId"></param>
        /// <param name="faceUrl"></param>
        /// <param name="userData"></param>
        /// <param name="targetFace"></param>
        /// <returns></returns>
        Task<(bool success, string data)> AddFaceToFaceList(string faceListId, string faceUrl, string userData = "", string targetFace = "");

        /// <summary>
        /// remove face from face list
        /// </summary>
        /// <param name="faceListId"></param>
        /// <param name="persistedFaceId"></param>
        /// <returns></returns>
        Task<(bool success, string data)> DeleteFaceFromFaceList(string faceListId, string persistedFaceId);

        #endregion

        #region face group

        Task<(bool success, GroupResult data, string errorMessage)> GroupFaces(List<string> faceListId);

        #endregion

        #region main

        /// <summary>
        /// detect
        /// </summary>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        Task<(bool success, List<DetectResult> data, string errorMessage)> DetectFace(byte[] fileContents);

        /// <summary>
        /// identify
        /// </summary>
        /// <param name="personGroupId">personGroupId of the target person group, created by Person Group - Create a Person Group.</param>
        /// <param name="faceIds">Array of query faces faceIds, created by the Face - Detect. Each of the faces are identified independently. The valid number of faceIds is between [1, 10].</param>
        /// <param name="maxNumOfCandidatesReturned">The range of maxNumOfCandidatesReturned is between 1 and 5 (default is 1).</param>
        /// <param name="confidenceThreshold">Optional parameter. Confidence threshold of identification, used to judge whether one face belong to one person.The range of confidenceThreshold is [0, 1] (default specified by algorithm).</param>
        /// <returns></returns>
        Task<(bool success, List<IdentifyResult> data, string errorMessage)> Identify(string personGroupId, string[] faceIds, int maxNumOfCandidatesReturned = 1, double? confidenceThreshold = null);

        Task<(bool success, VerifyResult result, string errorMessage)> VerifyFaceToFace(List<string> faceIds);

        Task<(bool success, VerifyResult result, string errorMessage)> VerifyFaceToPerson(string groupId, string personId, string faceId);

        Task<(bool success, List<FindSimilarResult> data, string errorMessage)> FindSimilar(string mode, string faceId, List<string> faceIds, int? maxNumOfCandidates = 20);

        #endregion
    }
}
