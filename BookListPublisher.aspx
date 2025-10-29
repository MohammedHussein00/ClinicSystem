<%@ Page Title="Book List" Language="C#" AutoEventWireup="true"  MasterPageFile="~/Site.Master" CodeBehind="BookListPublisher.aspx.cs" Inherits="LibrarySystem.BookListPublisher" %>



<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
        <script type="text/javascript">
            // JavaScript function to preview the selected image
            function previewImage(event) {
                var file = event.target.files[0]; // Get the selected file
                if (file) {
                    var reader = new FileReader(); // Create a FileReader object

                    // Set the onload event to display the image
                    reader.onload = function (e) {
                        var imgPreview = document.getElementById("imgPreview");
                        imgPreview.src = e.target.result; // Set the image source
                        imgPreview.style.display = "block"; // Make the image visible
                    };

                    reader.readAsDataURL(file); // Read the file as a data URL
                }
            }
    </script>
        <asp:HiddenField ID="hfBookId" runat="server" />

&nbsp;<h2 class="mb-4">Manage Books</h2>

        <!-- Book Form -->

<asp:Button ID="btnToggleForm" runat="server" Text="Add New Book" OnClick="btnToggleForm_Click" CssClass="btn btn-primary" />

<asp:Panel ID="Panel1" runat="server" CssClass="row form-container" Visible="false">

        <div class="row">
            <div class="col-md-6 mb-3">
                <label class="form-label">Book Name</label>
                <asp:TextBox ID="txtBookName" runat="server" CssClass="form-control" />
            </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">Price</label>
                <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" TextMode="Number"/>
            </div>
            </div>



             <div class="container mt-5">
            <h2>Upload Image with Preview</h2>

            <!-- File Upload Control -->
<asp:FileUpload ID="fuBookImage" runat="server" CssClass="form-control mb-3" accept="image/*" />

            <!-- Image Preview -->
            <img id="imgPreview" src="#" alt="Image Preview" style="display:none; max-width: 300px; max-height: 300px;" class="mb-3" />

            <!-- Save Button -->

            <!-- Status Message -->
        </div>




            <div class="col-md-6 mb-3">
                <label class="form-label">Description</label>
                <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" />
            </div>

            <div class="col-md-6 mb-3">
                <label class="form-label">Number of Pages</label>
                <asp:TextBox ID="txtNumOfPages" runat="server" CssClass="form-control" TextMode="Number"/>
            </div>
            <div class="col-md-6 mb-3">
                <label class="form-label">Publish Date</label>
                <asp:TextBox ID="txtPublishDate" runat="server" TextMode="Date" CssClass="form-control" />
            </div>
            
            <div class="col-md-6 mb-3">
                <label class="form-label">Category</label>
                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
            </div>
                <div class="col-md-6 mb-3">
               <label class="form-label">Copies for Sale</label>
               <asp:TextBox ID="txtCopiesForSale" runat="server" CssClass="form-control" TextMode="Number" />
           </div>

           <div class="col-md-6 mb-3">
               <label class="form-label">Copies for Borrow</label>
               <asp:TextBox ID="txtCopiesForBorrow" runat="server" CssClass="form-control" TextMode="Number" />
           </div>
            </div>
            <asp:Label ID="lblMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>

</asp:Panel>




<asp:Panel ID="PublisherPanel2" runat="server" Visible="false">
            <asp:Button ID="btnSave" runat="server" Text="Add Book" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:Button ID="btnUpdate" runat="server" Text="Update Book" CssClass="btn btn-warning ms-2" OnClick="btnUpdate_Click" Visible="false" />
</asp:Panel>

        <hr />

<asp:GridView ID="gvBooks" runat="server" CssClass="table table-bordered"
              AutoGenerateColumns="False" OnSelectedIndexChanged="gvBooks_SelectedIndexChanged"
              DataKeyNames="Id">
    <Columns>
        <asp:TemplateField HeaderText="Name">
            <ItemTemplate>
                <asp:LinkButton ID="lnkBookName" runat="server" 
                                Text='<%# Eval("Name") %>' 
                                PostBackUrl='<%# "BookDetailsAdmin.aspx?Id=" + Eval("Id") %>' />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Description" HeaderText="Description" />
        <asp:BoundField DataField="Price" HeaderText="Price" />
        <asp:BoundField DataField="NumOfPages" HeaderText="Pages" />
        <asp:BoundField DataField="PublishDate" HeaderText="Published" DataFormatString="{0:yyyy-MM-dd}" />
        <asp:BoundField DataField="CategoryName" HeaderText="Category" />
         <asp:BoundField DataField="CopiesForSale" HeaderText="Copies For Sale" />
        <asp:BoundField DataField="CopiesForBorrow" HeaderText="Copies For Borrow" />
        <asp:TemplateField HeaderText="Book Status">
            <ItemTemplate>
                <%# GetBookStatus(Eval("BookStatusId")) %>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Last Update Status">
            <ItemTemplate>
                <%# GetLastUpdateStatus(Eval("LastUpdateStatusId"), Eval("LastUpdateDate")) %>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:CommandField ShowSelectButton="true" SelectText="Edit" />
    </Columns>
</asp:GridView>


</asp:Content>


