<%@ Page Title="Book Details" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookDetailsAdmin.aspx.cs" Inherits="LibrarySystem.BookDetailsAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .book-label { font-weight: bold; width: 160px; display: inline-block; }
        .book-value { display: inline-block; }
        .book-detail-row { margin-bottom: 10px; }
        .update-status { margin-top: 20px; }
        .publisher-books { margin-top: 20px; }
        .request-actions { display: inline-block; }
    </style>

    <div class="container mt-4">
        <h2 class="mb-4">Book Details</h2>
        
        <asp:Panel ID="pnlBookDetails" runat="server">
            <!-- Book Image -->
            <img ID="book_Image" runat="server" alt="Book Cover" class="img-thumbnail mb-3" 
                 style="max-width: 300px; max-height: 400px;" />

            <!-- Book Information -->
            <div class="book-detail-row">
                <span class="book-label">Name:</span>
                <asp:HyperLink ID="hlName" runat="server" CssClass="book-value" />
            </div>
            
            <div class="book-detail-row">
                <span class="book-label">Description:</span>
                <asp:Label ID="lblDescription" runat="server" CssClass="book-value" />
            </div>
            
            <div class="book-detail-row">
                <span class="book-label">Price:</span>
                <asp:Label ID="lblPrice" runat="server" CssClass="book-value" />
            </div>
            
            <div class="book-detail-row">
                <span class="book-label">Pages:</span>
                <asp:Label ID="lblPages" runat="server" CssClass="book-value" />
            </div>
            
            <div class="book-detail-row">
                <span class="book-label">Publish Date:</span>
                <asp:Label ID="lblPublishDate" runat="server" CssClass="book-value" />
            </div>
            
            <div class="book-detail-row">
                <span class="book-label">Publisher:</span>
                <asp:Label ID="lblPublisher" runat="server" CssClass="book-value" />
            </div>
            
            <div class="book-detail-row">
                <span class="book-label">Category:</span>
                <asp:Label ID="lblCategory" runat="server" CssClass="book-value" />
            </div>
  
            <div class="book-detail-row">
    <span class="book-label">Available for Borrow:</span>
    <asp:Label ID="lblBorrowCopies" runat="server" CssClass="book-value" />
</div>
<div class="book-detail-row">
    <span class="book-label">Available for Sale:</span>
    <asp:Label ID="lblSaleCopies" runat="server" CssClass="book-value" />
</div>
<!-- Add this section below the Book Requests GridView -->
<div class="mt-4">
    <h3>Pending Book Updates</h3>
    <asp:GridView 
        ID="gvPendingUpdates" 
        runat="server"
        AutoGenerateColumns="False"
        CssClass="table table-striped table-bordered"
        DataKeyNames="Id"
        OnRowCommand="gvPendingUpdates_RowCommand"
        OnRowDataBound="gvPendingUpdates_RowDataBound">
        <Columns>
            <asp:BoundField DataField="UpdatedName" HeaderText="New Name" />
            <asp:BoundField DataField="CreatedAt" HeaderText="Submitted On" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
            <asp:BoundField DataField="UpdatedPrice" HeaderText="New Price" DataFormatString="{0:C}" />
            <asp:BoundField DataField="UpdatedNumOfPages" HeaderText="New Pages" />
            <asp:BoundField DataField="UpdatedPublishDate" HeaderText="New Publish Date" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <%# GetUpdateStatusLabel(Eval("StatusId")) %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Actions">
                <ItemTemplate>
                    <div class="request-actions">
                        <asp:LinkButton 
                            ID="lnkApproveUpdate" 
                            runat="server" 
                            CommandName="ApproveUpdate" 
                            CommandArgument='<%# Eval("Id") %>'
                            CssClass="btn btn-success btn-sm me-2"
                            OnClientClick='return confirm("Approve this update?");'>
                            Approve
                        </asp:LinkButton>
                        <asp:LinkButton 
                            ID="lnkRejectUpdate" 
                            runat="server" 
                            CommandName="RejectUpdate" 
                            CommandArgument='<%# Eval("Id") %>'
                            CssClass="btn btn-danger btn-sm"
                            OnClientClick='return confirm("Reject this update?");'>
                            Reject
                        </asp:LinkButton>
                    </div>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</div>

            <!-- Book Requests Section -->
            <div class="mt-4">
                <h3>Book Requests</h3>
                <asp:GridView ID="gvBookRequests" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-striped table-bordered"
                    DataKeyNames="Id"
                    OnRowCommand="gvBookRequests_RowCommand"
                    OnRowDataBound="gvBookRequests_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="UserName" HeaderText="User" />
                        <asp:BoundField DataField="RequestType" HeaderText="Type" />
                        <asp:BoundField DataField="RequestDate" HeaderText="Requested On" 
                            DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                            
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <%# GetRequestStatusLabel(Eval("Status")) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:BoundField DataField="ApprovedAt" HeaderText="Confirmed On" 
                            DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                        <asp:BoundField DataField="AdminName" HeaderText="Confirmed By" 
                            NullDisplayText="-" />

                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="request-actions">
                                    <asp:LinkButton ID="lnkApprove" runat="server"
                                        CommandName="Approve"
                                        CommandArgument='<%# Eval("Id") %>'
                                        CssClass="btn btn-success btn-sm me-2"
                                        OnClientClick='return confirm("Approve this request?");'>
                                        Approve
                                    </asp:LinkButton>
                                    
                                    <asp:LinkButton ID="lnkReject" runat="server"
                                        CommandName="Reject"
                                        CommandArgument='<%# Eval("Id") %>'
                                        CssClass="btn btn-danger btn-sm"
                                        OnClientClick='return confirm("Reject this request?");'>
                                        Reject
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <!-- Publisher Books Section -->
            <div class="publisher-books mt-4">
                <h3>Other Books by Publisher</h3>
                <asp:GridView ID="gvPublisherBooks" runat="server" 
                    AutoGenerateColumns="False" 
                    CssClass="table table-bordered">
                    <Columns>
                        <asp:TemplateField HeaderText="Book Name">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkBookName" runat="server" 
                                    Text='<%# Eval("Name") %>'
                                    NavigateUrl='<%# "BookDetailsAdmin.aspx?id=" + Eval("Id") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Description" HeaderText="Description" />
                        <asp:BoundField DataField="Price" HeaderText="Price" 
                            DataFormatString="{0:C}" />
                        <asp:BoundField DataField="NumOfPages" HeaderText="Pages" />
                        <asp:BoundField DataField="PublishDate" HeaderText="Publish Date" 
                            DataFormatString="{0:yyyy-MM-dd}" />
                    </Columns>
                </asp:GridView>
            </div>

            <!-- Action Buttons -->
            <div class="mt-4">
                <asp:Button ID="btnBack" runat="server" Text="Back to Book List" 
                    CssClass="btn btn-secondary" OnClick="btnBack_Click" />
            </div>
        </asp:Panel>
    </div>

</asp:Content>