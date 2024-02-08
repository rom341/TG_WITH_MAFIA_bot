using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TgWithMafia
{
    public partial class fMain : Form
    {
        DatabaseController databaseController;
        BotController botController;
        GameController gameController;
        public fMain()
        {
            InitializeComponent();
            new AppConfigController();//Считываем данные из конфига

            databaseController = new DatabaseController();
            Task.Delay(1000).Wait();
            botController = new BotController();
            gameController = new GameController();
            test1();
        }
        void test1()
        {
            var resultTable = databaseController.ExecuteSqlQuery("SELECT * FROM `Users`");
            rtbDebug.Text = "";
            foreach (DataRow row in resultTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    rtbDebug.Text += item + "\t";
                }
                rtbDebug.Text += "\n---------\n";
            }
        }
    }
}
