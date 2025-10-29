using System;
using System.Data.SqlClient; // Switch from OleDb to SqlConnection
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;

namespace LibrarySystem
{
    public partial class Login : Page
    {
        SqlConnection Conn; // Changed from OleDbConnection to SqlConnection
        String Sqlst;
        TableRow tr;
        CultureInfo newCulture;
        String LoggedUserID;
        String CurrDate;
        String CurrDateSht;

        protected void Page_Load(object sender, EventArgs e)
        {
            var x = Session["UserIdVal"]?.ToString();

            CurrDate = " to_date('" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/mm/yyyy hh24:mi:ss')";
            CurrDateSht = " to_date('" + DateTime.Now.ToString("dd/MM/yyyy") + "', 'dd/mm/yyyy')";
            newCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            newCulture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy HH24:mm:ss";
            newCulture.DateTimeFormat.DateSeparator = "/";
            Thread.CurrentThread.CurrentCulture = newCulture;
        }

        protected void open_conn()
        {
            // Call the static oraAuth() method from SiteMaster
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
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
                    //lblError.Text = "SQL Execution Error: " + ex.Message;
                }
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

        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        protected void MessageBox(String msg)
        {
            System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
            sb2.Clear();
            sb2.Append(@"<script language='javascript'>");
            sb2.Append(@"alert('" + msg + "');");
            sb2.Append(@"</script>");
            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), sb2.ToString(), false);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            open_conn();

            try
            {
                // Hash the entered password
                string hashedPassword = sha256_hash(txtPassword.Text.Trim());

                string Sqlst = @"
            SELECT 
                u.UserId,
                u.FullName AS UserName,
                r.RoleName AS Role_Name,
                u.ImageUrl
            FROM Users u
            INNER JOIN Roles r ON u.RoleId = r.RoleId
            WHERE 
                (u.Email = @Email OR u.Phone = @Phone)
                AND (u.PasswordHash = @PasswordHash)";

                using (SqlCommand cmd = new SqlCommand(Sqlst, Conn))
                {
                    // Prevent SQL injection
                    cmd.Parameters.AddWithValue("@Email", txtuserId.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", txtuserId.Text.Trim());
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // ✅ Login Success
                            Session["UserNameVal"] = reader["UserName"].ToString();
                            Session["UserIdVal"] = reader["UserId"].ToString();
                            Session["UserRoleVal"] = reader["Role_Name"].ToString();
                            Session["ImageUrlVal"] = reader["ImageUrl"]?.ToString();

                            lblError.Text = ""; // clear error message
                            Response.Redirect("Default.aspx");
                        }
                        else
                        {
                            // ❌ Invalid login
                            lblError.Text = "❌ Incorrect email or password.";
                            lblError.ForeColor = System.Drawing.Color.Red;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "An error occurred: " + ex.Message;
                lblError.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }
        }
    }
}