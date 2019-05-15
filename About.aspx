<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WikipediaDiscography.About" %>

<%@ Register src="AboutTable.ascx" tagname="About" tagprefix="uc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>About JSON Database</title>

    <link rel="icon"       type="image/x-icon" href="http://www.workingweb.info/database/favicon.ico" />
    <link rel="stylesheet" type="text/css"     href="http://www.workingweb.info/database/Site.css"    />
    <link rel="stylesheet" type="text/css"     href="http://www.workingweb.info/database/AboutTable.css"    />

</head>
<body>

    <form id="AboutForm" runat="server">    
        <uc1:About ID="AboutTable" runat="server" />
    </form>

</body>
</html>
