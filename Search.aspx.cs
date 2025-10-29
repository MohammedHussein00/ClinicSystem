using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class Search : System.Web.UI.Page
    {
        private readonly string connectionString = SiteMaster.oraAuth();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                LoadPublishers();
                DisplayAllBooks();
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Name FROM Categories";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    ddlCategory.DataSource = cmd.ExecuteReader();
                    ddlCategory.DataTextField = "Name";
                    ddlCategory.DataValueField = "Id";
                    ddlCategory.DataBind();
                    ddlCategory.Items.Insert(0, new ListItem("All Categories", ""));
                }
            }
        }

        private void LoadPublishers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT u.Id, u.Name 
                    FROM Users u
                    INNER JOIN UserRoles ur ON u.Id = ur.UserId
                    INNER JOIN Roles r ON ur.RoleId = r.Id
                    WHERE r.Name = 'Publisher'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    ddlPublisher.DataSource = cmd.ExecuteReader();
                    ddlPublisher.DataTextField = "Name";
                    ddlPublisher.DataValueField = "Id";
                    ddlPublisher.DataBind();
                    ddlPublisher.Items.Insert(0, new ListItem("All Publishers", ""));
                }
            }
        }

        private void DisplayAllBooks()
        {
            string query = @"
                SELECT 
                    B.Id, 
                    B.Name AS BookName, 
                    B.ImageUrl, 
                    B.Price, 
                    B.PublishDate, 
                    U.Name AS PublisherName
                FROM Books B
                JOIN Users U ON B.UserId = U.Id
                WHERE B.StatusId = 2";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DisplayResults(dt);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder(@"
                SELECT 
                    B.Id, 
                    B.Name AS BookName, 
                    B.ImageUrl, 
                    B.Price, 
                    B.PublishDate, 
                    U.Name AS PublisherName
                FROM Books B
                JOIN Users U ON B.UserId = U.Id
                WHERE B.StatusId = 2");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                query.Append(" AND B.Name LIKE @Search");
                parameters.Add(new SqlParameter("@Search", "%" + txtSearch.Text.Trim() + "%"));
            }

            if (!string.IsNullOrWhiteSpace(ddlCategory.SelectedValue))
            {
                query.Append(" AND B.CategoryId = @CategoryId");
                parameters.Add(new SqlParameter("@CategoryId", ddlCategory.SelectedValue));
            }

            if (!string.IsNullOrWhiteSpace(ddlPublisher.SelectedValue))
            {
                query.Append(" AND B.UserId = @PublisherId");
                parameters.Add(new SqlParameter("@PublisherId", ddlPublisher.SelectedValue));
            }

            if (!string.IsNullOrWhiteSpace(txtMinPrice.Text))
            {
                query.Append(" AND B.Price >= @MinPrice");
                parameters.Add(new SqlParameter("@MinPrice", decimal.Parse(txtMinPrice.Text)));
            }

            if (!string.IsNullOrWhiteSpace(txtMaxPrice.Text))
            {
                query.Append(" AND B.Price <= @MaxPrice");
                parameters.Add(new SqlParameter("@MaxPrice", decimal.Parse(txtMaxPrice.Text)));
            }

            if (!string.IsNullOrWhiteSpace(txtPublishDate.Text))
            {
                query.Append(" AND CONVERT(date, B.PublishDate) = @PublishDate");
                parameters.Add(new SqlParameter("@PublishDate", DateTime.Parse(txtPublishDate.Text)));
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
            {
                conn.Open();
                cmd.Parameters.AddRange(parameters.ToArray());
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DisplayResults(dt);
            }
        }

        private void DisplayResults(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            if (dt.Rows.Count == 0)
            {
                sb.Append("<div class='alert alert-info mt-4'>");
                sb.Append("No books found matching your search criteria.");
                sb.Append("</div>");
            }
            else
            {
                sb.Append("<div class='row'>");

                foreach (DataRow row in dt.Rows)
                {
                    int bookId = Convert.ToInt32(row["Id"]);
                    string title = row["BookName"].ToString();
                    decimal price = Convert.ToDecimal(row["Price"]);
                    string image = row["ImageUrl"].ToString();
                    string publisher = row["PublisherName"].ToString();
                    DateTime publishDate = Convert.ToDateTime(row["PublishDate"]);

                    sb.Append($@"
                <div class='col-md-3'>
                    <a href='BookDetailsUser.aspx?id={bookId}' style='text-decoration: none; color: inherit;'>
                        <div class='card mb-4'>
                            <img src='{image}' class='card-img-top' style='height: 200px; object-fit: cover;' />
                            <div class='card-body'>
                                <h5 class='card-title'>{title}</h5>
                                <p class='card-text text-muted'>Publisher: {publisher}</p>
                                <p class='card-text text-muted'>Price: ${price:C}</p>
                                <p class='card-text'><small>Published: {publishDate:yyyy-MM-dd}</small></p>
                            </div>
                        </div>
                    </a>
                </div>");
                }

                sb.Append("</div>");
            }

            litResults.Text = sb.ToString();
        }
    }
}