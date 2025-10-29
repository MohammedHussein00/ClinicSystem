<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="LibrarySystem.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .login-container {
            max-width: 400px;
            margin: 100px auto;
            padding: 30px;
            border: 1px solid #ddd;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            background-color: #ffffff;
        }

        .login-title {
            text-align: center;
            font-size: 24px;
            margin-bottom: 25px;
            font-weight: 600;
        }

        .btn-block {
            width: 100%;
        }

        .register-link {
            margin-top: 15px;
            text-align: center;
        }

        .register-link a {
            color: #007bff;
            text-decoration: none;
        }

        .register-link a:hover {
            text-decoration: underline;
        }

        /* ✅ Error message styling */
        .error-message {
            margin-top: 15px;
            text-align: center;
            font-weight: 500;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="login-title">Login</div>

            <div class="form-group">
                <label for="txtuserId">Username or Email</label>
                <asp:TextBox ID="txtuserId" runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtPassword">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click"
                CssClass="btn btn-primary btn-block" />

            <!-- ✅ Error message just below login button -->
            <asp:Label ID="lblError" runat="server" CssClass="error-message text-danger d-block"></asp:Label>

            <div class="register-link">
                Don't have an account? <a href="Register.aspx">Register</a>
            </div>
        </div>
    </form>
</body>
</html>
