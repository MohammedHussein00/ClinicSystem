<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterDoctor.aspx.cs" Inherits="LibrarySystem.RegisterDoctor" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Doctor Registration</title>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body {
            background-color: #f5f5f5;
            font-family: 'Segoe UI', sans-serif;
        }

        .register-container {
            max-width: 520px;
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
            <div class="register-title">Register as Doctor</div>

            <!-- Full Name -->
            <div class="form-group">
                <label for="txtName">Full Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter your full name"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" 
                    ErrorMessage="Name is required." CssClass="text-error" Display="Dynamic" />
            </div>

            <!-- Date of Birth -->
            <div class="form-group">
                <label for="txtDob">Date of Birth</label>
                <asp:TextBox ID="txtDob" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvDob" runat="server" ControlToValidate="txtDob" 
                    ErrorMessage="Date of Birth is required." CssClass="text-error" Display="Dynamic" />
            </div>

            <!-- Email -->
            <div class="form-group">
                <label for="txtEmail">Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your email"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                    ErrorMessage="Email is required." CssClass="text-error" Display="Dynamic" />
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                    ErrorMessage="Invalid email format." CssClass="text-error" Display="Dynamic"
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" />
            </div>

            <!-- Phone -->
            <div class="form-group">
                <label for="txtPhone">Phone</label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Phone" placeholder="Enter your phone number"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone" 
                    ErrorMessage="Phone number is required." CssClass="text-error" Display="Dynamic" />
                <asp:RegularExpressionValidator ID="revPhone" runat="server" ControlToValidate="txtPhone"
                    ErrorMessage="Invalid phone number format (10–15 digits)." CssClass="text-error" Display="Dynamic"
                    ValidationExpression="^\d{10,15}$" />
            </div>

            <!-- Password -->
            <div class="form-group">
                <label for="txtPassword">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter your password"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" 
                    ErrorMessage="Password is required." CssClass="text-error" Display="Dynamic" />
                <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword"
                    ErrorMessage="Password must be at least 6 characters." CssClass="text-error" Display="Dynamic"
                    ValidationExpression="^.{6,}$" />
            </div>

            <!-- Speciality Dropdown -->
            <div class="form-group">
                <label for="ddlSpeciality">Speciality</label>
                <asp:DropDownList ID="ddlSpeciality" runat="server" CssClass="form-control">
                    <asp:ListItem Text="-- Select Speciality --" Value="" />
                    <asp:ListItem Text="Cardiologist" Value="Cardiologist" />
                    <asp:ListItem Text="Dermatologist" Value="Dermatologist" />
                    <asp:ListItem Text="Dentist" Value="Dentist" />
                    <asp:ListItem Text="Neurologist" Value="Neurologist" />
                    <asp:ListItem Text="Pediatrician" Value="Pediatrician" />
                    <asp:ListItem Text="Ophthalmologist" Value="Ophthalmologist" />
                    <asp:ListItem Text="Orthopedic Surgeon" Value="Orthopedic Surgeon" />
                    <asp:ListItem Text="Psychiatrist" Value="Psychiatrist" />
                    <asp:ListItem Text="Radiologist" Value="Radiologist" />
                    <asp:ListItem Text="General Practitioner" Value="General Practitioner" />
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvSpeciality" runat="server" ControlToValidate="ddlSpeciality" 
                    InitialValue="" ErrorMessage="Please select a speciality." CssClass="text-error" Display="Dynamic" />
            </div>

            <!-- Location -->
            <div class="form-group">
                <label for="txtLocation">Location</label>
                <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control" placeholder="Enter your clinic or hospital location"></asp:TextBox>
            </div>

            <!-- Register Button -->
            <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn btn-primary btn-block" OnClick="btnRegister_Click" />

            <div class="or-divider">or</div>

            <!-- Login Redirect -->
            <div class="login-link">
                <a href="Login.aspx">Already have an account? Login here</a>
            </div>

            <!-- Error Labels -->
            <asp:Label ID="lblError" runat="server" CssClass="text-error"></asp:Label>
        </div>
    </form>
</body>
</html>
