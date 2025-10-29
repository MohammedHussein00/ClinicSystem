<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="LibrarySystem.Contact" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Contact Us</title>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        /* General styles */
        body {
            font-family: Arial, sans-serif;
            background-color: #f8f9fa;
        }

        .contact-container {
            max-width: 600px;
            margin: 100px auto;
            padding: 30px;
            border: 1px solid #ddd;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            background-color: #ffffff;
        }

        .header-bar {
            background-color: #007bff;
            color: white;
            padding: 10px 20px;
            text-align: center;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-control {
            border: 1px solid #ced4da;
            border-radius: 4px;
            padding: 10px;
        }

        .form-control:focus {
            border-color: #80bdff;
            box-shadow: none;
        }

        .submit-btn {
            width: 100%;
            padding: 10px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            font-size: 16px;
            cursor: pointer;
        }

        .submit-btn:hover {
            background-color: #0056b3;
        }

        .error-message {
            color: red;
            margin-top: 10px;
        }
    </style>
</head>
<body>
    <div class="contact-container">
        <!-- Header Bar -->
        <div class="header-bar">
            Contact Us
        </div>
        <br />
        <br />

        <!-- Contact Form -->
        <form id="form1" runat="server">
            <div class="form-group row">
                <div class="col-lg-6 col-md-6 col-sm-12">
                <label >First Name</label>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="Please enter first name..."></asp:TextBox>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-12">
                <label >Last Name</label>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Please enter last name..."></asp:TextBox>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-12">
                <label>Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Please enter email..."></asp:TextBox>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-12">
                <label>Phone Number</label>
                    <asp:TextBox ID="txtPhoneNumber" runat="server" CssClass="form-control" placeholder="Please enter phone number..."></asp:TextBox>
                </div>
            </div>

            <div class="form-group">
                <label>What do you have in mind?</label>
                <asp:TextBox ID="txtQuery" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" placeholder="Please enter your query..."></asp:TextBox>
            </div>

            <div class="form-group">
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" CssClass="submit-btn" />
            </div>

            <!-- Error Message -->
            <asp:Label ID="lblError" runat="server" CssClass="error-message"></asp:Label>
        </form>
    </div>
</body>
</html>