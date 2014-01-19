using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POIWebServiceContracts;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net.Mail;
using System.Runtime.Serialization;

namespace POIWebServiceHost
{
    public class Service : POIService
    {
        public static string[] validImageExtensions = { "jpeg", "jpg", "gif", "png" };


        public PointsOfInterest GetPointsOfInterest()
        {
            Dictionary<int, Uri> pois = DatabaseHandler.GetInstance().GetPointsOfInterest();
            return new PointsOfInterest(pois);
        }
        
        //If the id was malformed give response of bad request
        public PointOfInterest GetPointOfInterest(string id)
        {
            PointOfInterest poi = DatabaseHandler.GetInstance().GetPointOfInterest(ServiceHelper.GetParsedId(id));

            if (ServiceHelper.GetParsedId(id) == 0)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else if (poi == null)
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            return poi;
        }

        //Complete with errorhandling
        public void CreatePointOfInterest(PointOfInterest newPointOfInterest)
        {
            int idOfAddedPoi = DatabaseHandler.GetInstance().AddPointOfInterest(newPointOfInterest);

            if (idOfAddedPoi == -1)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else
                WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(GetPointsOfInterest().GetPoiLink(idOfAddedPoi));
        }


        //Complete with errorhandling
        public void PutPointOfInterest(PointOfInterest updatedPointOfInterest, string id)
        {
            PointOfInterest poi = DatabaseHandler.GetInstance().GetPointOfInterest(ServiceHelper.GetParsedId(id));

            if (ServiceHelper.GetParsedId(id) == 0)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else if (poi == null)
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            else
            {
                if (!string.IsNullOrEmpty(updatedPointOfInterest.Name))
                    poi.Name = updatedPointOfInterest.Name;
                if (!string.IsNullOrEmpty(updatedPointOfInterest.Presentation))
                    poi.Presentation = updatedPointOfInterest.Presentation;
                DatabaseHandler.GetInstance().UpdatePointOfInterest(poi);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            }
        }

        //Complete with errorhandling
        public void DeletePointOfInterest(string id)
        {
            PointOfInterest poi = GetPointOfInterest(id);

            if (ServiceHelper.GetParsedId(id) == 0)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else if (poi == null)
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            else
            {
                DatabaseHandler.GetInstance().DeletePointOfInterest(ServiceHelper.GetParsedId(id));
                FileHandler.DeleteAllPoiFilesFromSystem(id);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            }
        }



    /****************Image secion begins here***********************/

        //Complete with errorhandling
        public PoiFile GetPointOfInterestImageInfo(string id, string imageId)
        {
            PoiFile poiFile = null;

            if (ServiceHelper.GetParsedId(id) != 0 && ServiceHelper.GetParsedId(imageId) != 0)
            {
                poiFile = DatabaseHandler.GetInstance().GetPoiFile(ServiceHelper.GetParsedId(id), ServiceHelper.GetParsedId(imageId));
                if (poiFile == null)
                    WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            }
            else
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;

            return poiFile;
        }

        public Stream GetPointOfInterestImage(string id, string imageId, string fileName)
        {
            return FileHandler.GetPointOfInterestFileFromSystem(id, imageId, fileName);
        }

        //Complete with errorhandling
        public void CreatePointOfInterestImage(string id, string fileName, Stream fileStream, string description)
        {
            int newFileId = -1;
            if (FileHandler.CreatePointOfInterestOnFile(id, fileName, fileStream, description, ref newFileId))
                WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + id + "/image/" + newFileId));
            else
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
        }

        //Complete with errorhandling
        public void PutPointOfInterestImageInfo(string id, string imageId, string description)
        {
            UpdateFileInfo(id, imageId, description);
        }


        //Needs some better errorhandling
        public void PutPointOfInterestImage(string id, string imageId, string fileName, Stream fileStream, string description)
        {
            FileHandler.UpdateFile(id, imageId, fileName, fileStream, description);
        }

        public void DeletePointOfInterestImage(string id, string imageId)
        {
            FileHandler.DeletePoiFile(id, imageId);
        }



    /****************File secion begins here***********************/

        //Complete with errorhandling
        public PoiFile GetPointOfInterestFileInfo(string id, string fileId)
        {
            PoiFile poiFile = null;

            if (ServiceHelper.GetParsedId(id) != 0 && ServiceHelper.GetParsedId(fileId) != 0)
            {
                poiFile = DatabaseHandler.GetInstance().GetPoiFile(ServiceHelper.GetParsedId(id), ServiceHelper.GetParsedId(fileId));
                if(poiFile == null)
                    WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            }
            else
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;

            return poiFile;
        }

        public Stream GetPointOfInterestFile(string id, string fileId, string fileName)
        {
            return FileHandler.GetPointOfInterestFileFromSystem(id, fileId, fileName);
        }


        //Complete with errorhandling
        public void CreatePointOfInterestFile(string id, string fileName, Stream fileStream, string description)
        {
            int newFileId = -1;
            if (FileHandler.CreatePointOfInterestOnFile(id, fileName, fileStream, description, ref newFileId))
                WebOperationContext.Current.OutgoingResponse.SetStatusAsCreated(new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + id + "/file/" + newFileId));
            else
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
        }

        //Complete with errorhandling
        public void PutPointOfInterestFileInfo(string id, string fileId, string description)
        {
            UpdateFileInfo(id, fileId, description);
        }

        //Needs some better errorhandling
        public void PutPointOfInterestFile(string id, string fileId, string fileName, Stream fileStream, string description)
        {
            FileHandler.UpdateFile(id, fileId, fileName, fileStream, description);
        }

        public void DeletePointOfInterestFile(string id, string fileId)
        {
            FileHandler.DeletePoiFile(id, fileId);
        }

        //General both image and file operations

        public void UpdateFileInfo(string id, string fileId, string description)
        {
            PoiFile poiFile = DatabaseHandler.GetInstance().GetPoiFile(ServiceHelper.GetParsedId(id), ServiceHelper.GetParsedId(fileId));

            if (ServiceHelper.GetParsedId(id) == 0 || ServiceHelper.GetParsedId(fileId) == 0 || string.IsNullOrEmpty(description))
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else if (poiFile == null)
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            else
            {
                poiFile.Description = description;
                DatabaseHandler.GetInstance().UpdatePoiFile(poiFile);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            }
        }


    /****************Authentication secion begins here***********************/

        public bool AuthenticateUser(string user)
        {
            WebOperationContext ctx = WebOperationContext.Current;
            string requestUri = ctx.IncomingRequest.UriTemplateMatch.RequestUri.ToString();
            string authHeader = ctx.IncomingRequest.Headers[HttpRequestHeader.Authorization];
            // if supplied hash is valid, user is authenticated
            if (IsValidUserKey(authHeader, requestUri))
                return true;
            return false;
        }
        public bool IsValidUserKey(string key, string uri)
        {
            string[] authParts = key.Split(':');
            if (authParts.Length == 2)
            {
                string userid = authParts[0];
                string hash = authParts[1];
                if (ValidateHash(userid, uri, hash))
                    return true;
            }
            return false;
        }


        public bool ValidateHash(string userid, string uri, string hash)
        {
            Dictionary<string, string> userKeys = DatabaseHandler.GetInstance().GetUserKeys(userid);
            
            if (!userKeys.ContainsKey(userid))
                return false;
            string userkey = userKeys[userid];
            byte[] secretBytes = ASCIIEncoding.ASCII.GetBytes(userkey);
            HMACMD5 hmac = new HMACMD5(secretBytes);
            byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(uri);
            byte[] computedHash = hmac.ComputeHash(dataBytes);
            string computedHashString = Convert.ToBase64String(computedHash);
           
            return computedHashString.Equals(hash);
        }

        //Example on how to use the authorization
        //if (!AuthenticateUser(WebOperationContext.Current.IncomingRequest.Headers.Get("Authorization")))
        //{
        //    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
        //    return null;
        //}

        public void CreateUserKey(string username)
        {
            //if (!AuthenticateUser(WebOperationContext.Current.IncomingRequest.Headers.Get("Authorization")))
            //{
            //    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
            //    return;
            //}

            String secretKey = GenerateSecretKey(username);
            bool keyAdded = DatabaseHandler.GetInstance().AddUserKey(username, secretKey);

            if (keyAdded)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NoContent;
            else
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Conflict;
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Warning",  "Failed to fulfill the request. Username: " + username + " might already exist in database");
            }
        }

        public string GenerateSecretKey(string userid)
        {
            byte[] userIdInBytes = ASCIIEncoding.ASCII.GetBytes(userid);
            HMACMD5 hmac = new HMACMD5();
            
            return Convert.ToBase64String(hmac.ComputeHash(userIdInBytes));
        }
    }
}
