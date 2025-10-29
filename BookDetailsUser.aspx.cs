using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace LibrarySystem
{
    public partial class BookDetailsUser : Page
    {
        private SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string id = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(id) && int.TryParse(id, out int bookId))
                {
                    LoadBookDetails(bookId);
                    int userId = Convert.ToInt32(Session["UserIdVal"]);

                    LoadTransactionHistory(bookId, userId); // <-- Add this
                    CheckUserRequests(bookId); // 👈 Force check again after any request


                }
                else
                {
                    ShowAlert("Invalid Book ID.");
                    Response.Redirect("BookListUser.aspx");
                }
            }
        }

        private void OpenConnection()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }

        private void CloseConnection()
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
                Conn.Close();
        }

        private void LoadBookDetails(int bookId)
        {
            try
            {
                OpenConnection();

                // Get book details including actual available copies
                string query = @"
                    SELECT 
                        B.Id AS BookId,
                        B.Name AS BookName,
                        B.Description,
                        B.Price,
                        B.NumOfPages,
                        B.PublishDate,
                        B.ImageUrl,
                        U.Name AS PublisherName,
                        C.Name AS CategoryName,
                        ISNULL(BC.AvailableCopies, 0) AS TotalAvailableCopies,
                        ISNULL((
                            SELECT COUNT(*) FROM BookRequests BR 
                            WHERE BR.RequestType = 'Borrow' AND BR.Status IN ('Approved') AND BR.ReturnDate IS NULL AND BR.BookCopyId = B.Id
                        ), 0) AS ActiveBorrows
                    FROM Books B
                    INNER JOIN Users U ON B.UserId = U.Id
                    INNER JOIN Categories C ON B.CategoryId = C.Id
                    LEFT JOIN BookCopies BC ON B.Id = BC.BookId
                    WHERE B.Id = @BookId";

                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int totalAvailable = Convert.ToInt32(reader["TotalAvailableCopies"]);
                            int activeBorrows = Convert.ToInt32(reader["ActiveBorrows"]);
                            int actualAvailable = totalAvailable - activeBorrows;

                            book_Image.Src = reader["ImageUrl"]?.ToString() ?? "";
                            hlName.Text = reader["BookName"].ToString();
                            lblDescription.Text = reader["Description"].ToString();
                            lblPrice.Text = Convert.ToDecimal(reader["Price"]).ToString("C");
                            lblPages.Text = reader["NumOfPages"].ToString();
                            lblPublishDate.Text = Convert.ToDateTime(reader["PublishDate"]).ToString("yyyy-MM-dd");
                            lblPublisher.Text = reader["PublisherName"].ToString();
                            lblCategory.Text = reader["CategoryName"].ToString();
                            lblAvailableCopies.Text = actualAvailable.ToString(); // Show actual available
                        }
                    }
                }

                CheckUserRequests(bookId); // Set button visibility and alerts
                LoadPublisherBooks(bookId); // Load other books by same publisher

            }
            catch (Exception ex)
            {
                ShowAlert($"Error loading book details: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }

        private void CheckUserRequests(int bookId)
        {
            int userId = Convert.ToInt32(Session["UserIdVal"]);
            bool hasPendingBorrow = false;
            bool hasApprovedBorrow = false;
            bool hasPendingReturn = false;
            bool hasApprovedReturn = false;
            OpenConnection();
            string query = @"
        SELECT RequestType, Status 
        FROM BookRequests 
        WHERE UserId = @UserId AND BookCopyId = @BookId
        ORDER BY RequestDate DESC";

            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string requestType = reader["RequestType"].ToString();
                        string status = reader["Status"].ToString();

                        if (requestType == "Borrow")
                        {
                            if (status == "Pending")
                                hasPendingBorrow = true;
                            else if (status == "Approved")
                                hasApprovedBorrow = true;
                        }
                        else if (requestType == "Buy")
                        {
                            if (status == "Approved" || status == "Rejected")
                            {
                                lblBuyMessage.Text = status == "Approved"
                                    ? "You have already purchased this book."
                                    : "Your previous purchase request was rejected.";
                                lblBuyMessage.Visible = true;
                            }
                            else if (status == "Pending")
                            {
                                lblBuyMessage.Text = "Your purchase request is still pending approval.";
                                lblBuyMessage.Visible = true;
                            }
                        }
                        else if (requestType == "Return")
                        {
                            if (status == "Pending")
                                hasPendingReturn = true;
                            else if (status == "Approved")
                                hasApprovedReturn = true;
                        }
                    }
                }
            }

            // -----------------------------
            // 🔘 Borrow Button Logic
            // -----------------------------
            // Hide if there's any approved borrow
            btnRequestBorrow.Visible = !hasApprovedBorrow;

            // Disable if there's any pending borrow
            btnRequestBorrow.Enabled = !hasPendingBorrow && btnRequestBorrow.Visible;

            // Messages
            if (hasPendingBorrow)
            {
                lblBorrowMessage.Text = "Your borrow request is pending approval.";
                lblBorrowMessage.Visible = true;
            }
            else if (hasApprovedBorrow && !hasPendingReturn)
            {
                lblBorrowMessage.Text = "You are currently borrowing this book.";
                lblBorrowMessage.Visible = true;
            }
            else if (hasPendingReturn)
            {
                lblBorrowMessage.Text = "A return request has been submitted. Waiting for admin confirmation.";
                lblBorrowMessage.Visible = true;
            }
            else if (hasApprovedReturn)
            {
                lblBorrowMessage.Text = "This book has been successfully returned.";
                lblBorrowMessage.Visible = true;
            }
            else
            {
                lblBorrowMessage.Visible = false;
            }

            // -----------------------------
            // 🔄 Return Button Logic
            // -----------------------------
            // Show only if there's an approved borrow
            btnReturnBook.Visible = hasApprovedBorrow;

            // Disable if there's a pending return
            btnReturnBook.Enabled = !hasPendingReturn && btnReturnBook.Visible;

            // -----------------------------
            // 💵 Buy Button Logic
            // -----------------------------
            string lastBuyStatus = GetLastBuyRequestStatus(bookId, userId);
            btnRequestBuy.Enabled = (lastBuyStatus != null && (lastBuyStatus == "Approved" || lastBuyStatus == "Rejected"))|| lastBuyStatus==null;
            btnRequestBuy.Visible = true; // Always visible but disabled when needed

            if (btnRequestBuy.Enabled)
            {
                lblBuyMessage.Text = "";
                lblBuyMessage.Visible = false;
            }
        }
        private string GetLastBuyRequestStatus(int bookId, int userId)
        {
            string query = @"
        SELECT TOP 1 Status 
        FROM BookRequests 
        WHERE UserId = @UserId AND BookCopyId = @BookId AND RequestType = 'Buy'
        ORDER BY RequestDate DESC";

            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookId", bookId);

                object result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }
        private bool HasApprovedReturn(int bookId, int userId)
        {
            string query = @"
        SELECT COUNT(*) 
        FROM BookRequests 
        WHERE UserId = @UserId AND BookCopyId = @BookId AND RequestType = 'Return' AND Status = 'Approved'";

            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookId", bookId);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        private bool HasPendingOrApprovedBuyRequest(int bookId, int userId)
        {
            string query = @"
        SELECT COUNT(*) 
        FROM BookRequests 
        WHERE UserId = @UserId AND BookCopyId = @BookId AND RequestType = 'Buy'
          AND Status IN ('Pending', 'Approved')";

            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        private bool IsValidBookCopy(int bookCopyId)
        {
            string query = "SELECT COUNT(*) FROM BookCopies WHERE BookId = @BookCopyId";
            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@BookCopyId", bookCopyId);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private bool IsBorrowAvailable(int bookId)
        {
            string query = @"
                SELECT AvailableCopies 
                FROM BookCopies 
                WHERE BookId = @BookId AND IsForBorrow = 1";
            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        protected void btnRequestBorrow_Click(object sender, EventArgs e)
        {
            SubmitBookRequest("Borrow");
        }

        protected void btnRequestBuy_Click(object sender, EventArgs e)
        {
            SubmitBookRequest("Buy");
        }

        private void SubmitBookRequest(string requestType)
        {
            int bookId = Convert.ToInt32(Request.QueryString["id"]);
            int userId = Convert.ToInt32(Session["UserIdVal"]);

            try
            {
                OpenConnection();

                // Get valid BookCopyId based on request type
                int bookCopyId = GetAvailableBookCopyId(bookId, requestType == "Borrow");
                if (bookCopyId == 0)
                {
                    ShowAlert("No available copies for this request type.");
                    return;
                }

                // Check existing requests for buy operations
                if (requestType == "Buy")
                {
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM BookRequests 
                WHERE UserId = @UserId 
                AND BookCopyId = @BookCopyId 
                AND RequestType = 'Buy' 
                AND Status = 'Pending'";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, Conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        checkCmd.Parameters.AddWithValue("@BookCopyId", bookCopyId);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            ShowAlert("You already have a pending buy request for this book.");
                            return;
                        }
                    }
                }

                // Insert request with valid BookCopyId
                string insertQuery = @"
            INSERT INTO BookRequests 
            (UserId, BookCopyId, RequestType, RequestDate)
            VALUES (@UserId, @BookCopyId, @RequestType, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(insertQuery, Conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookCopyId", bookCopyId); // Use valid copy ID
                    cmd.Parameters.AddWithValue("@RequestType", requestType);
                    cmd.ExecuteNonQuery();
                }

                ShowAlert($"Your '{requestType}' request has been submitted successfully.");
                LoadBookDetails(bookId);
            }
            catch (Exception ex)
            {
                ShowAlert($"Error submitting request: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }

        private int GetAvailableBookCopyId(int bookId, bool isForBorrow)
        {
            string query = @"
        SELECT TOP 1 Id 
        FROM BookCopies 
        WHERE BookId = @BookId 
        AND IsForBorrow = @IsForBorrow 
        AND AvailableCopies > 0";

            using (SqlCommand cmd = new SqlCommand(query, Conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.Parameters.AddWithValue("@IsForBorrow", isForBorrow);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        protected void btnReturnBook_Click(object sender, EventArgs e)
        {
            int bookId = Convert.ToInt32(Request.QueryString["id"]);
            int userId = Convert.ToInt32(Session["UserIdVal"]);

            try
            {
                OpenConnection();

                string checkQuery = @"
                    SELECT Id 
                    FROM BookRequests 
                    WHERE UserId = @UserId AND BookCopyId = @BookId AND RequestType = 'Borrow' AND Status = 'Approved'";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, Conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                    checkCmd.Parameters.AddWithValue("@BookId", bookId);
                    object result = checkCmd.ExecuteScalar();
                    if (result == null)
                    {
                        ShowAlert("You cannot return a book you haven't borrowed.");
                        return;
                    }
                }

                string insertQuery = @"
                    INSERT INTO BookRequests (UserId, BookCopyId, RequestType, RequestDate, Status)
                    VALUES (@UserId, @BookId, 'Return', GETDATE(), 'Pending')";

                using (SqlCommand insertCmd = new SqlCommand(insertQuery, Conn))
                {
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@BookId", bookId);
                    insertCmd.ExecuteNonQuery();
                }

                ShowAlert("Your return request has been submitted. An admin will process it soon.");
                LoadBookDetails(bookId);
            }
            catch (Exception ex)
            {
                ShowAlert($"Error submitting return request: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }

        private void LoadPublisherBooks(int currentBookId)
        {
            try
            {
                OpenConnection();

                string query = @"
                    SELECT  Id,Name, Description, Price, NumOfPages, PublishDate
                    FROM Books
                    WHERE UserId = (SELECT UserId FROM Books WHERE Id = @CurrentBookId)
                      AND Id <> @CurrentBookId";

                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentBookId", currentBookId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvPublisherBooks.DataSource = dt;
                    gvPublisherBooks.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error loading publisher's books: {ex.Message}");
            }
        }
        private void LoadTransactionHistory(int bookId, int userId)
        {
            string query = @"
        SELECT 
            RequestType, 
            RequestDate, 
            Status, 
            ReturnDate
        FROM BookRequests
        WHERE UserId = @UserId AND BookCopyId = @BookId
        ORDER BY RequestDate DESC";

            try
            {
                OpenConnection();
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvTransactionHistory.DataSource = dt;
                    gvTransactionHistory.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error loading transaction history: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("BookListUser.aspx");
        }

        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'");
            ScriptManager.RegisterStartupScript(this, GetType(), "alertScript", $"alert('{cleanMessage}');", true);
        }
    }
}