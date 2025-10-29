using System;
using System.Data.SqlClient; // Use SqlConnection and SqlCommand
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem
{
    public partial class Register : System.Web.UI.Page
    {
        SqlConnection Conn; // Changed from OleDbConnection to SqlConnection
        string Sqlst;

        protected void Page_Load(object sender, EventArgs e)
        {
            var x = Session["UserIdVal"]?.ToString();
        }
        private bool IsEmailRegistered(string email)
        {
            try
            {
                open_conn();
                string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            finally
            {
                close_conn();
            }
        }
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (int.Parse(DateTime.Now.Date.ToString("yyyyMMdd")) > 20230701)
            {
                Session["UserNameVal"] = "";
                Session["NameVal"] = "";
                Session["UserIdVal"] = "";
                Session["UserRoleVal"] = "";
            }
            
            // Get input values from the form
            string username = txtName.Text.Trim().Replace("'", "''"); 
            string email = txtEmail.Text.Trim().Replace("'", "''");   
            string phone = txtPhone.Text.Trim().Replace("'", "''");   
            string password = txtPassword.Text.Trim();
            string hashedPassword = sha256_hash(password);

            if (IsEmailRegistered(email))
            {
                lblError.Text = "This email is already registered.";
                return;
            }
            open_conn();

            try
            {
                // SQL query to insert data into the Users table
                string SqlstInsert = "INSERT INTO Users (Name, Phone, Email, PasswordHash, ImageUrl) ";
                SqlstInsert += "VALUES ('" + username + "', '" + phone + "', '" + email + "', '" + hashedPassword + "', NULL);";

                // Execute the insert query
                exec_sql(SqlstInsert);

                // SQL query to retrieve the newly inserted user's data
                string SqlstSelect = "SELECT Id FROM Users WHERE Email = '" + email.Replace("'", "''") + "'";

                int userId = -1;
                using (SqlCommand cmdSelect = new SqlCommand(SqlstSelect, Conn))
                {
                    using (SqlDataReader reader = cmdSelect.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = Convert.ToInt32(reader["Id"]);
                        }
                    }
                }

                if (userId > 0)
                {
                    // Assign a default role ("User") to the newly registered user
                    string SqlstRoleInsert = "INSERT INTO UserRoles (UserId, RoleId) ";
                    SqlstRoleInsert += "VALUES (" + userId + ", 2);"; // Assuming RoleId = 2 corresponds to "User"

                    exec_sql(SqlstRoleInsert);

                    // Set session variables
                    Session["UserIdVal"] = userId.ToString();
                    Session["UserNameVal"] = username;
                    Session["NameVal"] = username;
                    Session["UserRoleVal"] = "User"; // Default role

                    // Debugging: Verify session values
                    lblDebug.Text = "UserIdVal: " + Session["UserIdVal"] +
                                    ", UserNameVal: " + Session["UserNameVal"] +
                                    ", UserRoleVal: " + Session["UserRoleVal"];
                }

                // Redirect to another page after successful registration
                Response.Redirect("Default.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "An error occurred: " + ex.Message;
            }
            finally
            {
                // Close the connection
                close_conn();
            }
        }
        protected void open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }

        protected void close_conn()
        {
            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
                Conn.Dispose();
            }
        }

        protected void exec_sql(String SqlstP)
        {
            using (SqlCommand Comm = new SqlCommand(SqlstP, Conn)) // Use SqlCommand instead of OleDbCommand
            {
                try
                {
                    Comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    lblError.Text = "SQL Execution Error: " + ex.Message;
                }
            }
        }

        private string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
    }
}