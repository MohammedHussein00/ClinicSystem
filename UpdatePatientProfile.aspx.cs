using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace LibrarySystem
{
    public partial class UpdatePatientProfile : System.Web.UI.Page
    {
        SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserIdVal"] == null)
                Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
                LoadUserData();
        }

        private void LoadUserData()
        {
            int userId = Convert.ToInt32(Session["UserIdVal"]);
            open_conn();

            string query = @"
                SELECT u.FullName, u.Phone, u.ImageUrl, p.DateOfBirth
                FROM Users u
                LEFT JOIN Patients p ON u.UserId = p.UserId
                WHERE u.UserId = @UserId";

            SqlCommand cmd = new SqlCommand(query, Conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                txtName.Text = reader["FullName"].ToString();
                txtPhone.Text = reader["Phone"].ToString();

                if (reader["DateOfBirth"] != DBNull.Value)
                    txtDOB.Text = Convert.ToDateTime(reader["DateOfBirth"]).ToString("yyyy-MM-dd");

                if (reader["ImageUrl"] != DBNull.Value)
                {
                    string imageUrl = reader["ImageUrl"].ToString();
                    imgProfile.ImageUrl = imageUrl;
                    imgProfile.Visible = true;

                    imgPreview.ImageUrl = imageUrl;
                    imgPreview.Visible = true;
                }
            }

            reader.Close();
            close_conn();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserIdVal"]);
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            DateTime? dateOfBirth = null;

            if (!string.IsNullOrEmpty(txtDOB.Text) && DateTime.TryParse(txtDOB.Text, out DateTime parsedDate))
                dateOfBirth = parsedDate;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                lblMessage.Text = "❌ Name and phone are required!";
                lblMessage.CssClass = "text-danger";
                return;
            }

            open_conn();

            string imageUrl = null;
            string oldImageUrl = GetCurrentUserImageUrl(userId);

            if (fuProfileImage.HasFile)
            {
                try
                {
                    string fileName = Path.GetFileName(fuProfileImage.FileName);
                    string folderPath = Server.MapPath($"~/Content/Images/Profiles/{userId}/");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string fullPath = Path.Combine(folderPath, fileName);
                    fuProfileImage.SaveAs(fullPath);

                    imageUrl = $"Content/Images/Profiles/{userId}/{fileName}";

                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        string oldImagePath = Server.MapPath(oldImageUrl);
                        if (File.Exists(oldImagePath))
                            File.Delete(oldImagePath);
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Image upload failed: " + ex.Message;
                    lblMessage.CssClass = "text-danger";
                    close_conn();
                    return;
                }
            }

            // ✅ Update Users Table
            StringBuilder sqlUser = new StringBuilder("UPDATE Users SET FullName = @Name, Phone = @Phone");
            if (imageUrl != null)
                sqlUser.Append(", ImageUrl = @ImageUrl");
            sqlUser.Append(" WHERE UserId = @UserId");

            SqlCommand cmdUser = new SqlCommand(sqlUser.ToString(), Conn);
            cmdUser.Parameters.AddWithValue("@Name", name);
            cmdUser.Parameters.AddWithValue("@Phone", phone);
            cmdUser.Parameters.AddWithValue("@UserId", userId);
            if (imageUrl != null)
                cmdUser.Parameters.AddWithValue("@ImageUrl", imageUrl);
            cmdUser.ExecuteNonQuery();

            // ✅ Update or Insert Patient DateOfBirth
            string checkPatient = "SELECT COUNT(*) FROM Patients WHERE UserId = @UserId";
            SqlCommand checkCmd = new SqlCommand(checkPatient, Conn);
            checkCmd.Parameters.AddWithValue("@UserId", userId);
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists > 0)
            {
                string updatePatient = "UPDATE Patients SET DateOfBirth = @DateOfBirth WHERE UserId = @UserId";
                SqlCommand updateCmd = new SqlCommand(updatePatient, Conn);
                updateCmd.Parameters.AddWithValue("@UserId", userId);
                updateCmd.Parameters.AddWithValue("@DateOfBirth", (object)dateOfBirth ?? DBNull.Value);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                string insertPatient = "INSERT INTO Patients (UserId, DateOfBirth) VALUES (@UserId, @DateOfBirth)";
                SqlCommand insertCmd = new SqlCommand(insertPatient, Conn);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                insertCmd.Parameters.AddWithValue("@DateOfBirth", (object)dateOfBirth ?? DBNull.Value);
                insertCmd.ExecuteNonQuery();
            }

            close_conn();

            lblMessage.Text = "✅ Profile updated successfully!";
            lblMessage.CssClass = "text-success";

            LoadUserData(); // Refresh UI
        }

        // ✅ Change Password Function
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserIdVal"]);
            string oldPassword = txtOldPassword.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                lblMessage.Text = "❌ Please enter both old and new passwords.";
                lblMessage.CssClass = "text-danger";
                return;
            }

            if (newPassword.Length < 6)
            {
                lblMessage.Text = "❌ The new password must be at least 6 characters long.";
                lblMessage.CssClass = "text-danger";
                return;
            }

            open_conn();

            string query = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
            SqlCommand cmd = new SqlCommand(query, Conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            object result = cmd.ExecuteScalar();

            if (result != null)
            {
                string currentHashed = result.ToString();
                string oldHashed = ComputeSha256Hash(oldPassword);

                if (currentHashed != oldHashed)
                {
                    lblMessage.Text = "❌ Incorrect old password.";
                    lblMessage.CssClass = "text-danger";
                    close_conn();
                    return;
                }

                string newHashed = ComputeSha256Hash(newPassword);
                string updateQuery = "UPDATE Users SET PasswordHash = @NewPassword WHERE UserId = @UserId";
                SqlCommand updateCmd = new SqlCommand(updateQuery, Conn);
                updateCmd.Parameters.AddWithValue("@NewPassword", newHashed);
                updateCmd.Parameters.AddWithValue("@UserId", userId);
                updateCmd.ExecuteNonQuery();

                lblMessage.Text = "✅ Password changed successfully!";
                lblMessage.CssClass = "text-success";
            }

            close_conn();
        }

        private string GetCurrentUserImageUrl(int userId)
        {
            string query = "SELECT ImageUrl FROM Users WHERE UserId = @UserId";
            SqlCommand cmd = new SqlCommand(query, Conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            object result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : null;
        }

        protected void open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }

        protected void close_conn()
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
                Conn.Close();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
