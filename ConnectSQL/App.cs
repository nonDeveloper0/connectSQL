#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion

namespace ConnectSQL
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            string tabName = "SQL";
            a.CreateRibbonTab(tabName);
            RibbonPanel sqlPanel = a.CreateRibbonPanel(tabName, "Connect SQL");
            
            string thisAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("cmdConnectSQL", "SQL 연결", thisAssemblyPath, "ConnectSQL.Command");
            PushButton pushButton = sqlPanel.AddItem(buttonData) as PushButton;     // RibbonItem을 PushButton으로 형변환
                        
            pushButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ConnectSQL;component/Resources/sql_32.ico"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {   
            return Result.Succeeded;
        }
    }
}
