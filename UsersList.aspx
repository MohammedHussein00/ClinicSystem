
<%@ Page Title="Users List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UsersList.aspx.cs" Inherits="LibrarySystem.UsersList" %>


<asp:Content ID="users1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="mb-3">
        <asp:Button ID="btnShowAll" runat="server" Text="Show All" OnClick="btnShowAll_Click" CssClass="btn btn-primary mr-2" />
        <asp:Button ID="btnShowUsers" runat="server" Text="Show Users" OnClick="btnShowUsers_Click" CssClass="btn btn-secondary mr-2" />
        <asp:Button ID="btnShowPublishers" runat="server" Text="Show Publishers" OnClick="btnShowPublishers_Click" CssClass="btn btn-info" />
        <asp:Button ID="btnAddNew" runat="server" Text="Add New User/Publisher" OnClick="btnAddNew_Click" CssClass="btn btn-success float-right" />
    </div>

    <!-- Add New User/Publisher Form -->
    <div id="formContainer" class="form-container border p-3" runat="server" style="display:none;">
        <h5>Add New User/Publisher</h5>
        <div class="form-group">
            <label>Name:</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Phone:</label>
            <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Email:</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Password:</label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Role:</label>
            <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-control">
                <asp:ListItem Value="2">User</asp:ListItem>
                <asp:ListItem Value="3">Publisher</asp:ListItem>
            </asp:DropDownList>
        </div>
       
        <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" CssClass="btn btn-primary" />
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-danger ml-2" />
    </div>

    <!-- Table to Display Users -->
    <div id="tableContainer" class="table-container" runat="server">
        <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:TemplateField HeaderText="Image">
                    <ItemTemplate>
                        <asp:Image ID="imgUser" runat="server" ImageUrl='<%# Eval("ImageUrl") %>' Width="50px" Height="50px" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>