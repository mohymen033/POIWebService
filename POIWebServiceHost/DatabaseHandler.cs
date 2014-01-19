using System;
using System.Collections.Generic;
using System.Linq;
using POIWebServiceContracts;
using System.Text;

using System.Configuration;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;

namespace POIWebServiceHost
{
    class DatabaseHandler
    {
        private SqlConnection connection = null;
        private static DatabaseHandler instance = null;
        private const string connectionStringKey = "POIWebServiceHost.Properties.Settings.POIDBConnectionString";
        private string connectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;

        //Get the current instance of Databasehandler, if no instance exists create a new
        public static DatabaseHandler GetInstance()
        {
            if (instance == null)
            {
                instance = new DatabaseHandler();
            }
            return instance;
        }

        //Get point of interest given an id, also assign the correct image and filelist to the point of interest
        public PointOfInterest GetPointOfInterest(int id)
        {
            PointOfInterest poi = null;
            SqlCommand command = new SqlCommand("SELECT * FROM PointOfInterest WHERE Id = " + id, GetConnection());
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                poi = new PointOfInterest(); //Should maybe be initialized
                poi.Id = (int)reader["id"];
                poi.Name = reader["name"].ToString();
                poi.Presentation = reader["presentation"].ToString();
                poi.Link = new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + reader["id"]);
            }

            command.Connection.Close();

            if (poi != null)
            {
                poi.ImageList = new ImageList(GetImageListFromPoi(poi.Id));
                poi.FileList = new FileList(GetFileListFromPoi(poi.Id));
            }
            return poi;
        }

        //Add a new point of interest in the database, returning the id of the added point of interest
        public int AddPointOfInterest(PointOfInterest poi)
        {
            int id = -1;
            try
            {
                SqlCommand command = new SqlCommand("INSERT INTO PointOfInterest (Name, Presentation) VALUES (@Name, @Presentation); SELECT CAST(scope_identity() AS int)", GetConnection());
                command.Parameters.AddWithValue("@Name", poi.Name);
                command.Parameters.AddWithValue("@Presentation", poi.Presentation);
                id = (int)command.ExecuteScalar();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }

        public void DeletePointOfInterest(int id)
        {
            try
            {
                SqlCommand command = new SqlCommand("DELETE FROM PointOfInterest WHERE Id = " + id, GetConnection());
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool DeletePoiFile(PoiFile poiFile)
        {
            bool deletedPoiFile = true;
            try
            {
                SqlCommand command = new SqlCommand("DELETE FROM Images WHERE Id = " + poiFile.Id, GetConnection());
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                deletedPoiFile = false;
            }
            return deletedPoiFile;
        }


        public void UpdatePointOfInterest(PointOfInterest poi)
        {
            try
            {
                SqlCommand command = new SqlCommand("UPDATE PointOfInterest SET Name = @Name, Presentation = @Presentation WHERE Id = " + poi.Id, GetConnection());
                command.Parameters.AddWithValue("@Name", poi.Name);
                command.Parameters.AddWithValue("@Presentation", poi.Presentation);
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdatePoiFile(PoiFile poiFile)
        {
            try
            {
                SqlCommand command = new SqlCommand("UPDATE Images SET Name = @Name, Description = @Description, Filetype = @Filetype, Filesize = @Filesize WHERE Id = " + poiFile.Id, GetConnection());
                command.Parameters.AddWithValue("@Name", poiFile.Name);
                command.Parameters.AddWithValue("@Description", poiFile.Description);
                command.Parameters.AddWithValue("@Filetype", poiFile.Filetype);
                command.Parameters.AddWithValue("@Filesize", poiFile.Filesize);
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Insert new file info 
        public bool AddFileToPoi(PoiFile poiFile, ref int newFileId)
        {
            bool addedFile = true;
            try
            {
                SqlCommand command = new SqlCommand("INSERT INTO Images (Name, Filetype, Description, Filesize, POI_Id) VALUES (@Name, @Filetype, @Description, @Filesize, @PoiId); SELECT CAST(scope_identity() AS int)", GetConnection());
                command.Parameters.AddWithValue("@Name", poiFile.Name);
                command.Parameters.AddWithValue("@Filetype", poiFile.Filetype);
                command.Parameters.AddWithValue("@Description", poiFile.Description);
                command.Parameters.AddWithValue("@Filesize", poiFile.Filesize);
                command.Parameters.AddWithValue("@PoiId", poiFile.PoiId);
                newFileId = (int)command.ExecuteScalar();

                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("File was not added: {0}", ex.Message);
                addedFile = false;
            }

            return addedFile;
        }

        //Get file based on the point of interest id and the file id
        public PoiFile GetPoiFile(int poiId, int id)
        {
            SqlCommand command = new SqlCommand("SELECT * FROM Images WHERE POI_id = " + poiId + " AND Id = " + id, GetConnection());
            SqlDataReader reader = command.ExecuteReader();
            PoiFile poiFile = null;
            while (reader.Read())
            {
                poiFile = new PoiFile();
                poiFile.Id = int.Parse(reader["Id"].ToString());
                poiFile.Name = reader["Name"].ToString();
                poiFile.Filetype = reader["Filetype"].ToString().Substring(1);
                poiFile.Description = reader["Description"].ToString();
                poiFile.Filesize = int.Parse(reader["Filesize"].ToString());
                poiFile.PoiId = poiId;
                if (Service.validImageExtensions.Contains<string>(poiFile.Filetype.ToLower()))
                    poiFile.LinkToFile = new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + poiId + "/Image/" + reader["Id"] + "/" + poiFile.Name);
                else
                    poiFile.LinkToFile = new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + poiId + "/File/" + reader["Id"] + "/" + poiFile.Name);
            }
            command.Connection.Close();
            return poiFile;
        }

        //Gets the imagelist that belongs to the poi
        public List<Uri> GetImageListFromPoi(int id)
        {
            List<Uri> imageList = new List<Uri>();

            SqlCommand command = new SqlCommand("SELECT * FROM Images WHERE POI_Id = " + id, GetConnection());
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (Service.validImageExtensions.Contains<string>(reader["Filetype"].ToString().Substring(1)))
                    imageList.Add(new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + id + "/Image/" + reader["Id"]));
            }
            command.Connection.Close();

            return imageList;
        }

        //Gets the filelist that belongs to the poi
        public List<Uri> GetFileListFromPoi(int id)
        {
            List<Uri> fileList = new List<Uri>();

            SqlCommand command = new SqlCommand("SELECT * FROM Images WHERE POI_Id = " + id, GetConnection());
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!Service.validImageExtensions.Contains<string>(reader["Filetype"].ToString().Substring(1)))
                    fileList.Add(new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + id + "/File/" + reader["Id"]));
            }
            command.Connection.Close();

            return fileList;
        }

        //Gets a dictionary of all pois
        public Dictionary<int, Uri> GetPointsOfInterest()
        {
            Dictionary<int, Uri> pointsOfInterest = new Dictionary<int, Uri>();
            SqlCommand command = new SqlCommand("SELECT * FROM PointOfInterest", GetConnection());
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                pointsOfInterest.Add((int)reader["id"], new Uri(Program.HostAddress.AbsoluteUri + "/POI/" + reader["id"]));
            }
            command.Connection.Close();
            return pointsOfInterest;
        }

        //Get Dictionary of UserKeys for use with authentication
        public Dictionary<string, string> GetUserKeys(string username)
        {
            Dictionary<string, string> UserKeys = new Dictionary<string, string>();
            SqlCommand command = new SqlCommand("SELECT * FROM Users WHERE Username = '" + username + "'", GetConnection());
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                UserKeys.Add(reader["Username"].ToString(), reader["SecretKey"].ToString());
            }
            command.Connection.Close();
            return UserKeys;
        }

        public bool AddUserKey(string username, string secretKey)
        {
            bool addedUserKey = true;
            try
            {
                SqlCommand command = new SqlCommand("INSERT INTO Users (Username, SecretKey) VALUES (@Username, @SecretKey)", GetConnection());
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@SecretKey", secretKey);
                command.ExecuteScalar();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Key was not added: {0}", ex.Message);
                addedUserKey = false;
            }

            return addedUserKey;
        }

        //Returns and open the existing connection to the database, if no connection exists create new one
        private SqlConnection GetConnection()
        {
            if (connection == null)
            {
                connection = new SqlConnection(connectionString);
            }
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }


    }
}
