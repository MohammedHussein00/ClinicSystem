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
    public partial class Contact : Page
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
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhoneNumber.Text.Trim();
            string message = txtQuery.Text.Trim();

            // Basic validation
            if (string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(message))
            {
                lblError.Text = "Please fill all required fields.";
                return;
            }

            open_conn();
            try
            {
                string insertQuery = @"
                    INSERT INTO ContactMessages (
                        FirstName, LastName, Email, PhoneNumber, Message
                    ) VALUES (
                        @FirstName, @LastName, @Email, @PhoneNumber, @Message
                    )";

                exec_sql(insertQuery,
                    new SqlParameter("@FirstName", firstName),
                    new SqlParameter("@LastName", lastName),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@PhoneNumber", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                    new SqlParameter("@Message", message));

                ResetForm();
                ShowAlert("Your message has been sent successfully!");
            }
            catch (Exception ex)
            {
                lblError.Text = "An error occurred while submitting your message: " + ex.Message;
            }
            finally
            {
                close_conn();
            }
        }
        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'"); // Escape quotes
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "alertMessage",
                $"alert('{cleanMessage}');",
                true);
        }
        protected void exec_sql(string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    lblError.Text = "SQL Execution Error: " + ex.Message;
                }
            }
        }
        private void ResetForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPhoneNumber.Text = "";
            txtQuery.Text = "";
            lblError.Text = "";
        }


    }
}