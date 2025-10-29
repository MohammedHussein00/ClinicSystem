<%@ Page Title="Search Page" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="LibrarySystem.Search" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlSearch" runat="server" CssClass="mb-4">
        <div class="row">
            <div class="col-md-3">
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Search by name..." />
            </div>
            <div class="col-md-2">
                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control">
                    <asp:ListItem Text="All Categories" Value="" />
                </asp:DropDownList>
            </div>
            <div class="col-md-2">
                <asp:DropDownList ID="ddlPublisher" runat="server" CssClass="form-control">
                    <asp:ListItem Text="All Publishers" Value="" />
                </asp:DropDownList>
            </div>
            <div class="col-md-2">
                <asp:TextBox ID="txtMinPrice" runat="server" CssClass="form-control" placeholder="Min Price" />
            </div>
            <div class="col-md-2">
                <asp:TextBox ID="txtMaxPrice" runat="server" CssClass="form-control" placeholder="Max Price" />
            </div>
            <div class="col-md-3 mt-2">
                <asp:TextBox ID="txtPublishDate" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-2 mt-2">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary"
                    OnClick="btnSearch_Click" />
            </div>
        </div>
    </asp:Panel>

    <asp:Literal ID="litResults" runat="server" />
</asp:Content>