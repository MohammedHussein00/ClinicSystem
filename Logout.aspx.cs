using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["UserNameVal"] = "";
            Session["UserIdVal"] = "";
            Session["UserRoleVal"] = "";
            Session["UserRoleVal"] = "";
            Session["ImageUrlVal"] = "";
            Response.Redirect("Default.aspx");

        }
    }
}