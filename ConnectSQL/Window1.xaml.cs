using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using Autodesk.Revit.UI;

namespace ConnectSQL
{
    public partial class Window1 : Window
    {
        Document _doc;
        SQLDBConnect sqlConnection = new SQLDBConnect();

        public Window1(Document doc)
        {
            InitializeComponent();

            _doc = doc;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 창이 열릴 때, DB 연결
            sqlConnection.ConnectDB();
        }

        // 테이블 존재 여부 확인 (지정한 DB에 지정한 테이블이 있는지 확인)
        private bool TableExists(string database, string name)
        {
            try 
            {
                string existQuery = "select case when exists((select * FROM [" + database + "].sys.tables " +
                    "WHERE name = '" + name + "')) then 1 else 0 end";
                SqlCommand command = sqlConnection.Query(existQuery);
                return (int)command.ExecuteScalar() == 1;
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return true;    // 오류가 있을 때 true 반환
            }
        }

        private void btnCreateTable_Click(object sender, RoutedEventArgs e)
        {
            bool doesExist = TableExists("TutorialDB", "Adaptives");  // TableExists(dbName, tableName)
            if (doesExist)  // table 존재 할 경우
            {
                TaskDialog.Show("SQL Table Error", "Table already exists");
            }
            else
            {
                try
                {
                    // 테이블 생성 쿼리
                    string tableQuery = "CREATE TABLE Adaptives" +
                        "(UniqueId varchar(255) NOT NULL PRIMARY KEY," +
                        "Family varchar(255)," +
                        "Type varchar(255)," +
                        "Mark varchar(255))";
                    SqlCommand command = sqlConnection.Query(tableQuery);
                    command.ExecuteNonQuery();

                    TaskDialog.Show("Create SQL Table", "table created");
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("SQL Table Error", ex.ToString());
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            IList<Element> adaptives = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .Where(e => e != null && AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance(e as FamilyInstance))
                .ToList();

            string setQuery = "INSERT INTO Adaptives(UniqueId, Family, Type, Mark)" +
                "VALUES(@param1, @param2, @param3, @param4)";

            foreach (Element elem in adaptives)
            {
                FamilyInstance famInst = elem as FamilyInstance;
                string uniqueId = famInst.Id.ToString();
                string familyType = famInst.Symbol.Family.Name; // Family Name
                string familySymbol = famInst.Symbol.Name; // Type Name
                string mark = famInst.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.AsString() ?? "";

                try
                {
                    SqlCommand command = sqlConnection.Query(setQuery);
                    command.Parameters.AddWithValue("@param1", uniqueId);
                    command.Parameters.AddWithValue("@param2", familyType);
                    command.Parameters.AddWithValue("@param3", familySymbol);
                    command.Parameters.AddWithValue("@param4", mark);
                    command.ExecuteNonQuery();

                    TaskDialog.Show("Success", "Export Success!");
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("SQL Export Error", ex.ToString());
                }
            }
        }
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            string getQuery = "SELECT * FROM Adaptives";

            SqlCommand command = sqlConnection.Query(getQuery);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())   // LandXml 리더와 비슷
            {
                string id = reader["UniqueId"].ToString();
                string mark = reader["Mark"].ToString();

                ElementId eId = new ElementId(int.Parse(id));
                Element elem = _doc.GetElement(eId);
                if (elem == null) continue;
                Parameter? markParam = elem.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                using (Transaction tx = new Transaction(_doc, "Update Mark"))
                {
                    tx.Start();
                    markParam.Set(mark);
                    tx.Commit();
                }
            }
            reader.Close();

            TaskDialog.Show("Success", "Import Success!");
            Close();
        }
    }
}
