using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class BookListAdmin : System.Web.UI.Page
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
                LoadDropdowns();
                LoadBooks();
            }
        }

        protected void open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }
        protected void btnToggleForm_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the form
            Panel1.Visible = !Panel1.Visible;
            Panel2.Visible = !Panel2.Visible;
            pnlAddCategory.Visible = false;
            // Update button text based on visibility of the form
            btnToggleForm.Text = Panel1.Visible ? "Hide Form" : "Add New Book";

            // Optionally reset the form when hiding
            if (!Panel1.Visible)
            {
                ResetForm();
            }
        }

        private void close_conn()
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
                Conn.Close();
        }

        private void LoadDropdowns()
        {
            open_conn();

   


            SqlDataAdapter daPublisher = new SqlDataAdapter(@"SELECT u.Id, u.Name, u.Email, u.Phone
            FROM Users u
            INNER JOIN UserRoles ur ON u.Id = ur.UserId
            INNER JOIN Roles r ON ur.RoleId = r.Id
            WHERE r.Name = 'Publisher';
            ", Conn);
            DataTable dtPublisher = new DataTable();
            daPublisher.Fill(dtPublisher);
            ddlPublisher.DataSource = dtPublisher;
            ddlPublisher.DataTextField = "Name";
            ddlPublisher.DataValueField = "Id";
            ddlPublisher.DataBind();
            
            SqlDataAdapter daCategory = new SqlDataAdapter("SELECT Id, Name FROM Categories", Conn);
            DataTable dtCategory = new DataTable();
            daCategory.Fill(dtCategory);
            ddlCategory.DataSource = dtCategory;
            ddlCategory.DataTextField = "Name";
            ddlCategory.DataValueField = "Id";
            ddlCategory.DataBind();

            close_conn();
        }

        protected void LoadBooks()
        {
            var currentUserId = Convert.ToInt32(Session["UserIdVal"].ToString());
            open_conn();

            try
            {
                string query = @"
    SELECT 
        B.Id, 
        B.Name, 
        B.Description, 
        B.Price, 
        B.NumOfPages, 
        B.PublishDate,
        C.Name AS CategoryName,
        B.StatusId AS BookStatusId,
        BU.StatusId AS LastUpdateStatusId,
        BU.CreatedAt AS LastUpdateDate,
        COALESCE(SUM(CASE WHEN BC.IsForBorrow = 0 THEN BC.AvailableCopies ELSE 0 END), 0) AS CopiesForSale,
        COALESCE(SUM(CASE WHEN BC.IsForBorrow = 1 THEN BC.AvailableCopies ELSE 0 END), 0) AS CopiesForBorrow
    FROM Books B
    JOIN Categories C ON B.CategoryId = C.Id
    LEFT JOIN BookCopies BC ON B.Id = BC.BookId
    LEFT JOIN (
        SELECT 
            BookId, StatusId, CreatedAt,
            ROW_NUMBER() OVER (PARTITION BY BookId ORDER BY CreatedAt DESC) AS RowNum
        FROM BookUpdates
    ) BU ON B.Id = BU.BookId AND BU.RowNum = 1
    GROUP BY B.Id, B.Name, B.Description, B.Price, B.NumOfPages, B.PublishDate, C.Name, B.StatusId, BU.StatusId, BU.CreatedAt"; SqlDataAdapter da = new SqlDataAdapter(query, Conn);
                da.SelectCommand.Parameters.AddWithValue("@currentUserId", currentUserId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                gvBooks.DataSource = dt;
                gvBooks.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error while loading books: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }
        }
        protected void gvBooks_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Safely retrieve StatusId and LastUpdateStatusId
                int statusId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "BookStatusId"));
                object lastUpdateStatusIdObj = DataBinder.Eval(e.Row.DataItem, "LastUpdateStatusId");
                int lastUpdateStatusId = lastUpdateStatusIdObj != DBNull.Value
                    ? Convert.ToInt32(lastUpdateStatusIdObj)
                    : 0; // Default to 0 if LastUpdateStatusId is NULL

                // Find the "Reject" LinkButton in the row
                LinkButton lnkReject = (LinkButton)e.Row.FindControl("lnkReject");
                if (statusId == 1) // Pending Admin Approval
                {
                    lnkReject.Visible = true; // Show "Reject" link
                }
                else
                {
                    lnkReject.Visible = false; // Hide "Reject" link
                }

                // Find the "Accept Update" and "Reject Update" LinkButtons in the row
                LinkButton lnkAcceptUpdate = (LinkButton)e.Row.FindControl("lnkAcceptUpdate");
                LinkButton lnkRejectUpdate = (LinkButton)e.Row.FindControl("lnkRejectUpdate");

                // Show or hide the "Accept Update" and "Reject Update" links
                if (lastUpdateStatusId == 1) // Pending Update Approval
                {
                    lnkAcceptUpdate.Visible = true;
                    lnkRejectUpdate.Visible = true;
                }
                else
                {
                    lnkAcceptUpdate.Visible = false;
                    lnkRejectUpdate.Visible = false;
                }
            }
        }
        protected void RejectBook(int bookId)
        {
            open_conn();
            try
            {
                string updateQuery = @"
        UPDATE Books
        SET StatusId = 3 -- Rejected
        WHERE Id = @BookId";
                SqlCommand cmd = new SqlCommand(updateQuery, Conn);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    ShowAlert("Book rejected successfully.");
                }
                else
                {
                    ShowAlert("Failed to reject the book. Book may not exist.");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error while rejecting book: " + ex.Message);
            }
            finally
            {
                close_conn();
                LoadBooks(); // Reload books after rejection
            }
        }
        protected void gvBooks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "UpdateBook")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvBooks.Rows[rowIndex];
                int bookId = Convert.ToInt32(gvBooks.DataKeys[rowIndex].Value);

                // Load book data and populate the form
                LoadBookForEditing(bookId);

                // Show the form
                Panel1.Visible = true;
                Panel2.Visible = true;

                btnToggleForm.Text = "Hide Form";
                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else if (e.CommandName == "ConfirmBook")
            {
                int bookId = Convert.ToInt32(e.CommandArgument);
                ConfirmBook(bookId);
            }
            else if (e.CommandName == "DeleteBook")
            {
                int bookId = Convert.ToInt32(e.CommandArgument);
                DeleteBook(bookId);
            }
            else if (e.CommandName == "RejectBook")
            {
                int bookId = Convert.ToInt32(e.CommandArgument);
                RejectBook(bookId);
            }
            else if (e.CommandName == "AcceptUpdate")
            {
                int bookId = Convert.ToInt32(e.CommandArgument);
                AcceptUpdate(bookId);
            }
            else if (e.CommandName == "RejectUpdate")
            {
                int bookId = Convert.ToInt32(e.CommandArgument);
                RejectUpdate(bookId);
            }
        }
        private void LoadBookForEditing(int bookId)
        {
            open_conn();
            try
            {
                string query = @"
            SELECT 
                B.Id, B.Name, B.Description, B.Price, B.NumOfPages, B.PublishDate, 
                B.CategoryId, B.UserId, B.ImageUrl,
                COALESCE(SUM(CASE WHEN BC.IsForBorrow = 0 THEN BC.AvailableCopies ELSE 0 END), 0) AS CopiesForSale,
                COALESCE(SUM(CASE WHEN BC.IsForBorrow = 1 THEN BC.AvailableCopies ELSE 0 END), 0) AS CopiesForBorrow
            FROM Books B
            LEFT JOIN BookCopies BC ON B.Id = BC.BookId
            WHERE B.Id = @BookId
            GROUP BY B.Id, B.Name, B.Description, B.Price, B.NumOfPages, B.PublishDate, B.CategoryId, B.UserId, B.ImageUrl";

                SqlCommand cmd = new SqlCommand(query, Conn);
                cmd.Parameters.AddWithValue("@BookId", bookId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    hfBookId.Value = bookId.ToString();
                    txtBookName.Text = reader["Name"].ToString();
                    txtDescription.Text = reader["Description"].ToString();
                    txtPrice.Text = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]).ToString("F2") : "";
                    txtNumOfPages.Text = reader["NumOfPages"] != DBNull.Value ? reader["NumOfPages"].ToString() : "";
                    txtPublishDate.Text = reader["PublishDate"] != DBNull.Value ? Convert.ToDateTime(reader["PublishDate"]).ToString("yyyy-MM-dd") : "";
                    ddlCategory.SelectedValue = reader["CategoryId"] != DBNull.Value ? reader["CategoryId"].ToString() : "1";
                    txtCopiesForSale.Text = reader["CopiesForSale"] != DBNull.Value ? reader["CopiesForSale"].ToString() : "0";
                    txtCopiesForBorrow.Text = reader["CopiesForBorrow"] != DBNull.Value ? reader["CopiesForBorrow"].ToString() : "0";

                    // Optional: Load image preview
                    string imageUrl = reader["ImageUrl"] as string;
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "previewImage", $@"
                    var imgPreview = document.getElementById('imgPreview');
                    imgPreview.src = '{ResolveUrl("~/" + imageUrl)}';
                    imgPreview.style.display = 'block';", true);
                    }
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                ShowAlert("Error loading book for editing: " + ex.Message);
            }
            finally
            {
                close_conn();
            }
        }
        protected void RejectUpdate(int bookId)
        {
            open_conn();
            try
            {
                // Get the latest pending update for the book
                string selectQuery = @"
        SELECT Id
        FROM BookUpdates
        WHERE BookId = @BookId AND StatusId = 1 -- Pending Approval
        ORDER BY CreatedAt DESC
        OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY";
                SqlCommand selectCmd = new SqlCommand(selectQuery, Conn);
                selectCmd.Parameters.AddWithValue("@BookId", bookId);
                object updateId = selectCmd.ExecuteScalar();

                if (updateId != null && updateId != DBNull.Value)
                {
                    // Mark the update as rejected (StatusId = 3)
                    string rejectQuery = @"
          UPDATE BookUpdates
  SET StatusId = 3 -- Approved
  WHERE BookId = @updateId AND StatusId = 1";
                    SqlCommand rejectCmd = new SqlCommand(rejectQuery, Conn);
                    rejectCmd.Parameters.AddWithValue("@UpdateId", updateId);
                    int rowsAffected = rejectCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ShowAlert("Update rejected successfully.");
                    }
                    else
                    {
                        ShowAlert("Failed to reject the update. Update may not exist.");
                    }
                }
                else
                {
                    ShowAlert("No pending updates found for this book.");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error while rejecting update: " + ex.Message);
            }
            finally
            {
                close_conn();
                LoadBooks(); // Reload books after rejecting the update
            }
        }
        protected void AcceptUpdate(int bookId)
        {
            open_conn();
            try
            {
                // Get the latest update for the book
                string selectQuery = @"
        SELECT 
            UpdatedName, UpdatedCategoryId, UpdatedDescription, 
            UpdatedNumOfPages, UpdatedPrice, UpdatedPublishDate
        FROM BookUpdates
        WHERE BookId = @BookId AND StatusId = 1 -- Pending Approval
        ORDER BY CreatedAt DESC
        OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY";
                SqlCommand selectCmd = new SqlCommand(selectQuery, Conn);
                selectCmd.Parameters.AddWithValue("@BookId", bookId);

                // Use ExecuteReader to fetch the data
                SqlDataReader reader = selectCmd.ExecuteReader();
                if (reader.Read())
                {
                    // Read the values from the reader
                    string updatedName = reader["UpdatedName"].ToString();
                    int updatedCategoryId = Convert.ToInt32(reader["UpdatedCategoryId"]);
                    string updatedDescription = reader["UpdatedDescription"].ToString();
                    int updatedNumOfPages = Convert.ToInt32(reader["UpdatedNumOfPages"]);
                    decimal updatedPrice = Convert.ToDecimal(reader["UpdatedPrice"]);
                    DateTime updatedPublishDate = Convert.ToDateTime(reader["UpdatedPublishDate"]);

                    // Close the reader before executing other commands
                    reader.Close();

                    // Update the Books table with the latest values
                    string updateQuery = @"
            UPDATE Books
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                NumOfPages = @NumOfPages,
                PublishDate = @PublishDate,
                CategoryId = @CategoryId,
                StatusId = 2 -- Approved
            WHERE Id = @BookId";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, Conn);
                    updateCmd.Parameters.AddWithValue("@Name", updatedName);
                    updateCmd.Parameters.AddWithValue("@Description", updatedDescription);
                    updateCmd.Parameters.AddWithValue("@Price", updatedPrice);
                    updateCmd.Parameters.AddWithValue("@NumOfPages", updatedNumOfPages);
                    updateCmd.Parameters.AddWithValue("@PublishDate", updatedPublishDate);
                    updateCmd.Parameters.AddWithValue("@CategoryId", updatedCategoryId);
                    updateCmd.Parameters.AddWithValue("@BookId", bookId);
                    updateCmd.ExecuteNonQuery();

                    // Update the BookUpdates table to mark the update as approved
                    string approveQuery = @"
          UPDATE BookUpdates
  SET StatusId = 2 -- Approved
  WHERE BookId = @BookId AND StatusId = 1 ";
                    SqlCommand approveCmd = new SqlCommand(approveQuery, Conn);
                    approveCmd.Parameters.AddWithValue("@BookId", bookId);
                    approveCmd.ExecuteNonQuery();

                    ShowAlert("Update accepted successfully.");
                }
                else
                {
                    reader.Close(); // Ensure the reader is closed if no data is found
                    ShowAlert("No pending updates found for this book.");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error while accepting update: " + ex.Message);
            }
            finally
            {
                close_conn();
                LoadBooks(); // Reload books after accepting the update
            }
        }
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtBookName.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtNumOfPages.Text) ||
                string.IsNullOrWhiteSpace(txtCopiesForBorrow.Text) ||
                string.IsNullOrWhiteSpace(txtCopiesForSale.Text) ||
                string.IsNullOrWhiteSpace(txtPublishDate.Text))
            {
                lblMessage.Text = "Please fill in all required fields.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            open_conn();

            try
            {
                int bookId = Convert.ToInt32(hfBookId.Value);
                int currentUserId = Convert.ToInt32(Session["UserIdVal"]);

                // Check if the book has been confirmed by the admin
                string checkBookStatusQuery = @"
            SELECT StatusId 
            FROM Books 
            WHERE Id = @BookId";
                SqlCommand checkBookStatusCmd = new SqlCommand(checkBookStatusQuery, Conn);
                checkBookStatusCmd.Parameters.AddWithValue("@BookId", bookId);

                object bookStatusId = checkBookStatusCmd.ExecuteScalar();

                if (bookStatusId != null && bookStatusId != DBNull.Value)
                {

                    int statusId = Convert.ToInt32(bookStatusId);
                    if (statusId == 3)
                    {
                        lblMessage.Text = "This book is rejected.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                    string updateBookQuery = @"
                    UPDATE Books
                    SET Name = @Name,
                        Description = @Description,
                        Price = @Price,
                        NumOfPages = @NumOfPages,
                        PublishDate = @PublishDate,
                        CategoryId = @CategoryId
                    WHERE Id = @BookId";

                        SqlCommand updateBookCmd = new SqlCommand(updateBookQuery, Conn);
                        updateBookCmd.Parameters.AddWithValue("@BookId", bookId);
                        updateBookCmd.Parameters.AddWithValue("@Name", txtBookName.Text);
                        updateBookCmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                        updateBookCmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(txtPrice.Text));
                        updateBookCmd.Parameters.AddWithValue("@NumOfPages", Convert.ToInt32(txtNumOfPages.Text));
                        updateBookCmd.Parameters.AddWithValue("@PublishDate", Convert.ToDateTime(txtPublishDate.Text));
                        updateBookCmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt32(ddlCategory.SelectedValue));

                        updateBookCmd.ExecuteNonQuery();
                    if (statusId == 2)
                    {
                        lblMessage.Text = "The book has been updated successfully.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        lblMessage.Text = "The book has been updated successfully but has not yet been approved.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
                        int copiesForSale = int.Parse(txtCopiesForSale.Text);
                        int copiesForBorrow = int.Parse(txtCopiesForBorrow.Text);
                        UpdateBookCopies(bookId, copiesForSale, copiesForBorrow);
                 
                }
                else
                {
                    lblMessage.Text = "Unable to determine the status of the book. Please try again.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error while processing your request: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }

            ResetForm();
            LoadBooks();
        }
        // Add this method to BookListAdmin.aspx.cs
        private void UpdateBookCopies(int bookId, int copiesForSale, int copiesForBorrow)
        {
            // Update sale copies
            string updateSaleQuery = @"
        UPDATE BookCopies 
        SET AvailableCopies = @Copies 
        WHERE BookId = @BookId AND IsForBorrow = 0";

            using (SqlCommand saleCmd = new SqlCommand(updateSaleQuery, Conn))
            {
                saleCmd.Parameters.AddWithValue("@Copies", copiesForSale);
                saleCmd.Parameters.AddWithValue("@BookId", bookId);
                saleCmd.ExecuteNonQuery();
            }

            // Update borrow copies
            string updateBorrowQuery = @"
        UPDATE BookCopies 
        SET AvailableCopies = @Copies 
        WHERE BookId = @BookId AND IsForBorrow = 1";

            using (SqlCommand borrowCmd = new SqlCommand(updateBorrowQuery, Conn))
            {
                borrowCmd.Parameters.AddWithValue("@Copies", copiesForBorrow);
                borrowCmd.Parameters.AddWithValue("@BookId", bookId);
                borrowCmd.ExecuteNonQuery();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtBookName.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtNumOfPages.Text) ||
                string.IsNullOrWhiteSpace(txtCopiesForBorrow.Text) ||
                string.IsNullOrWhiteSpace(txtCopiesForSale.Text) ||
                string.IsNullOrWhiteSpace(txtPublishDate.Text))
            {
                lblMessage.Text = "Please fill in all required fields.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            open_conn();
            string imageUrl = null;

            try
            {
                // Handle image upload
                if (fuBookImage.HasFile)
                {
                    string fileName = Path.GetFileName(fuBookImage.FileName);
                    imageUrl = $"Content/Images/TEMP/{fileName}";
                }

                // Insert book
                string query = @"
            INSERT INTO Books (
                Name, Description, Price, NumOfPages, PublishDate, UserId, CategoryId, 
                ImageUrl, IsAdminCreated, StatusId
            )
            OUTPUT INSERTED.Id
            VALUES (
                @Name, @Description, @Price, @NumOfPages, @PublishDate, @UserId, @CategoryId, 
                @ImageUrl, 0, 1 -- Default values
            )";

                SqlCommand cmd = new SqlCommand(query, Conn);
                cmd.Parameters.AddWithValue("@Name", txtBookName.Text);
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                cmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(txtPrice.Text));
                cmd.Parameters.AddWithValue("@NumOfPages", Convert.ToInt32(txtNumOfPages.Text));
                cmd.Parameters.AddWithValue("@PublishDate", Convert.ToDateTime(txtPublishDate.Text));
                cmd.Parameters.AddWithValue("@UserId", Convert.ToInt32(Session["UserIdVal"]));
                cmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt32(ddlCategory.SelectedValue));
                cmd.Parameters.AddWithValue("@ImageUrl", (object)imageUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsAdminCreated", 1); 
                cmd.Parameters.AddWithValue("@StatusId", 2);
                int bookId = (int)cmd.ExecuteScalar();

                // Save image if uploaded
                if (fuBookImage.HasFile)
                {
                    string folderPath = Server.MapPath($"~/Content/Images/{bookId}/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = Path.GetFileName(fuBookImage.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);
                    fuBookImage.SaveAs(fullPath);

                    string finalImageUrl = $"Content/Images/{bookId}/{fileName}";

                    string updateQuery = "UPDATE Books SET ImageUrl = @ImageUrl WHERE Id = @Id";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, Conn);
                    updateCmd.Parameters.AddWithValue("@ImageUrl", finalImageUrl);
                    updateCmd.Parameters.AddWithValue("@Id", bookId);
                    updateCmd.ExecuteNonQuery();
                }

                // Insert Book Copies
                int copiesForSale = string.IsNullOrEmpty(txtCopiesForSale.Text) ? 0 : int.Parse(txtCopiesForSale.Text);
                int copiesForBorrow = string.IsNullOrEmpty(txtCopiesForBorrow.Text) ? 0 : int.Parse(txtCopiesForBorrow.Text);

                InsertBookCopies(bookId, copiesForSale, copiesForBorrow);

                ResetForm();
                LoadBooks();
                lblMessage.Text = "Book added successfully. It will be reviewed before being published.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error while saving the book: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }
        }

        private void InsertBookCopies(int bookId, int copiesForSale, int copiesForBorrow)
        {
            if (copiesForSale > 0)
            {
                string insertQuery = @"
            INSERT INTO BookCopies (BookId, IsForBorrow, AvailableCopies)
            VALUES (@BookId, 0, @AvailableCopies)";
                SqlCommand cmd = new SqlCommand(insertQuery, Conn);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.Parameters.AddWithValue("@AvailableCopies", copiesForSale);
                cmd.ExecuteNonQuery();
            }

            if (copiesForBorrow > 0)
            {
                string insertQuery = @"
            INSERT INTO BookCopies (BookId, IsForBorrow, AvailableCopies)
            VALUES (@BookId, 1, @AvailableCopies)";
                SqlCommand cmd = new SqlCommand(insertQuery, Conn);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.Parameters.AddWithValue("@AvailableCopies", copiesForBorrow);
                cmd.ExecuteNonQuery();
            }
        }
        private void ResetForm()
        {
            txtBookName.Text = "";
            txtDescription.Text = "";
            txtPrice.Text = "";
            txtNumOfPages.Text = "";
            txtPublishDate.Text = "";
            ddlCategory.ClearSelection();
            hfBookId.Value = "";

            btnSave.Visible = true;
            btnUpdate.Visible = false;
        }

        protected string GetBookStatus(object statusId)
        {
            int status = Convert.ToInt32(statusId);
            switch (status)
            {
                case 1:
                    return "Pending Admin Approval";
                case 2:
                    return "Approved";
                case 3:
                    return "Rejected";
                default:
                    return "Unknown Status";
            }
        }
        protected string GetLastUpdateStatus(object updateStatusId, object updateDate)
        {
            if (updateStatusId == DBNull.Value || updateDate == DBNull.Value)
            {
                return "No Updates Made";
            }

            int status = Convert.ToInt32(updateStatusId);
            DateTime date = Convert.ToDateTime(updateDate);

            switch (status)
            {
                case 1:
                    return $"Pending Admin Review (Last Update: {date.ToShortDateString()})";
                case 2:
                    return $"Update Approved (Last Update: {date.ToShortDateString()})";
                case 3:
                    return $"Update Rejected (Last Update: {date.ToShortDateString()})";
                default:
                    return "Unknown Status";
            }
        }

        protected void ConfirmBook(int bookId)
        {
            open_conn();

            try
            {
                string updateQuery = @"
            UPDATE Books
            SET StatusId = 2
            WHERE Id = @BookId";

                SqlCommand cmd = new SqlCommand(updateQuery, Conn);
                cmd.Parameters.AddWithValue("@BookId", bookId);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    ShowAlert("Book confirmed successfully.");
                }
                else
                {
                    ShowAlert("Failed to confirm the book. Book may not exist.");
                }
            }
            catch (Exception ex)
            {
                ShowAlert("Error while confirming book: " + ex.Message);
            }
            finally
            {
                close_conn();
                LoadBooks(); // Reload books after confirmation
            }
        }


        private void DeleteBook(int bookId)
        {
            open_conn();
            SqlTransaction transaction = null;

            try
            {
                // Start transaction
                transaction = Conn.BeginTransaction();

                // Step 1: Delete Book Requests referencing book copies
                string deleteRequestsQuery = "DELETE FROM BookRequests WHERE BookCopyId IN (SELECT Id FROM BookCopies WHERE BookId = @BookId)";
                SqlCommand deleteRequestsCmd = new SqlCommand(deleteRequestsQuery, Conn, transaction);
                deleteRequestsCmd.Parameters.AddWithValue("@BookId", bookId);
                deleteRequestsCmd.ExecuteNonQuery();

                // Step 2: Delete Transactions that reference book copies
                string deleteTransactionsQuery = "DELETE FROM Transactions WHERE BookCopyId IN (SELECT Id FROM BookCopies WHERE BookId = @BookId)";
                SqlCommand deleteTransactionsCmd = new SqlCommand(deleteTransactionsQuery, Conn, transaction);
                deleteTransactionsCmd.Parameters.AddWithValue("@BookId", bookId);
                deleteTransactionsCmd.ExecuteNonQuery();

                // Step 3: Delete Book Copies
                string deleteCopiesQuery = "DELETE FROM BookCopies WHERE BookId = @BookId";
                SqlCommand deleteCopiesCmd = new SqlCommand(deleteCopiesQuery, Conn, transaction);
                deleteCopiesCmd.Parameters.AddWithValue("@BookId", bookId);
                deleteCopiesCmd.ExecuteNonQuery();

                // Step 4: Delete Book Updates
                string deleteUpdatesQuery = "DELETE FROM BookUpdates WHERE BookId = @BookId";
                SqlCommand deleteUpdatesCmd = new SqlCommand(deleteUpdatesQuery, Conn, transaction);
                deleteUpdatesCmd.Parameters.AddWithValue("@BookId", bookId);
                deleteUpdatesCmd.ExecuteNonQuery();

                // Step 5: Finally, delete the book itself
                string deleteBookQuery = "DELETE FROM Books WHERE Id = @BookId";
                SqlCommand deleteBookCmd = new SqlCommand(deleteBookQuery, Conn, transaction);
                deleteBookCmd.Parameters.AddWithValue("@BookId", bookId);
                int rowsAffected = deleteBookCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    ShowAlert("Book and all related data deleted successfully.");
                }
                else
                {
                    ShowAlert("No book found with the given ID.");
                }

                // Commit transaction
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                transaction?.Rollback();
                ShowAlert("Error while deleting book: " + ex.Message);
            }
            finally
            {
                close_conn();
                LoadBooks(); // Refresh grid view
            }
        }
        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'"); // Escape any single quotes
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertMessage", $"alert('{cleanMessage}');", true);
        }
        protected void btnToggleCategoryForm_Click(object sender, EventArgs e)
        {
            pnlAddCategory.Visible = !pnlAddCategory.Visible;
            btnToggleCategoryForm.Text = pnlAddCategory.Visible ? "Hide Form" : "Add New Category";

            Panel1.Visible = false;
            Panel2.Visible = false;
        }

        protected void btnAddCategory_Click(object sender, EventArgs e)
        {
            string categoryName = txtNewCategory.Text.Trim();
            if (string.IsNullOrEmpty(categoryName))
            {
                lblMessage.Text = "Category name cannot be empty.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            open_conn();
            SqlTransaction transaction = Conn.BeginTransaction();
            try
            {
                // Check for duplicates
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Categories WHERE Name = @Name",
                    Conn, transaction);
                checkCmd.Parameters.AddWithValue("@Name", categoryName);
                int existingCount = (int)checkCmd.ExecuteScalar();

                if (existingCount > 0)
                {
                    lblMessage.Text = "Category already exists.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Insert new category
                var insertCmd = new SqlCommand(
                    "INSERT INTO Categories (Name) VALUES (@Name)",
                    Conn, transaction);
                insertCmd.Parameters.AddWithValue("@Name", categoryName);
                insertCmd.ExecuteNonQuery();

                transaction.Commit();
                lblMessage.Text = "Category added successfully.";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                // Refresh category dropdown
                LoadDropdowns();
                txtNewCategory.Text = "";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                close_conn();
            }
        }
    }


    }
