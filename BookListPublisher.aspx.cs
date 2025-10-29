using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class BookListPublisher : System.Web.UI.Page
    {
        SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserRoleVal"] == null || Session["UserRoleVal"].ToString() != "Publisher")
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
            PublisherPanel2.Visible = !PublisherPanel2.Visible;

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

            SqlDataAdapter daPublisher = new SqlDataAdapter("SELECT Id, Name FROM Users", Conn);
            DataTable dtPublisher = new DataTable();
            daPublisher.Fill(dtPublisher);
 

            SqlDataAdapter daCategory = new SqlDataAdapter("SELECT Id, Name FROM Categories", Conn);
            DataTable dtCategory = new DataTable();
            daCategory.Fill(dtCategory);
            ddlCategory.DataSource = dtCategory;
            ddlCategory.DataTextField = "Name";
            ddlCategory.DataValueField = "Id";
            ddlCategory.DataBind();

            close_conn();
        }

        private void LoadBooks()
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
                -- Add these two columns
                (SELECT AvailableCopies FROM BookCopies WHERE BookId = B.Id AND IsForBorrow = 0) AS CopiesForSale,
                (SELECT AvailableCopies FROM BookCopies WHERE BookId = B.Id AND IsForBorrow = 1) AS CopiesForBorrow
            FROM Books B
            JOIN Categories C ON B.CategoryId = C.Id
            LEFT JOIN (
                SELECT 
                    BookId, 
                    StatusId, 
                    CreatedAt,
                    ROW_NUMBER() OVER (PARTITION BY BookId ORDER BY CreatedAt DESC) AS RowNum
                FROM BookUpdates
            ) BU ON B.Id = BU.BookId AND BU.RowNum = 1
            WHERE B.UserId = @currentUserId";
                SqlDataAdapter da = new SqlDataAdapter(query, Conn);
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
        protected void gvBooks_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected row
            GridViewRow row = gvBooks.SelectedRow;
            // Get the book Id from the DataKey
            hfBookId.Value = gvBooks.DataKeys[row.RowIndex].Value.ToString();

            // Populate form fields
            txtBookName.Text = ((LinkButton)row.FindControl("lnkBookName")).Text;
            txtDescription.Text = Server.HtmlDecode(row.Cells[1].Text);
            txtPrice.Text = row.Cells[2].Text;
            txtNumOfPages.Text = row.Cells[3].Text;
            txtPublishDate.Text = Convert.ToDateTime(row.Cells[4].Text).ToString("yyyy-MM-dd");

            // Populate Category dropdown
            ddlCategory.ClearSelection();
            var categoryItem = ddlCategory.Items.FindByText(Server.HtmlDecode(row.Cells[5].Text));
            if (categoryItem != null) categoryItem.Selected = true;

          
                txtCopiesForSale.Text = row.Cells[6].Text;
           
                txtCopiesForBorrow.Text = row.Cells[7].Text;
            

            // Show the form panels
            Panel1.Visible = true;
            PublisherPanel2.Visible = true;
            // Toggle button states
            btnSave.Visible = false;
            btnUpdate.Visible = true;
            btnToggleForm.Text = "Hide Form"; // Update the toggle button text
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

                    if (statusId == 1) // Book is still pending admin approval
                    {
                        // Update the book directly in the Books table
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

                        lblMessage.Text = "The book has been updated successfully.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                        int copiesForSale = int.Parse(txtCopiesForSale.Text);
                        int copiesForBorrow = int.Parse(txtCopiesForBorrow.Text);
                        UpdateBookCopies(bookId, copiesForSale, copiesForBorrow);
                    }
                    else
                    {
                        // Check if there is an existing unconfirmed update for this book
                        string checkQuery = @"
                    SELECT Id 
                    FROM BookUpdates 
                    WHERE BookId = @BookId AND StatusId = 1"; // StatusId = 1 means Pending

                        SqlCommand checkCmd = new SqlCommand(checkQuery, Conn);
                        checkCmd.Parameters.AddWithValue("@BookId", bookId);

                        object existingUpdateId = checkCmd.ExecuteScalar();

                        if (existingUpdateId != null && existingUpdateId != DBNull.Value)
                        {
                            // Update the existing unconfirmed update in the BookUpdates table
                            string updateQuery = @"
                        UPDATE BookUpdates
                        SET UpdatedName = @UpdatedName,
                            UpdatedCategoryId = @UpdatedCategoryId,
                            UpdatedDescription = @UpdatedDescription,
                            UpdatedNumOfPages = @UpdatedNumOfPages,
                            UpdatedPrice = @UpdatedPrice,
                            UpdatedPublishDate = @UpdatedPublishDate,
                            UpdatedBy = @UpdatedBy
                        WHERE Id = @UpdateId";

                            SqlCommand updateCmd = new SqlCommand(updateQuery, Conn);
                            updateCmd.Parameters.AddWithValue("@UpdateId", existingUpdateId);
                            updateCmd.Parameters.AddWithValue("@UpdatedName", txtBookName.Text);
                            updateCmd.Parameters.AddWithValue("@UpdatedCategoryId", Convert.ToInt32(ddlCategory.SelectedValue));
                            updateCmd.Parameters.AddWithValue("@UpdatedDescription", txtDescription.Text);
                            updateCmd.Parameters.AddWithValue("@UpdatedNumOfPages", Convert.ToInt32(txtNumOfPages.Text));
                            updateCmd.Parameters.AddWithValue("@UpdatedPrice", Convert.ToDecimal(txtPrice.Text));
                            updateCmd.Parameters.AddWithValue("@UpdatedPublishDate", Convert.ToDateTime(txtPublishDate.Text));
                            updateCmd.Parameters.AddWithValue("@UpdatedBy", currentUserId);

                            updateCmd.ExecuteNonQuery();

                            lblMessage.Text = "Your update request has been updated successfully. It will be reviewed before being applied.";
                        }
                        else
                        {
                            // Insert a new update request into the BookUpdates table
                            string insertQuery = @"
                        INSERT INTO BookUpdates (
                            BookId, UpdatedName, UpdatedCategoryId, UpdatedDescription,
                            UpdatedNumOfPages, UpdatedPrice, UpdatedPublishDate, StatusId, UpdatedBy
                        )
                        VALUES (
                            @BookId, @UpdatedName, @UpdatedCategoryId, @UpdatedDescription,
                            @UpdatedNumOfPages, @UpdatedPrice, @UpdatedPublishDate, 1, @UpdatedBy
                        )";

                            SqlCommand insertCmd = new SqlCommand(insertQuery, Conn);
                            insertCmd.Parameters.AddWithValue("@BookId", bookId);
                            insertCmd.Parameters.AddWithValue("@UpdatedName", txtBookName.Text);
                            insertCmd.Parameters.AddWithValue("@UpdatedCategoryId", Convert.ToInt32(ddlCategory.SelectedValue));
                            insertCmd.Parameters.AddWithValue("@UpdatedDescription", txtDescription.Text);
                            insertCmd.Parameters.AddWithValue("@UpdatedNumOfPages", Convert.ToInt32(txtNumOfPages.Text));
                            insertCmd.Parameters.AddWithValue("@UpdatedPrice", Convert.ToDecimal(txtPrice.Text));
                            insertCmd.Parameters.AddWithValue("@UpdatedPublishDate", Convert.ToDateTime(txtPublishDate.Text));
                            insertCmd.Parameters.AddWithValue("@UpdatedBy", currentUserId);

                            insertCmd.ExecuteNonQuery();
                            int copiesForSale = int.Parse(txtCopiesForSale.Text);
                            int copiesForBorrow = int.Parse(txtCopiesForBorrow.Text);
                            UpdateBookCopies(bookId, copiesForSale, copiesForBorrow);
                            lblMessage.Text = "Your update request has been submitted successfully. It will be reviewed before being applied.";
                        }
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                    }
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
        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtBookName.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtNumOfPages.Text) ||
                string.IsNullOrWhiteSpace(txtPublishDate.Text))
            {
                lblMessage.Text = "Please fill in all required fields.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            open_conn();

            string imageUrl = null;

            // Handle image upload
            if (fuBookImage.HasFile)
            {
                string fileName = Path.GetFileName(fuBookImage.FileName);
                imageUrl = $"Content/Images/TEMP/{fileName}";
            }

            // Insert book into the database
            string query = @"
        INSERT INTO Books (
            Name, Description, Price, NumOfPages, PublishDate, UserId, CategoryId, 
            ImageUrl, IsAdminCreated, StatusId
        )
        OUTPUT INSERTED.Id
        VALUES (
            @Name, @Description, @Price, @NumOfPages, @PublishDate, @UserId, @CategoryId, 
            @ImageUrl, 0, 1 -- IsAdminCreated = 0, StatusId = 1 (Pending)
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

            int bookId = (int)cmd.ExecuteScalar();

            // Save the image if uploaded
            if (fuBookImage.HasFile)
            {
                try
                {
                    string folderPath = Server.MapPath($"~/Content/Images/{bookId}/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = Path.GetFileName(fuBookImage.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);
                    fuBookImage.SaveAs(fullPath);

                    // Update the ImageUrl in the database
                    string finalImageUrl = $"Content/Images/{bookId}/{fileName}";

                    string updateQuery = "UPDATE Books SET ImageUrl = @ImageUrl WHERE Id = @Id";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, Conn);
                    updateCmd.Parameters.AddWithValue("@ImageUrl", finalImageUrl);
                    updateCmd.Parameters.AddWithValue("@Id", bookId);
                    updateCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Book saved, but image failed to save: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.OrangeRed;
                    return;
                }
            }

            close_conn();

            ResetForm();
            LoadBooks();
            int copiesForSale = string.IsNullOrEmpty(txtCopiesForSale.Text) ? 0 : int.Parse(txtCopiesForSale.Text);
            int copiesForBorrow = string.IsNullOrEmpty(txtCopiesForBorrow.Text) ? 0 : int.Parse(txtCopiesForBorrow.Text);

            InsertBookCopies(bookId, copiesForSale, copiesForBorrow);
            lblMessage.Text = "Book added successfully. It will be reviewed before being published.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }
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

        private void InsertBookCopies(int bookId, int copiesForSale, int copiesForBorrow)
        {
            if (copiesForSale > 0)
            {
                open_conn();
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
    }
}