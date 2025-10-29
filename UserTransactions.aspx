<%@ Page Title="My Book Requests" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="UserTransactions.aspx.cs" Inherits="LibrarySystem.UserTransactions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h2 class="mb-4">My Book Requests</h2>
        <asp:Label ID="lblNoRequests" runat="server" CssClass="text-info" Visible="false">
            You have not submitted any book requests yet.
        </asp:Label>

        <asp:GridView ID="gvRequests" runat="server" AutoGenerateColumns="False"
            CssClass="table table-hover" OnRowDataBound="gvRequests_RowDataBound" DataKeyNames="Id">
            <Columns>
                <asp:TemplateField HeaderText="Cover">
                    <ItemTemplate>
                        <img src='<%# Eval("ImageUrl") %>' alt="Book Cover" style="width:80px;" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Title">
                    <ItemTemplate>
                        <asp:HyperLink runat="server" Text='<%# Eval("BookName") %>'
                            NavigateUrl='<%# "BookDetailsUser.aspx?id=" + Eval("BookId") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:BoundField DataField="RequestType" HeaderText="Request Type" />
                <asp:BoundField DataField="RequestDate" HeaderText="Request Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span id="lblStatus" runat="server"></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Return Date">
                    <ItemTemplate>
                        <%# Eval("ReturnDate") != DBNull.Value ? Convert.ToDateTime(Eval("ReturnDate")).ToString("yyyy-MM-dd") : "-" %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <div class="mt-4">
            <asp:Button ID="btnBack" runat="server" Text="Back to Home" CssClass="btn btn-secondary"
                OnClick="btnBack_Click" />
        </div>
    </div>
</asp:Content>