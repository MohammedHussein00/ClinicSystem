using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class AdminContacts : System.Web.UI.Page
    {
        SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserRoleVal"] == null || Session["UserRoleVal"].ToString() != "Admin")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
            if (!IsPostBack)
            {
                LoadContactMessages();
            }
        }

        private void open_conn()
        {
            string connectionString = SiteMaster.oraAuth(); // Make sure this returns a valid SQL connection string
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }

        private void close_conn()
        {
            if (Conn != null && Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
                Conn.Dispose();
            }
        }

        private void LoadContactMessages()
        {
            open_conn();
            try
            {
                string query = @"
                    SELECT Id, FirstName, LastName, Email, PhoneNumber, Message, SubmittedAt 
                    FROM ContactMessages 
                    ORDER BY SubmittedAt DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, Conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvContacts.DataSource = dt;
                gvContacts.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading messages: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }
        }

        protected void gvContacts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                int messageId = Convert.ToInt32(e.CommandArgument);
                DeleteContactMessage(messageId);
            }
        }

        private void DeleteContactMessage(int messageId)
        {
            open_conn();
            try
            {
                string query = "DELETE FROM ContactMessages WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, Conn);
                cmd.Parameters.AddWithValue("@Id", messageId);
                cmd.ExecuteNonQuery();

                lblMessage.Text = "Message deleted successfully.";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                LoadContactMessages(); // Refresh grid
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error deleting message: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }
        }
    }
}