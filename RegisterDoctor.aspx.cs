using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem
{
    public partial class RegisterDoctor : System.Web.UI.Page
    {
        SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Could load specialties dynamically from DB in future
            }
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
            if (!Page.IsValid)
                return;

            string fullName = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text.Trim();
            string speciality = ddlSpeciality.SelectedValue;
            string location = txtLocation.Text.Trim();
            string dob = txtDob.Text.Trim();

            if (IsEmailRegistered(email))
            {
                lblError.Text = "This email is already registered.";
                return;
            }

            string hashedPassword = sha256_hash(password);
            int userId = 0;

            try
            {
                open_conn();

                int roleId = GetRoleId("Doctor");
                if (roleId == 0)
                {
                    lblError.Text = "Doctor role not found in the database.";
                    return;
                }

                string insertUserQuery = @"
                    INSERT INTO Users (FullName, Email, Phone, PasswordHash, RoleId, DateOfBirth)
                    OUTPUT INSERTED.UserId
                    VALUES (@FullName, @Email, @Phone, @PasswordHash, @RoleId, @DOB);
                ";

                using (SqlCommand cmd = new SqlCommand(insertUserQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@RoleId", roleId);
                    cmd.Parameters.AddWithValue("@DOB", string.IsNullOrEmpty(dob) ? (object)DBNull.Value : dob);
                    userId = (int)cmd.ExecuteScalar();
                }

                string insertDoctorQuery = @"
                    INSERT INTO Doctors (UserId, Speciality, Location)
                    VALUES (@UserId, @Speciality, @Location);
                ";

                using (SqlCommand cmd = new SqlCommand(insertDoctorQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Speciality", speciality);
                    cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(location) ? (object)DBNull.Value : location);
                    cmd.ExecuteNonQuery();
                }

                Session["UserIdVal"] = userId.ToString();
                Session["UserNameVal"] = fullName;
                Session["NameVal"] = fullName;
                Session["UserRoleVal"] = "Doctor";

                Response.Redirect("DoctorDashboard.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "An error occurred: " + ex.Message;
            }
            finally
            {
                close_conn();
            }
        }

        private int GetRoleId(string roleName)
        {
            string query = "SELECT RoleId FROM Roles WHERE RoleName = @RoleName";
            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@RoleName", roleName);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        protected void open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            if (Conn.State != System.Data.ConnectionState.Open)
                Conn.Open();
        }

        protected void close_conn()
        {
            if (Conn != null && Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
                Conn.Dispose();
            }
        }

        private string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
    }
}
