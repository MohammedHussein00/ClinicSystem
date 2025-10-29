using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class _Default : Page
    {
        SqlConnection Conn;

        protected void Page_Load(object sender, EventArgs e)
        {
        

            if (!IsPostBack)
            {
            }
        }

        protected void open_conn()
        {
            string connectionString = SiteMaster.oraAuth();
            Conn = new SqlConnection(connectionString);
            Conn.Open();
        }
        protected void close_conn()
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
                Conn.Close();
        }

    }
}