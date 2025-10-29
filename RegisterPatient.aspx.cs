using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem
{
    public partial class RegisterPatient : System.Web.UI.Page
    {
        SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Reserved for future use
            }
        }

        // ✅ Check if the email is already registered
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
            string fullName = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text.Trim();
            string dob = txtDOB.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(dob))
            {
                lblError.Text = "Please fill in all required fields.";
                return;
            }

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

                // Step 1: Get Patient RoleId
                int roleId = GetRoleId("Patient");
                if (roleId == 0)
                {
                    lblError.Text = "Patient role not found in the database.";
                    return;
                }

                // Step 2: Insert into Users table
                string insertUserQuery = @"
                    INSERT INTO Users (FullName, Email, Phone, PasswordHash, RoleId)
                    OUTPUT INSERTED.UserId
                    VALUES (@FullName, @Email, @Phone, @PasswordHash, @RoleId);
                ";

                using (SqlCommand cmd = new SqlCommand(insertUserQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@RoleId", roleId);
                    userId = (int)cmd.ExecuteScalar();
                }

                // Step 3: Insert into Patients table
                string insertPatientQuery = @"
                    INSERT INTO Patients (UserId, DateOfBirth)
                    VALUES (@UserId, @DateOfBirth);
                ";

                using (SqlCommand cmd = new SqlCommand(insertPatientQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@DateOfBirth", Convert.ToDateTime(dob));
                    cmd.ExecuteNonQuery();
                }

                // Step 4: Store in session
                Session["UserIdVal"] = userId.ToString();
                Session["UserNameVal"] = fullName;
                Session["NameVal"] = fullName;
                Session["UserRoleVal"] = "Patient";

                // Step 5: Redirect to Patient Dashboard
                Response.Redirect("PatientDashboard.aspx");
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
