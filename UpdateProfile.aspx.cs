using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace LibrarySystem
{
    public partial class UpdateProfile : System.Web.UI.Page
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

            string query = "SELECT FullName, Phone, ImageUrl FROM Users WHERE UserId = @UserId";
            SqlCommand cmd = new SqlCommand(query, Conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                txtName.Text = reader["FullName"].ToString();
                txtPhone.Text = reader["Phone"].ToString();

                if (reader["ImageUrl"] != DBNull.Value)
                {
                    string imageUrl = reader["ImageUrl"].ToString();
                    imgProfile.ImageUrl = imageUrl;
                    imgProfile.Visible = true;

                    imgPreview.ImageUrl = imageUrl;       // ✅ Correct property
                    imgPreview.Visible = true;            // ✅ Use this to show the image
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
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    close_conn();
                    return;
                }
            }

            StringBuilder sql = new StringBuilder("UPDATE Users SET FullName = @Name, Phone = @Phone");
            if (imageUrl != null)
                sql.Append(", ImageUrl = @ImageUrl");

            sql.Append(" WHERE UserId = @UserId");

            SqlCommand cmd = new SqlCommand(sql.ToString(), Conn);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Phone", phone);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (imageUrl != null)
                cmd.Parameters.AddWithValue("@ImageUrl", imageUrl);
            Session["ImageUrlVal"] = imageUrl;
            cmd.ExecuteNonQuery();
            close_conn();

            lblMessage.Text = "✅ Profile updated successfully!";
            lblMessage.CssClass = "text-success";

            LoadUserData(); // Refresh UI with updated image
        }

        private string GetCurrentUserImageUrl(int userId)
        {
            string query = "SELECT ImageUrl FROM Users WHERE UserId = @UserId";
            SqlCommand cmd = new SqlCommand(query, Conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            object result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : null;
        }

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

            string query = "SELECT Password FROM Users WHERE Id = @UserId";
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
                string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE Id = @UserId";
                SqlCommand updateCmd = new SqlCommand(updateQuery, Conn);
                updateCmd.Parameters.AddWithValue("@NewPassword", newHashed);
                updateCmd.Parameters.AddWithValue("@UserId", userId);
                updateCmd.ExecuteNonQuery();

                lblMessage.Text = "✅ Password changed successfully!";
                lblMessage.CssClass = "text-success";
            }

            close_conn();
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
