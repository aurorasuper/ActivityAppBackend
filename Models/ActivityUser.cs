using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json.Serialization;

namespace ActivityJournal.Models
{
    public partial class ActivityUser
    {
        public ActivityUser()
        {
            ActivityLogs = new HashSet<ActivityLog>();
        }

        public int UsrId { get; set; }
        public string UsrFirstName { get; set; } = null!;
        public string UsrLastName { get; set; } = null!;
        [DataType(DataType.EmailAddress)]
        public string UsrEmail { get; set; } = null!;
        [DataType(DataType.Password)]
        [JsonIgnore]
        public byte[] UsrPassword { get; set; } = null!;
        [JsonIgnore]
        public byte[] UsrSalt { get; set; } = null!;
        public bool UsrIsAdmin { get; set; }
       
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
        public int GetId(string email, out string errormsg)
        {
            // create connection
            SqlConnection dBConnection = new SqlConnection();
            int i = 0;

            //establish connection to SQL server
            dBConnection.ConnectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Books;Integrated Security=True;";

            // sql string and get user from database
            String sqlsting = "SELECT [Usr_ID] FROM [Activity_Users] WHERE Usr_Email=@email";
            SqlCommand dbCommand = new SqlCommand(sqlsting, dBConnection);
            dbCommand.Parameters.Add("email", SqlDbType.NVarChar, 150).Value = email;
            SqlDataReader reader;
            errormsg = "";
            try
            {
                dBConnection.Open();
                // start and read from reader
                reader = dbCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        i = Convert.ToInt32(reader["Usr_Id"]);
                    }
                    reader.Close();
                    return i;
                }
                else
                {
                    errormsg = "Could not user Id from database";
                    return -1;
                }

            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return -1;
            }
            finally
            {
                dBConnection.Close();
            }
        }


    }


}
