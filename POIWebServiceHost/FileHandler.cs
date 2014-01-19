using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POIWebServiceContracts;
using System.IO;
using System.Net;
using System.ServiceModel.Web;

namespace POIWebServiceHost
{
    static class FileHandler
    {
        //Store the file on the file system and store its info in the database
        static public bool CreatePointOfInterestOnFile(string id, string fileName, Stream fileStream, string description, ref int newFileId)
        {
            string pathAndFileName = PrepareFilePath(fileName, id);
            int fileSize = 0;

            bool fileCreated = CreateFile(pathAndFileName, fileStream, ref fileSize);

            PoiFile poiFile = new PoiFile();
            poiFile.Name = fileName;
            poiFile.Filetype = fileName.Substring(fileName.LastIndexOf("."));

            if (string.IsNullOrEmpty(description))
                poiFile.Description = "No description";
            else
                poiFile.Description = description;
            poiFile.Filesize = fileSize;
            poiFile.PoiId = int.Parse(id);

            bool fileInfoStoredInDatabase = DatabaseHandler.GetInstance().AddFileToPoi(poiFile, ref newFileId);

            return fileCreated && fileInfoStoredInDatabase;
        }

        //Prepare the paths and directories for the file.
        static private String PrepareFilePath(string fileName, string id)
        {
            string pathAndFileName = null;
            try
            {
                string path = "../../../Uploads/POI";
                string specificPoiPath = Path.Combine(path, id);
                string fileFolderPath = Path.Combine(specificPoiPath, GetFileTypeFolder(fileName));
                pathAndFileName = Path.Combine(fileFolderPath, fileName);

                if (!Directory.Exists(specificPoiPath))
                {
                    Directory.CreateDirectory(specificPoiPath);
                }

                if (!Directory.Exists(fileFolderPath))
                {
                    Directory.CreateDirectory(fileFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to prepare file path: {0}", ex.Message);
            }

            return pathAndFileName;
        }

        //Get the right filefolder type depending on if it is an image or a file
        static private String GetFileTypeFolder(String fileName)
        {
            string fileTypeFolder = "Files";

            foreach (string extension in Service.validImageExtensions)
            {
                if (fileName.EndsWith("." + extension, false, null))
                    fileTypeFolder = "Images";
            }

            return fileTypeFolder;
        }

        //Return the file from the filesystem, Complete with errorhandling/responsehandling
        static public Stream GetPointOfInterestFileFromSystem(string id, string fileId, string fileName)
        {
            String path = PrepareFilePath(fileName, id);
            FileStream fileStream = null;

            try
            {
                if (ServiceHelper.GetParsedId(id) == 0 || ServiceHelper.GetParsedId(fileId) == 0)
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                else if (DatabaseHandler.GetInstance().GetPoiFile(ServiceHelper.GetParsedId(id), ServiceHelper.GetParsedId(fileId)) == null)
                    throw new Exception("File not found in database");
                else
                    fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

                if (GetFileTypeFolder(fileName) == "Images")
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/" + fileName.Substring(fileName.LastIndexOf(".") + 1);
                else
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/" + fileName.Substring(fileName.LastIndexOf(".") + 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem fetching file: {0}", ex.Message);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
            }

            return fileStream;
        }

        //Store the file on the filesystem
        static private bool CreateFile(String pathAndFileName, Stream fileStream, ref int fileSize)
        {
            bool fileCreated = true;
            try
            {
                FileStream stream = new FileStream(pathAndFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                const int bufferLength = 4096;
                byte[] buffer = new byte[bufferLength];
                int count = 0;

                while ((count = fileStream.Read(buffer, 0, bufferLength)) > 0)
                {
                    fileSize += bufferLength;
                    stream.Write(buffer, 0, count);
                }

                stream.Close();
                fileStream.Close();

                Console.WriteLine("Uploaded file {0} with {1} bytes", pathAndFileName, fileSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create file: {0}", ex.StackTrace);
                fileCreated = false;
            }
            return fileCreated;
        }

        //Complete with errorhandling
        static public void DeletePoiFile(string id, string fileId)
        {
            PoiFile poiFile = DatabaseHandler.GetInstance().GetPoiFile(ServiceHelper.GetParsedId(id), ServiceHelper.GetParsedId(fileId));

            if (ServiceHelper.GetParsedId(id) == 0 || ServiceHelper.GetParsedId(fileId) == 0)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else if (poiFile == null)
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            else
            {
                bool deletedFromDb = DatabaseHandler.GetInstance().DeletePoiFile(poiFile);

                bool deletedFromFs = DeleteFile(PrepareFilePath(poiFile.Name, poiFile.PoiId.ToString()));

                if (deletedFromDb && deletedFromFs)
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                else
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
            }
        }

        //Needs more extensive error handling, eg. if old file was not deleted what happens? if new file is not created, assign the correct status codes.. and so on...
        static public void UpdateFile(string id, string fileId, string fileName, Stream fileStream, string description)
        {
            PoiFile poiFile = DatabaseHandler.GetInstance().GetPoiFile(ServiceHelper.GetParsedId(id), ServiceHelper.GetParsedId(fileId));

            if (ServiceHelper.GetParsedId(id) == 0 || ServiceHelper.GetParsedId(fileId) == 0)
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
            else if (poiFile == null)
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound("Could not found the requested resource, make sure you have entered the right id");
            else
            {
                int fileSize = 0;
                string oldFilePath = PrepareFilePath(poiFile.Name, id);
                DeleteFile(oldFilePath);

                string newFilePath = PrepareFilePath(fileName, id);
                CreateFile(newFilePath, fileStream, ref fileSize);

                if (string.IsNullOrEmpty(description))
                    poiFile.Description = "No description";
                else
                    poiFile.Description = description;

                poiFile.Name = fileName;
                poiFile.Filesize = fileSize;
                poiFile.Filetype = fileName.Substring(fileName.LastIndexOf("."));

                DatabaseHandler.GetInstance().UpdatePoiFile(poiFile);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            }
        }

        //Delete the file from the filesystem
        static private bool DeleteFile(String pathAndFileName)
        {
            bool fileDeleted = true;
            try
            {
                File.Delete(pathAndFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to delete file: {0}", ex.StackTrace);
                fileDeleted = false;
            }
            return fileDeleted;
        }

        

        //Complete with errorhandling
        static public void DeleteAllPoiFilesFromSystem(string id)
        {
            string path = "../../../Uploads/POI/" + id;

            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
