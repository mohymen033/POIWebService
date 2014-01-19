using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using Microsoft.Http;
using Microsoft.ServiceModel.Web;


namespace POIWebServiceContracts
{
    [ServiceContract(Namespace="")]
    public interface POIService
    {

    /****************POI secion begins here***********************/
        
        //GET
        [OperationContract]
        [WebHelp(Comment = "Gets a list of all points of interest.")]
        [WebGet(UriTemplate = "poi")]
        PointsOfInterest GetPointsOfInterest();

        [OperationContract]
        [WebHelp(Comment = "Gets the point of interest given a specific id. Will throw [HTTP 404 - Not Found] if nothing could be found. In case the id is invalid [HTTP 400 - Bad request] will be thrown.")]
        [WebGet(UriTemplate = "poi/{id}")]
        PointOfInterest GetPointOfInterest(string id);
            
        //POST
        [OperationContract]
        [WebHelp(Comment="Create a new point of interest by passing an xml or json. Will throw [HTTP 400 - Bad Request] if the request could not be fulfilled. Else [HTTP 201 - Created] will be returned. Click 'Request Example' to see the structure of a point of interest object. Please note that you cannot pass an ImageList or FileList with this operation.")]
        [WebInvoke(Method = "POST", UriTemplate = "poi")]
        void CreatePointOfInterest(PointOfInterest newPointOfInterest);

        //PUT
        [OperationContract]
        [WebHelp(Comment = "Update an existing point of interest by passing an xml or json. Will throw [HTTP 400 - Bad Request] if the request could not be fulfilled else [HTTP 201 - Created] will be returned. Click 'Request Example' to see the structure of a point of interest object. Please note that you cannot pass an ImageList or FileList with this operation.")]
        [WebInvoke(Method = "PUT", UriTemplate = "poi/{id}")]
        void PutPointOfInterest(PointOfInterest updatedPointOfInterest, string id);
        
        //DELETE
        [OperationContract]
        [WebHelp(Comment = "Delete an existing point of interest given a specific id. Will throw [HTTP 400 - Bad Request] if id was invalid, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if the point of interest was successfully deleted.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "poi/{id}")]
        void DeletePointOfInterest(string id);

    /****************Image secion begins here***********************/

        //GET
        [OperationContract]
        [WebHelp(Comment = "Get information about a specific image belonging to the point of interest. Will throw [HTTP 400 - Bad Request] if the id's are invalid or [HTTP 404 - Not Found] if nothing could be found with the passed id's.")]
        [WebGet(UriTemplate = "poi/{id}/image/{imageId}")]
        PoiFile GetPointOfInterestImageInfo(string id, string ImageId);

        [OperationContract]
        [WebHelp(Comment = "Show the specific image belonging to the point of interest. Will throw [HTTP 400 - Bad Request] if the id's are invalid or [HTTP 404 - Not Found] if nothing could be found with the passed id's.")]
        [WebGet(UriTemplate = "poi/{id}/image/{imageId}/{fileName}")]
        Stream GetPointOfInterestImage(string id, string imageId, string fileName);
            
        //POST
        [OperationContract]
        [WebHelp(Comment = "Create a new image belonging to the point of interest by passing an image and an optional description to the above specified URI. Will throw [HTTP 404 - Not Found] if no point of interest could be found with the specified id.")]
        [WebInvoke(Method = "POST", UriTemplate = "poi/{id}/image/{fileName}?description={description}")]
        void CreatePointOfInterestImage(string id, string fileName, Stream fileStream, string description);
            
        //PUT
        [OperationContract] //Replace only image description
        [WebHelp(Comment = "Update the image description by using the above specified URI. Will throw [HTTP 400 - Bad Request] if id was invalid or no description was set, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if it was successfully updated.")]
        [WebInvoke(Method = "PUT", UriTemplate = "poi/{id}/image/{imageId}?description={description}")]
        void PutPointOfInterestImageInfo(string id, string imageId, string description);

        [OperationContract] //Replace both image and description
        [WebHelp(Comment = "Update the image by passing a new image and an optional description by using the above specified URI. Will throw [HTTP 400 - Bad Request] if id was invalid or no description was set, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if it was successfully updated.")]
        [WebInvoke(Method = "PUT", UriTemplate = "poi/{id}/image/{imageId}/{fileName}?description={description}")]
        void PutPointOfInterestImage(string id, string imageId, string fileName, Stream fileStream, string description);

        //DELETE
        [OperationContract]
        [WebHelp(Comment = "Delete the image given the specific id's. Will throw [HTTP 400 - Bad Request] if id's are invalid or no description was set, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if it was successfully deleted.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "poi/{id}/image/{imageId}")]
        void DeletePointOfInterestImage(string id, string imageId);

 
    /****************File secion begins here***********************/

        //GET
        [OperationContract]
        [WebHelp(Comment = "Get information about a specific file belonging to the point of interest. Will throw [HTTP 400 - Bad Request] if the id's are invalid or [HTTP 404 - Not Found] if nothing could be found with the passed id's.")]
        [WebGet(UriTemplate = "poi/{id}/file/{fileId}")]
        PoiFile GetPointOfInterestFileInfo(string id, string fileId);

        [OperationContract]
        [WebHelp(Comment = "Show the specific file belonging to the point of interest. Will throw [HTTP 400 - Bad Request] if the id's are invalid or [HTTP 404 - Not Found] if nothing could be found with the passed id's.")]
        [WebGet(UriTemplate = "poi/{id}/file/{fileId}/{fileName}")]
        Stream GetPointOfInterestFile(string id, string fileId, string fileName);

        //POST
        [OperationContract]
        [WebHelp(Comment = "Create a new file belonging to the point of interest by passing an file and an optional description to the above specified URI. Will throw [HTTP 404 - Not Found] if no point of interest could be found with the specified id.")]
        [WebInvoke(Method = "POST", UriTemplate = "poi/{id}/file/{fileName}?description={description}")]
        void CreatePointOfInterestFile(string id, string fileName, Stream fileStream, string description);

        //PUT
        [OperationContract] //Replace only file description
        [WebHelp(Comment = "Update the file description by using the above specified URI. Will throw [HTTP 400 - Bad Request] if id was invalid or no description was set, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if it was successfully updated.")]
        [WebInvoke(Method = "PUT", UriTemplate = "poi/{id}/file/{fileId}?description={description}")]
        void PutPointOfInterestFileInfo(string id, string fileId, string description);

        [OperationContract] //Replace both file and description
        [WebHelp(Comment = "Update the file description by using the above specified URI. Will throw [HTTP 400 - Bad Request] if id was invalid or no description was set, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if it was successfully updated.")]
        [WebInvoke(Method = "PUT", UriTemplate = "poi/{id}/file/{fileId}/{fileName}?description={description}")]
        void PutPointOfInterestFile(string id, string fileId, string fileName, Stream fileStream, string description);

        //DELETE
        [OperationContract]
        [WebHelp(Comment = "Delete the file given the specific id's. Will throw [HTTP 400 - Bad Request] if id's are invalid or no description was set, [HTTP 404 - Not Found] if nothing was found or [HTTP 200 - OK] if it was successfully deleted.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "poi/{id}/file/{fileId}")]
        void DeletePointOfInterestFile(string id, string fileId);
 
    /*^^^^^^^^^^^^^^^^File secion ends here*************************/

        [OperationContract]
        [WebHelp(Comment = "")]
        [WebInvoke(Method = "POST", UriTemplate="key")]
        void CreateUserKey(string username);

        bool AuthenticateUser(string user);

        bool IsValidUserKey(string key, string uri);
         
        bool ValidateHash(string userid, string uri, string hash);
        
    
    }

    [DataContract(Name="POI", Namespace="")]
    public class PointOfInterest
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Presentation { get; set; }
        [DataMember]
        public Uri Link { get; set; }
        [DataMember]
        public ImageList ImageList { get; set; }
        [DataMember]
        public FileList FileList { get; set; }
    }

    public class PoiFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Filetype { get; set; }
        public string Description { get; set; }
        public int Filesize { get; set; }
        public int PoiId { get; set; }
        public Uri LinkToFile { get; set; }
    }
  
    [CollectionDataContract(Name="POIList", ItemName="POI", KeyName="Id", ValueName="Link", Namespace="")]
    public class PointsOfInterest : Dictionary<int, Uri>
    {
        public PointsOfInterest() { }
        public PointsOfInterest(Dictionary<int, Uri> pointOfInterests) : base(pointOfInterests) {}

        public Uri GetPoiLink(int id) 
        {
            Uri uri = null;
            foreach (KeyValuePair<int, Uri> kvp in this)
            {
                if (id == kvp.Key)
                    uri = kvp.Value;
            }
            return uri;
        }

    }

    [CollectionDataContract(Name = "ImageList", ItemName = "Image", Namespace = "")]
    public class ImageList : List<Uri>
    {
        public ImageList() {}
        public ImageList(List<Uri> imageList) : base(imageList) {}
    }

    [CollectionDataContract(Name = "FileList", ItemName = "File", Namespace = "")]
    public class FileList : List<Uri>
    {
        public FileList() { }
        public FileList(List<Uri> fileList) : base(fileList) { }
    }
}