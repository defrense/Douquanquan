using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Diagnostics;

namespace AirlineMatrix
{
    public partial class ALMatrix : Form
    {
        public ALMatrix()
        {
            InitializeComponent();
        }

        private void ALMatrix_Load(object sender, EventArgs e)
        {
            textBox_excel_path.Text = "C:\\Phantomjs\\casperjs-1.1.4-1\\casperjs-1.1.4-1\\bin";
            textBox_date.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        private void button_OK_Click(object sender, EventArgs e)
        {

            Form_Airline_Matrix.ReadFillSheetbyChecking(textBox_excel_path.Text, int.Parse(textBox_row_begin.Text), int.Parse(textBox_row_end.Text), 
                                                            int.Parse(textBox_col_begin.Text), int.Parse(textBox_col_end.Text), textBox_date.Text);


        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowDialog();
            textBox_excel_path.Text = folderBrowserDialog.SelectedPath;
        }
    }

    public class Form_Airline_Matrix
    {
        //define variables

        //read an excel file
        public static void ReadFillSheetbyChecking(string path, int rows_begin, int rows_end, int columns_begin, int columns_end, string date)
        {
            Worksheet sheet = null;
            string departure = "";
            string destination = "";

            if (Directory.Exists(path))
            {
                string path_file = path + "\\AirlineMatrix.xlsx";
                Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                Workbook book = app.Workbooks.Open(path_file);
                sheet = book.Worksheets["sheet1"];
                //sheet is not null, and index numbers are correct.
                if (sheet != null && rows_begin > 0 && columns_begin > 0 && rows_end >= rows_begin && columns_end >= columns_begin)
                {
                    //there is an additional column to record the Chinese name of the city, other than simple code. Thus, starting from 2.
                    for (int i = rows_begin; i < rows_end; i++)
                    {
                        for (int j = columns_begin; j < columns_end; j++)
                        {
                            departure = sheet.Cells[i + 1, 0 + 2].Text;
                            destination = sheet.Cells[0 + 2, j + 1].Text;
                            //rewrite the js file
                            if (i != j)
                            {
                                bool success = Form_Airline_Matrix.Modify_JS_Parameters(departure, destination, date);
                                if (success)
                                {
                                    //run the batch file which actually calls the JS script.
                                    Process prc = System.Diagnostics.Process.Start("C:\\Phantomjs\\casperjs-1.1.4-1\\casperjs-1.1.4-1\\bin\\CheckPath.bat");
                                    prc.WaitForExit();

                                    //read the interim file
                                    StreamReader reader = new StreamReader("C:\\Phantomjs\\casperjs-1.1.4-1\\casperjs-1.1.4-1\\bin\\check_path_interim" +
                                                                           "_" + departure + "_" + destination + ".txt");
                                    string[] arr_path_check = reader.ReadToEnd().Split('\n');
                                    //close reader
                                    reader.Close();
                                    //fill the path_check value (end of the interim file) to the sheet in the memory.
                                    sheet.Cells[i + 1, j + 1] = arr_path_check[arr_path_check.Length - 1];


                                }
                                else
                                {
                                    MessageBox.Show("Modify JS file failed.");
                                }
                            }

                            else
                            {
                                sheet.Cells[i + 1, j + 1] = 0;
                            }
                        }
                    }

                    //save the excel.
                    app.DisplayAlerts = false;
                    book.Save();
                    app.Quit();
                }

                else
                {
                    MessageBox.Show("Issues with the sheet (null, or number of rows/columns)");
                }
            }

        }


        //rewrite the js file, which will be involked later in the thread.
        public static bool Modify_JS_Parameters (string depart, string desti, string date)
        {
            try
            {
                StreamWriter writer = new StreamWriter("C:\\Phantomjs\\casperjs-1.1.4-1\\casperjs-1.1.4-1\\bin\\Involke_check_path.js");
                writer.WriteLine("var scrape = require('./CheckAirlines');");
                writer.WriteLine("scrape.check_path(" + "'" + depart + "'," + "'" + desti + "'," + "'" + date + "');");
                writer.Dispose();

                return true;
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                return false;
            }
        }
    }
}

