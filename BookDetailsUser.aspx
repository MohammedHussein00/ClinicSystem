<%@ Page Title="Book Details" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookDetailsUser.aspx.cs" Inherits="LibrarySystem.BookDetailsUser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .book-label {
            font-weight: bold;
            display: inline-block;
            width: 160px;
        }
        .book-value {
            display: inline-block;
        }
        .book-detail-row {
            margin-bottom: 10px;
        }
        .update-status {
            margin-top: 20px;
        }
        .publisher-books {
            margin-top: 20px;
        }
        .btn-buy, .btn-borrow {
            margin-right: 10px;
        }
    </style>

    <div class="container mt-4">
        <h2 class="mb-4">Book Details</h2>
        <img ID="book_Image" runat="server" alt="" src="" />
        <asp:Panel ID="pnlBookDetails" runat="server">

            <!-- Existing Book Details -->
            <div class="book-detail-row">
                <span class="book-label">Name:</span>
                <span class="book-value">
                    <asp:HyperLink ID="hlName" runat="server" NavigateUrl="#" Text=""></asp:HyperLink>
                </span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Description:</span>
                <span class="book-value"><asp:Label ID="lblDescription" runat="server" /></span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Price:</span>
                <span class="book-value"><asp:Label ID="lblPrice" runat="server" /></span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Number of Pages:</span>
                <span class="book-value"><asp:Label ID="lblPages" runat="server" /></span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Publish Date:</span>
                <span class="book-value"><asp:Label ID="lblPublishDate" runat="server" /></span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Publisher:</span>
                <span class="book-value"><asp:Label ID="lblPublisher" runat="server" /></span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Category:</span>
                <span class="book-value"><asp:Label ID="lblCategory" runat="server" /></span>
            </div>

            <div class="book-detail-row">
                <span class="book-label">Available Copies:</span>
                <span class="book-value"><asp:Label ID="lblAvailableCopies" runat="server" /></span>
            </div>

            <!-- Request Messages -->
            <div class="mt-3">
                <asp:Label ID="lblBuyMessage" runat="server" CssClass="text-success" Visible="false"></asp:Label>
            </div>
            <div class="mt-3">
                <asp:Label ID="lblBorrowMessage" runat="server" CssClass="text-success" Visible="false"></asp:Label>
            </div>
            <!-- Transaction History -->
            <div class="mt-5">
                <h4>Transaction History</h4>
                <asp:GridView ID="gvTransactionHistory" runat="server" AutoGenerateColumns="False" CssClass="table table-striped">
                    <Columns>
                        <asp:BoundField DataField="RequestType" HeaderText="Request Type" />
                        <asp:BoundField DataField="RequestDate" HeaderText="Request Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:TemplateField HeaderText="Return Date">
                            <ItemTemplate>
                                <%# Eval("ReturnDate") != DBNull.Value ? Convert.ToDateTime(Eval("ReturnDate")).ToString("yyyy-MM-dd") : "-" %>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <!-- Buttons for Requests -->
            <div class="mt-4">
                <asp:Button ID="btnRequestBorrow" runat="server" Text="Request to Borrow" CssClass="btn btn-primary btn-borrow"
                    OnClick="btnRequestBorrow_Click" Visible="false" />
                <asp:Button ID="btnRequestBuy" runat="server" Text="Request to Buy" CssClass="btn btn-success btn-buy"
                    OnClick="btnRequestBuy_Click" Visible="false" />
                <asp:Button ID="btnReturnBook" runat="server" Text="Return Book"
                    CssClass="btn btn-warning" OnClick="btnReturnBook_Click" Visible="false" />
            </div>

            <!-- Back Button -->
            <div class="mt-4">
                <asp:Button ID="btnBack" runat="server" Text="Back to Book List" CssClass="btn btn-secondary" OnClick="btnBack_Click" />
            </div>

            <!-- Books by Same Publisher -->
            <asp:GridView ID="gvPublisherBooks" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered">
    <Columns>
        <asp:TemplateField HeaderText="Book Name">
            <ItemTemplate>
                <asp:HyperLink 
                    ID="lnkBookName" 
                    runat="server" 
                    Text='<%# Eval("Name") %>' 
                    NavigateUrl='<%# "BookDetailsUser.aspx?id=" + Eval("Id") %>'>
                </asp:HyperLink>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="Description" HeaderText="Description" />
        <asp:BoundField DataField="Price" HeaderText="Price" />
        <asp:BoundField DataField="NumOfPages" HeaderText="Pages" />
        <asp:BoundField DataField="PublishDate" HeaderText="Publish Date" />
    </Columns>
</asp:GridView>

            </div>

        </asp:Panel>
    </div>
</asp:Content>