<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterPatient.aspx.cs" Inherits="LibrarySystem.RegisterPatient" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Patient Registration</title>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body {
            background-color: #f5f5f5;
            font-family: 'Segoe UI', sans-serif;
        }

        .register-container {
            max-width: 500px;
            margin: 80px auto;
            padding: 35px;
            border-radius: 10px;
            background-color: #ffffff;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }

        .register-title {
            text-align: center;
            font-size: 26px;
            font-weight: 600;
            margin-bottom: 25px;
            color: #007bff;
        }

        .form-group label {
            font-weight: 500;
        }

        .btn-primary {
            width: 100%;
            padding: 10px;
            font-size: 16px;
        }

        .or-divider {
            text-align: center;
            margin: 15px 0;
            font-weight: bold;
            color: #777;
        }

        .login-link {
            text-align: center;
            margin-top: 10px;
        }

        .text-error {
            color: red;
            text-align: center;
            display: block;
            margin-top: 10px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="register-container">
            <div class="register-title">Register as Patient</div>

            <!-- Full Name -->
            <div class="form-group">
                <label for="txtName">Full Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter your full name" required="true"></asp:TextBox>
            </div>

            <!-- Email -->
            <div class="form-group">
                <label for="txtEmail">Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your email" required="true"></asp:TextBox>
            </div>

            <!-- Phone -->
            <div class="form-group">
                <label for="txtPhone">Phone</label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Phone" placeholder="Enter your phone number"></asp:TextBox>
            </div>

            <!-- Date of Birth -->
            <div class="form-group">
                <label for="txtDOB">Date of Birth</label>
                <asp:TextBox ID="txtDOB" runat="server" CssClass="form-control" TextMode="Date" required="true"></asp:TextBox>
            </div>

            <!-- Password -->
            <div class="form-group">
                <label for="txtPassword">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter your password" required="true"></asp:TextBox>
            </div>

            <!-- Register Button -->
            <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn btn-primary btn-block" OnClick="btnRegister_Click" />

            <div class="or-divider">or</div>

            <!-- Login Redirect -->
            <div class="login-link">
                Already have an account?
                <a href="Login.aspx"> Login here</a>
            </div>

            <!-- Error and Debug Labels -->
            <asp:Label ID="lblError" runat="server" CssClass="text-error"></asp:Label>
            <asp:Label ID="lblDebug" runat="server" CssClass="text-error"></asp:Label>
        </div>
    </form>
</body>
</html>
