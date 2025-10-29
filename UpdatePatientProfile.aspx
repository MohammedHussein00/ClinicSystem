<%@ Page Title="Update Patient Profile" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="UpdatePatientProfile.aspx.cs"
    Inherits="LibrarySystem.UpdatePatientProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h3 class="text-center mb-4">Update Patient Profile</h3>

        <asp:Panel ID="pnlProfile" runat="server" CssClass="card shadow p-4">
            <div class="row">
                <!-- Left Side -->
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="txtName" class="form-label">Full Name</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                    </div>

                    <div class="mb-3">
                        <label for="txtPhone" class="form-label">Phone</label>
                        <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>

                    <div class="mb-3">
                        <label for="txtDOB" class="form-label">Date of Birth</label>
                        <asp:TextBox ID="txtDOB" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>

                <!-- Right Side -->
                <div class="col-md-6">
                    <div class="mb-3">
                        <label for="fuProfileImage" class="form-label">Profile Image</label>
                        <asp:FileUpload ID="fuProfileImage" runat="server" CssClass="form-control" onchange="previewImage(this)" />
                    </div>

                    <div class="mb-3">
                        <asp:Image ID="imgProfile" runat="server" Width="150" CssClass="img-thumbnail" Visible="false" />
                        <asp:Image ID="imgPreview" runat="server" CssClass="img-thumbnail mt-2" Style="max-width: 150px; display: none;" />
                        <img id="img1" class="img-thumbnail mt-2" style="max-width: 150px; display: none;" />
                    </div>
                </div>
            </div>

            <asp:Button ID="btnUpdate" runat="server" Text="Update Profile" CssClass="btn btn-primary" OnClick="btnUpdate_Click" />
            <asp:Label ID="lblMessage" runat="server" CssClass="text-success mt-3 d-block"></asp:Label>

            <hr />

            <!-- ✅ Change Password Section -->
            <h5>Change Password</h5>
            <div class="mb-3">
                <label for="txtOldPassword" class="form-label">Old Password</label>
                <asp:TextBox ID="txtOldPassword" runat="server" TextMode="Password" CssClass="form-control" />
            </div>

            <div class="mb-3">
                <label for="txtNewPassword" class="form-label">New Password</label>
                <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control" />
            </div>

            <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="btn btn-warning" OnClick="btnChangePassword_Click" />
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function previewImage(input) {
            var preview = document.getElementById("img1");
            var file = input.files[0];
            var reader = new FileReader();

            reader.onload = function (e) {
                preview.src = e.target.result;
                preview.style.display = 'block';
            };

            if (file) {
                reader.readAsDataURL(file);
            }
        }
    </script>
</asp:Content>
