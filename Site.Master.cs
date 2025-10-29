using System;
using System.Web;

namespace LibrarySystem
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserNameVal"] is string userName && !string.IsNullOrEmpty(userName))
            {
                loginNavItem.Visible = false;
                registerNavItem.Visible = false;
                userDropdownNavItem.Visible = true;

                dropdownUserName.InnerText = $"{userName}";

                string imageUrl = Session["ImageUrlVal"] as string;
                imgUser.ImageUrl = !string.IsNullOrEmpty(imageUrl) ? imageUrl : "Content/Images/profile1.png";

                string role = Session["UserRoleVal"]?.ToString();
                bool isAdmin = role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
                bool isPublisher = role?.Equals("Publisher", StringComparison.OrdinalIgnoreCase) == true;

                contact.Visible = !isAdmin;
                bookList.Visible = isAdmin;
                usersList.Visible = isAdmin;
                bookListPublisher.Visible = isPublisher;
                PublisherList.Visible = true;
                contacts.Visible = isAdmin;
            }
            else
            {
                contact.Visible = true;
                loginNavItem.Visible = true;
                registerNavItem.Visible = true;
                userDropdownNavItem.Visible = false;
                bookList.Visible = false;
                usersList.Visible = false;
                PublisherList.Visible = false;
                contacts.Visible = false;
            }
        }





        public static string oraAuth()
        {
            //return "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySystem;Trusted_Connection=True;Encrypt=False;";
            return "Server=.;Database=MedicalManagementSystem;Trusted_Connection=True;Encrypt=False;";
        }

       
    }
}