<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="LibrarySystem.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register</title>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .register-container {
            max-width: 450px;
            margin: 100px auto;
            padding: 30px;
            border: 1px solid #ddd;
            border-radius: 8px;
            background-color: #ffffff;
        }
        .register-title {
            text-align: center;
            font-size: 24px;
            margin-bottom: 25px;
            font-weight: 600;
        }
        .btn-block {
            width: 100%;
        }
        .or-divider {
            text-align: center;
            margin: 15px 0;
            font-weight: bold;
            color: #777;
        }
        .login-link {
            text-align: center;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="register-container">
            <div class="register-title">Create an Account</div>

    

              <div class="form-group">
                <label for="txtName">Name</label>
                <asp:TextBox 
                    ID="txtName" 
                    runat="server" 
                    CssClass="form-control" 
                    placeholder="Enter your name" 
                    TextMode="SingleLine" 
                    required="true">
                </asp:TextBox>
                <small class="form-text text-muted">Please enter your full name.</small>
            </div>
            <div class="form-group">
                <label for="txtEmail">Email or username</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
            </div>
            <div class="form-group">
                <label for="txtEmail">Phone</label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Phone"></asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtPassword">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
            </div>

            <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn btn-primary btn-block" OnClick="btnRegister_Click" />

            <div class="or-divider">or</div>

            <div class="login-link">
                <a href="Login.aspx">Already have an account? Login</a>
            </div>
        </div>

        <asp:Label ID="lblDebug" runat="server" ForeColor="Red"></asp:Label>
            <br />

            <!-- Error Label -->
            <asp:Label ID="lblError" runat="server" ForeColor="Red"></asp:Label>
    </form>
</body>
</html>
