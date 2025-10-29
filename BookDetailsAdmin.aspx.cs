using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class BookDetailsAdmin : System.Web.UI.Page
    {
        private SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Allow Admin and Publisher roles
            if (Session["UserRoleVal"] == null ||
                (Session["UserRoleVal"].ToString() != "Admin" &&
                 Session["UserRoleVal"].ToString() != "Publisher"))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string id = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(id))
                {
                    LoadBookDetails(int.Parse(id));
                }
            }
        }

        private void open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }

        private void close_conn()
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
                Conn.Close();
        }

        private void LoadBookDetails(int bookId)
        {
            open_conn();

            // Query to fetch book details
            string query = @"
                SELECT 
                    B.Id, 
                    B.Name, 
                    B.Description, 
                    B.Price, 
                    B.NumOfPages, 
                    B.PublishDate,
                    B.ImageUrl,
                    B.UserId,
                    B.IsAdminCreated,
                    P.Name AS PublisherName, 
                    C.Name AS CategoryName,
                    BU.StatusId AS LastUpdateStatusId
                FROM Books B
                INNER JOIN Users P ON B.UserId = P.Id
                INNER JOIN Categories C ON B.CategoryId = C.Id
                LEFT JOIN (
                    SELECT 
                        BookId, 
                        StatusId, 
                        ROW_NUMBER() OVER (PARTITION BY BookId ORDER BY CreatedAt DESC) AS RowNum
                    FROM BookUpdates
                ) BU ON B.Id = BU.BookId AND BU.RowNum = 1
                WHERE B.Id = @BookId";

            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Populate book details
                        book_Image.Src = reader["ImageUrl"]?.ToString() ?? "";
                        hlName.Text = reader["Name"].ToString();
                        hlName.NavigateUrl = $"BookDetailsAdmin.aspx?id={bookId}";
                        lblDescription.Text = reader["Description"].ToString();
                        lblPrice.Text = Convert.ToDecimal(reader["Price"]).ToString("C");
                        lblPages.Text = reader["NumOfPages"].ToString();
                        lblPublishDate.Text = Convert.ToDateTime(reader["PublishDate"]).ToString("yyyy-MM-dd");
                        lblPublisher.Text = reader["PublisherName"].ToString();
                        lblCategory.Text = reader["CategoryName"].ToString();

                        // Display update status
                        int? lastUpdateStatusId = reader["LastUpdateStatusId"] as int?;

                       

                        // Check if the book was added by an Admin or Publisher
                        bool isAdminCreated = Convert.ToBoolean(reader["IsAdminCreated"]);
                        if (!isAdminCreated)
                        {
                            LoadPublisherBooks(reader["UserId"].ToString());
                        }
                        // After LoadUpdateDetails(...)
                        LoadBookRequests(bookId);
                        LoadPendingUpdates(bookId);
                    }
                }
                LoadAvailableCopies(bookId);
                // Ensure the reader is closed before executing other commands

            }

            close_conn();
        }
        private void LoadAvailableCopies(int bookId)
        {
            open_conn();
            try
            {
                string query = @"
            SELECT IsForBorrow, AvailableCopies 
            FROM BookCopies 
            WHERE BookId = @BookId";
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int borrow = 0, sale = 0;
                        while (reader.Read())
                        {
                            if (Convert.ToBoolean(reader["IsForBorrow"]))
                                borrow = Convert.ToInt32(reader["AvailableCopies"]);
                            else
                                sale = Convert.ToInt32(reader["AvailableCopies"]);
                        }
                        lblBorrowCopies.Text = borrow.ToString();
                        lblSaleCopies.Text = sale.ToString();
                    }
                }
            }
            finally { close_conn(); }
        }

        protected string GetUpdateStatusLabel(object statusId)
        {
            int id = Convert.ToInt32(statusId);
            switch (id)
            {
                case 1: return "<span class='badge bg-warning text-dark'>Pending</span>";
                case 2: return "<span class='badge bg-success'>Approved</span>";
                case 3: return "<span class='badge bg-danger'>Rejected</span>";
                default: return "<span class='badge bg-secondary'>Unknown</span>";
            }
        }
        protected void gvBookRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Get the data item
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                string status = rowView["Status"]?.ToString();
                string userRole = Session["UserRoleVal"]?.ToString();

                // Find the buttons
                LinkButton lnkApprove = (LinkButton)e.Row.FindControl("lnkApprove");
                LinkButton lnkReject = (LinkButton)e.Row.FindControl("lnkReject");

                // Hide buttons for non-Admins
                if (userRole != "Admin")
                {
                    lnkApprove.Visible = false;
                    lnkReject.Visible = false;
                }
                else
                {
                    // Hide buttons if status is not pending
                    bool isPending = string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase);
                    lnkApprove.Visible = isPending;
                    lnkReject.Visible = isPending;
                }
            }
        }
        private void LoadBookRequests(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(SiteMaster.oraAuth()))
            {
                conn.Open();
                string query = @"
            SELECT 
                BR.Id,
                U.Name AS UserName,
                BR.RequestType,
                BR.RequestDate,
                BR.Status,
                BR.ApprovedAt,
                A.Name AS AdminName
            FROM BookRequests BR
            INNER JOIN Users U ON BR.UserId = U.Id
            LEFT JOIN Users A ON BR.ApprovedBy = A.Id
            WHERE BR.BookCopyId IN (SELECT Id FROM BookCopies WHERE BookId = @BookId)
            ORDER BY BR.RequestDate DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@BookId", bookId);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvBookRequests.DataSource = dt;
                gvBookRequests.DataBind();
            }
        }
        private void LoadPendingUpdates(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(SiteMaster.oraAuth()))
            {
                conn.Open();
                string query = @"
            SELECT 
                Id,
                UpdatedName,
                UpdatedCategoryId,
                UpdatedDescription,
                UpdatedPrice,
                UpdatedNumOfPages,
                UpdatedPublishDate,
                StatusId,
                CreatedAt,
                ReviewedBy,
                ReviewedAt,
                ReviewComment
            FROM BookUpdates
            WHERE BookId = @BookId 
              AND StatusId = 1 -- Only show pending updates
            ORDER BY CreatedAt DESC";

                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@BookId", bookId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvPendingUpdates.DataSource = dt;
                    gvPendingUpdates.DataBind();
                }
            }
        }
        private void ApproveUpdate(int updateId, int bookId)
        {
            open_conn();
            try
            {
                // Fetch update details
                string selectQuery = @"
            SELECT 
                UpdatedName, 
                UpdatedCategoryId, 
                UpdatedDescription, 
                UpdatedNumOfPages, 
                UpdatedPrice, 
                UpdatedPublishDate
            FROM BookUpdates
            WHERE Id = @UpdateId";
                using (SqlCommand cmd = new SqlCommand(selectQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@UpdateId", updateId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Update main book record
                            string updateBookQuery = @"
                        UPDATE Books
                        SET 
                            Name = @Name,
                            CategoryId = @CategoryId,
                            Description = @Description,
                            Price = @Price,
                            NumOfPages = @NumOfPages,
                            PublishDate = @PublishDate
                        WHERE Id = @BookId";
                            using (SqlCommand bookCmd = new SqlCommand(updateBookQuery, Conn))
                            {
                                bookCmd.Parameters.AddWithValue("@Name", reader["UpdatedName"]);
                                bookCmd.Parameters.AddWithValue("@CategoryId", reader["UpdatedCategoryId"]);
                                bookCmd.Parameters.AddWithValue("@Description", reader["UpdatedDescription"]);
                                bookCmd.Parameters.AddWithValue("@Price", reader["UpdatedPrice"]);
                                bookCmd.Parameters.AddWithValue("@NumOfPages", reader["UpdatedNumOfPages"]);
                                bookCmd.Parameters.AddWithValue("@PublishDate", reader["UpdatedPublishDate"]);
                                bookCmd.Parameters.AddWithValue("@BookId", bookId);
                                bookCmd.ExecuteNonQuery();
                            }

                            // Update BookUpdates status to approved
                            string updateStatusQuery = @"
                        UPDATE BookUpdates
                        SET StatusId = 2,
                            ReviewedBy = @AdminId,
                            ReviewedAt = GETDATE(),
                            ReviewComment = 'Approved'
                        WHERE Id = @UpdateId";
                            using (SqlCommand statusCmd = new SqlCommand(updateStatusQuery, Conn))
                            {
                                statusCmd.Parameters.AddWithValue("@AdminId", Session["UserIdVal"]);
                                statusCmd.Parameters.AddWithValue("@UpdateId", updateId);
                                statusCmd.ExecuteNonQuery();
                            }
                            ShowAlert("Update approved successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error approving update: {ex.Message}");
            }
            finally
            {
                close_conn();
            }
        }

        private void RejectUpdate(int updateId, int bookId)
        {
            open_conn();
            try
            {
                // Update status to rejected
                string query = @"
            UPDATE BookUpdates
            SET StatusId = 3,
                ReviewedBy = @AdminId,
                ReviewedAt = GETDATE(),
                ReviewComment = 'Rejected'
            WHERE Id = @UpdateId";
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddWithValue("@AdminId", Session["UserIdVal"]);
                    cmd.Parameters.AddWithValue("@UpdateId", updateId);
                    cmd.ExecuteNonQuery();
                }
                ShowAlert("Update rejected successfully.");
            }
            catch (Exception ex)
            {
                ShowAlert($"Error rejecting update: {ex.Message}");
            }
            finally
            {
                close_conn();
            }
        }
        protected void gvPendingUpdates_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Hide buttons for non-admin users
                if (Session["UserRoleVal"].ToString() != "Admin")
                {
                    ((LinkButton)e.Row.FindControl("lnkApproveUpdate")).Visible = false;
                    ((LinkButton)e.Row.FindControl("lnkRejectUpdate")).Visible = false;
                    return;
                }

                // Hide buttons if status is not pending
                int statusId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "StatusId"));
                bool isPending = statusId == 1;
                ((LinkButton)e.Row.FindControl("lnkApproveUpdate")).Visible = isPending;
                ((LinkButton)e.Row.FindControl("lnkRejectUpdate")).Visible = isPending;
            }
        }
        protected void gvPendingUpdates_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ApproveUpdate" || e.CommandName == "RejectUpdate")
            {
                int updateId = Convert.ToInt32(e.CommandArgument);
                int bookId = Convert.ToInt32(Request.QueryString["id"]);

                if (e.CommandName == "ApproveUpdate")
                {
                    ApproveUpdate(updateId, bookId);
                }
                else
                {
                    RejectUpdate(updateId, bookId);
                }

                // Reload data after processing
                LoadBookDetails(bookId);
            }
        }
        protected void gvBookRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int requestId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Approve")
            {
                UpdateBookRequestStatus(requestId, "Approved");
            }
            else if (e.CommandName == "Reject")
            {
                UpdateBookRequestStatus(requestId, "Rejected");
            }

            // Reload the current book details
            int bookId = Convert.ToInt32(Request.QueryString["id"]);
            LoadBookDetails(bookId);
        }
        private void UpdateBookRequestStatus(int requestId, string status)
        {
            int bookId = Convert.ToInt32(Request.QueryString["id"]);

            open_conn();
            try
            {
                int adminId = Convert.ToInt32(Session["UserIdVal"]);

                // Update request status
                string updateQuery = $@"
            UPDATE BookRequests
            SET Status = @Status,
                ApprovedAt = CASE WHEN @Status = 'Approved' THEN GETDATE() ELSE NULL END,
                ApprovedBy = CASE WHEN @Status = 'Approved' THEN @AdminId ELSE NULL END
            WHERE Id = @RequestId";

                using (SqlCommand cmd = new SqlCommand(updateQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@AdminId", adminId);
                    cmd.ExecuteNonQuery();
                }

                if (status == "Approved" || status == "Rejected")
                {
                    string getRequestInfoQuery = @"
                SELECT BR.RequestType, BC.Id AS BookCopyId, BC.IsForBorrow, BR.UserId
                FROM BookRequests BR
                INNER JOIN BookCopies BC ON BR.BookCopyId = BC.Id
                WHERE BR.Id = @RequestId";

                    using (SqlCommand typeCmd = new SqlCommand(getRequestInfoQuery, Conn))
                    {
                        typeCmd.Parameters.AddWithValue("@RequestId", requestId);
                        using (SqlDataReader reader = typeCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string requestType = reader["RequestType"].ToString();
                                int bookCopyId = Convert.ToInt32(reader["BookCopyId"]);
                                //bool isForBorrow = Convert.ToBoolean(reader["IsForBorrow"]);
                                int userId = Convert.ToInt32(reader["UserId"]);
                                reader.Close();

                                switch (requestType)
                                {
                                    case "Buy":
                                        
                                            UpdateAvailableCopies(bookId, -1, isForBorrow: 0);
                                        
                                            //ShowAlert("Cannot buy a borrowable copy.");
                                        break;

                                    case "Borrow":
                                        //if (isForBorrow == true)
                                            UpdateAvailableCopies(bookId, -1, isForBorrow: 1);
                                        //else
                                            //ShowAlert("Cannot borrow a saleable copy.");
                                        break;

                                    case "Return":
                                        //if (isForBorrow == true)
                                        //{
                                        if (status == "Approved")
                                        {
                                            UpdateLatestBorrowAsReturned(userId, bookCopyId);

                                            UpdateAvailableCopies(bookId, +1, isForBorrow: 1);
                                        }
                                        else
                                            ShowAlert("Return request was rejected. No changes made.");
                                        //}
                                        //else
                                        //{
                                        //    ShowAlert("Cannot return a saleable copy.");
                                        //}
                                        break;
                                }
                            }
                        }
                    }
                }

                ShowAlert($"Request {status} successfully.");
            }
            catch (Exception ex)
            {
                ShowAlert("Error updating request: " + ex.Message);
            }
            finally
            {
                close_conn();
            }
        }
        private void UpdateLatestBorrowAsReturned(int userId, int bookCopyId)
        {
            string updateQuery = @"
                UPDATE BookRequests
                SET Status = 'Returned'
                WHERE Id = (
                    SELECT TOP 1 Id 
                    FROM BookRequests 
                    WHERE UserId = @UserId AND BookCopyId = @BookCopyId 
                      AND RequestType = 'Borrow' AND Status = 'Approved'
                    ORDER BY RequestDate DESC
                )";

            using (SqlCommand cmd = new SqlCommand(updateQuery, Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookCopyId", bookCopyId);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateAvailableCopies(int bookId, int changeAmount, int isForBorrow)
        {
            open_conn();
            try
            {
                // Validate the book copy exists and matches IsForBorrow flag
                string validateQuery = "SELECT 1 FROM BookCopies WHERE bookId = @bookId AND IsForBorrow = @isForBorrow";
                using (SqlCommand validateCmd = new SqlCommand(validateQuery, Conn))
                {
                    validateCmd.Parameters.AddWithValue("@bookId", bookId);
                    validateCmd.Parameters.AddWithValue("@isForBorrow", isForBorrow); // ✅ Fix: add missing parameter

                    var result = validateCmd.ExecuteScalar();
                    if (result == null)
                    {
                        ShowAlert("Book copy not found or does not match borrow condition.");
                        return;
                    }
                }

                // Update available copies
                string updateQuery = @"
            UPDATE BookCopies
            SET AvailableCopies = AvailableCopies + @ChangeAmount,
                UpdatedAt = GETDATE()
            WHERE bookId = @bookId AND IsForBorrow = @isForBorrow";

                using (SqlCommand cmd = new SqlCommand(updateQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@ChangeAmount", changeAmount);
                    cmd.Parameters.AddWithValue("@bookId", bookId);
                    cmd.Parameters.AddWithValue("@isForBorrow", isForBorrow);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error updating available copies: " + ex.Message);
            }
            finally
            {
                close_conn();
            }
        }


        private void LoadPublisherBooks(string publisherId)
        {
            // Use a separate connection for this method to avoid conflicts
            using (SqlConnection conn = new SqlConnection(SiteMaster.oraAuth()))
            {
                conn.Open();

                string query = @"
                SELECT 
                    B.Id, 
                    B.Name, 
                    B.Description, 
                    B.Price, 
                    B.NumOfPages, 
                    B.PublishDate
                FROM Books B
                WHERE B.UserId = @PublisherId AND B.StatusId = 2"; // Only approved books

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@PublisherId", publisherId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPublisherBooks.DataSource = dt;
                gvPublisherBooks.DataBind();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("BookListAdmin.aspx");
        }



        protected string GetRequestStatusLabel(object statusObj)
        {
            string status = statusObj?.ToString().ToLower() ?? "pending";
            switch (status)
            {
                case "approved":
                    return "<span class='badge bg-success'>Approved</span>";
                case "rejected":
                    return "<span class='badge bg-danger'>Rejected</span>";
                case "returned": // 👈 Add this case
                    return "<span class='badge bg-info'>Returned</span>";
                default:
                    return "<span class='badge bg-warning text-dark'>Pending</span>";
            }
        }
        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'"); // Escape single quotes
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertMessage", $"alert('{cleanMessage}');", true);
        }
    }
}