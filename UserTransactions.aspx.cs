using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class UserTransactions : System.Web.UI.Page
    {
        private string connectionString = SiteMaster.oraAuth();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserIdVal"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadBookRequests();
            }
        }

        private void LoadBookRequests()
        {
            int userId = Convert.ToInt32(Session["UserIdVal"]);
            string query = @"
                SELECT 
                    BR.Id,
                    BR.RequestType,
                    BR.RequestDate,
                    BR.Status,
                    BR.ReturnDate,
                    BR.AdminComment,
                    B.Id AS BookId,
                    B.Name AS BookName,
                    B.Description,
                    B.ImageUrl
                FROM BookRequests BR
                JOIN BookCopies BC ON BR.BookCopyId = BC.Id
                JOIN Books B ON BC.BookId = B.Id
                WHERE BR.UserId = @UserId
                ORDER BY BR.RequestDate DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    lblNoRequests.Visible = true;
                    gvRequests.Visible = false;
                }
                else
                {
                    gvRequests.DataSource = dt;
                    gvRequests.DataBind();
                }
            }
        }

        protected void gvRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                var statusLabel = (System.Web.UI.HtmlControls.HtmlGenericControl)e.Row.FindControl("lblStatus");

                string requestType = row["RequestType"].ToString();
                string status = row["Status"].ToString();
                DateTime? returnDate = row["ReturnDate"] as DateTime?;

                switch (requestType)
                {
                    case "Borrow":
                        if (status == "Approved" && !returnDate.HasValue)
                            statusLabel.InnerText = "Currently Borrowed";
                        else if (status == "Approved" && returnDate.HasValue)
                            statusLabel.InnerText = "Returned";
                        else
                            statusLabel.InnerText = status;
                        break;

                    case "Buy":
                        if (status == "Approved")
                            statusLabel.InnerText = "Purchased";
                        else
                            statusLabel.InnerText = status;
                        break;

                    case "Return":
                        statusLabel.InnerText = status;
                        break;

                    default:
                        statusLabel.InnerText = status;
                        break;
                }

                // Apply Bootstrap classes
                switch (statusLabel.InnerText)
                {
                    case "Approved":
                    case "Purchased":
                    case "Returned":
                        statusLabel.Attributes["class"] = "text-success";
                        break;
                    case "Pending":
                        statusLabel.Attributes["class"] = "text-warning";
                        break;
                    case "Rejected":
                        statusLabel.Attributes["class"] = "text-danger";
                        break;
                    default:
                        statusLabel.Attributes["class"] = "text-info";
                        break;
                }
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("BookListUser.aspx");
        }
    }
}