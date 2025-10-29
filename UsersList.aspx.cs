using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using WebGrease.Activities;

namespace LibrarySystem
{
    public partial class UsersList : System.Web.UI.Page
    {
        SqlConnection Conn; // Changed from OleDbConnection to SqlConnection

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUsers();
            }
        }

        protected SqlConnection open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        private void LoadUsers(string filter = "")
        {
            string query = @"
                SELECT u.Id, u.Name, u.Phone, u.Email, u.ImageUrl 
                FROM Users u
                INNER JOIN UserRoles ur ON u.Id = ur.UserId
                WHERE 1=1 " + filter;

            using (SqlConnection conn = open_conn())
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void btnShowUsers_Click(object sender, EventArgs e)
        {
            LoadUsers("AND ur.RoleId = 2");
        }

        protected void btnShowPublishers_Click(object sender, EventArgs e)
        {
            LoadUsers("AND ur.RoleId = 3");
        }

        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            bool isFormVisible = formContainer.Style["display"] != "none";

            if (!isFormVisible)
            {
                formContainer.Style["display"] = "block";
                tableContainer.Attributes["class"] = "table-container half-width";
                btnAddNew.Text = "Hide Form";
            }
            else
            {
                formContainer.Style["display"] = "none";
                tableContainer.Attributes["class"] = "table-container";
                btnAddNew.Text = "Add New User/Publisher";
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
        protected void close_conn()
        {
            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
                Conn.Dispose();
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string passwordHash = ComputeSha256Hash(txtPassword.Text.Trim());
            int roleId = Convert.ToInt32(ddlRole.SelectedValue);
            if (IsEmailRegistered(email))
            {
                string script = "alert('This email is already registered.');";
                ClientScript.RegisterStartupScript(this.GetType(), "EmailExistsAlert", script, true);
                return;
            }

            string query = @"
                INSERT INTO Users (Name, Phone, Email, PasswordHash, ImageUrl)
                VALUES (@Name, @Phone, @Email, @PasswordHash, @ImageUrl);
                DECLARE @UserId INT = SCOPE_IDENTITY();
                INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);";

            using (SqlConnection conn = open_conn())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@ImageUrl", "");
                cmd.Parameters.AddWithValue("@RoleId", roleId);

                cmd.ExecuteNonQuery();
            }

            LoadUsers();
            btnCancel_Click(sender, e);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            formContainer.Style["display"] = "none";
            tableContainer.Attributes["class"] = "table-container";
            ClearForm();
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtPassword.Text = "";
            ddlRole.SelectedIndex = 0;
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
