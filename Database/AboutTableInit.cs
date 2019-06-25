using System.Web.UI;

namespace About
{
    public class AboutTableInit : AboutTableInitBase
    {
        // NOTE: The constructor will pass a reference to the AboutTable object
        // (basically an HTML table) and all the work done in the base class will
        // act upon it.

        // Each application will define its own version of this class and, in this
        // way, I will accomplish a unique About window for each application.

        public AboutTableInit(UserControl AboutTable) : base(AboutTable)
        {
            this.set_cell_group("row1_column1", "<br/>Framework", "dot_net_4_2.jpg", "Microsoft .NET", "4.0");
            this.set_cell_group("row1_column2", "JSON<br/>Database", "asp.jpg", "ASP", "2015");
            this.set_cell_group("row1_column3", "<br/>Implementation", "c_sharp_2010.jpg", "Visual C#", "2015");

            this.set_cell_group("row2_column1", "<br/><br/>Design Pattern", "MVC3.jpg", "MVC", "5");
            this.set_cell_group("row2_column2", "<br/><br/>Storage", "json_db48x48.png", "JSON", "");
            this.set_cell_group("row2_column3", "<br/><br/>Hosting", "godaddy.jpg", "GoDaddy.com", "");

            this.set_cell_group("row3_column1", "<br/>Query<br/>Language", "json_path 60x60.png", "JSON<br/>Path", "");
            this.set_cell_group("row3_column2", "<br/><br/>Implementation", "json_net 81x50.png", "Newtonsoft<br/>Json.NET", "");
            this.set_cell_group("row3_column3", "<br/><br/>Release", "1.png", "", "");

            this.set_cell_group("row4_column1", "<br/><br/>Programming", "programmer.jpg", "Jim Kelleher", "");
            this.set_cell_group("row4_column2", "", "", "", "");
            this.set_cell_group("row4_column3", "", "", "", "");

        }

    }

}






