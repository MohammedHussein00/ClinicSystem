<%@ Page Title="Manage Contact Messages" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="AdminContacts.aspx.cs" Inherits="LibrarySystem.AdminContacts" %>



<asp:Content ID="Content22" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mt-5">
        <h2>Contact Messages</h2>
        <asp:Label ID="lblMessage" runat="server" CssClass="text-success d-block mb-3"></asp:Label>

        <asp:GridView ID="gvContacts" runat="server" CssClass="table table-bordered table-hover"
            AutoGenerateColumns="False" Width="100%" OnRowCommand="gvContacts_RowCommand">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" />
                <asp:TemplateField HeaderText="Full Name">
                    <ItemTemplate>
                        <%# Eval("FirstName") + " " + Eval("LastName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="PhoneNumber" HeaderText="Phone Number" />
                <asp:BoundField DataField="Message" HeaderText="Message" />
                <asp:BoundField DataField="SubmittedAt" HeaderText="Submitted At" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkDelete" runat="server" Text="Delete" CssClass="text-danger"
                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                            OnClientClick="return confirm('Are you sure you want to delete this message?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>