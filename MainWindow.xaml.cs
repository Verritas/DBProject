using DBProject.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection connect;

        public MainWindow()
        {
            InitializeComponent();
            GetAllRecords();
        }

        private void InitDB()
        {
            string ConnStr;
            ConnStr = "Data Source=WINDEV2010EVAL; Initial Catalog=StudentsDatabase; Trusted_Connection = True;";
            connect = new SqlConnection(ConnStr);
            connect.Open();
        }

        private void CloseDB()
        {
            if (connect != null && connect.State != ConnectionState.Closed)
            {
                connect.Close();
            }
        }

        private void GetAllRecords()
        {
            InitDB();
            if (connect != null && connect.State == ConnectionState.Open)
            {
                var adapter = new SqlDataAdapter("select * from vStudentsList order by ID;", connect);
                DataTable dataTable = new DataTable();
                DataSet DS = new DataSet();
                adapter.Fill(DS, "table1");
                DBTable.DataContext = DS.Tables[0];
            }
            CloseDB();
        }

        private void TryAddRecord(string fam, int year, double math, double cs, double forlang)
        {
            InitDB();

            SqlCommand cmd = new SqlCommand("dbo.addRecord", connect);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@fam", SqlDbType.VarChar));
            cmd.Parameters["@fam"].Value = fam;
            cmd.Parameters.Add(new SqlParameter("@byear", SqlDbType.Int));
            cmd.Parameters["@byear"].Value = year;
            cmd.Parameters.Add(new SqlParameter("@math", SqlDbType.Float));
            cmd.Parameters["@math"].Value = math;
            cmd.Parameters.Add(new SqlParameter("@cs", SqlDbType.Float));
            cmd.Parameters["@cs"].Value = cs;
            cmd.Parameters.Add(new SqlParameter("@forlang", SqlDbType.Float));
            cmd.Parameters["@forlang"].Value = forlang;

            cmd.ExecuteNonQuery();

            CloseDB();

            GetAllRecords();
        }

        private void tryRemoveRecord(int id)
        {
            InitDB();

            SqlCommand cmd = new SqlCommand("dbo.removeRecord", connect);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@pID", SqlDbType.VarChar));
            cmd.Parameters["@pID"].Value = id;

            cmd.ExecuteNonQuery();

            CloseDB();

            GetAllRecords();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Regex stringRegex = new Regex(@"^[a-zA-Z]+$");
                Regex numberRegex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
                if (!stringRegex.IsMatch(TextBlockFamily.Text)) throw new ErrorAddStudent();
                if (!numberRegex.IsMatch(TextBlockYear.Text)) throw new ErrorAddStudent();
                if (!numberRegex.IsMatch(TextBlockMath.Text)) throw new ErrorAddStudent();
                if (!numberRegex.IsMatch(TextBlockCS.Text)) throw new ErrorAddStudent();
                if (!numberRegex.IsMatch(TextBlockForeign.Text)) throw new ErrorAddStudent();

                TryAddRecord(TextBlockFamily.Text, Convert.ToInt32(TextBlockYear.Text), Convert.ToDouble(TextBlockMath.Text), Convert.ToDouble(TextBlockCS.Text), Convert.ToDouble(TextBlockForeign.Text));
                CaptionAddError.Text = "";
            }
            catch (ErrorAddStudent)
            {
                CaptionAddError.Text = "Error. Please check a data";
            }
            catch(Exception)
            {
                CaptionAddError.Text = "Error on DB";
            }
        }

        private void ButtonRemoveClick(object sender, RoutedEventArgs e)
        {   
            try
            {
                Regex IDRegex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
                if (!IDRegex.IsMatch(TextBlockRemoveID.Text)) throw new ErrorRemoveStudent();

                tryRemoveRecord(Convert.ToInt32(TextBlockRemoveID.Text));

                RemoveIDLabel.Text = "";
            }
            catch(ErrorRemoveStudent)
            {
                RemoveIDLabel.Text = "Error. Please check ID";
            }
            catch(Exception)
            {
                RemoveIDLabel.Text = "Error on DB";
            }
        }

        private void SortTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataView view = ((DataTable)DBTable.DataContext).DefaultView;
            switch (SortTypes.SelectedIndex) {
                case 0:
                    view.Sort = "ID ASC";
                    break;
                case 1:
                    view.Sort = "Fam ASC";
                    break;
                case 2:
                    view.Sort = "byear ASC";
                    break;
                case 3:
                    view.Sort = "math ASC";
                    break;
                case 4:
                    view.Sort = "cs ASC";
                    break;
                case 5:
                    view.Sort = "forLang ASC";
                    break;
            }
            DBTable.DataContext = view;
        }
    }
}
