﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;  // Added (for the database)
using System.IO;  // Added (for the import)
using System.Windows.Forms.DataVisualization.Charting;  // Added for Chart
using System.Globalization;   // Added for csv import
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace _06_Access_Chart_Simple
{

    public partial class Form1 : Form
    {

        public string connString;
        public string queryRaw;
        public string queryDFT;
        public string queryIndex;
        public string queryFrequency;
        public OleDbDataAdapter dAdapterRaw;
        public OleDbDataAdapter dAdapterDFT;
        public OleDbDataAdapter dAdapterIndex;
        public OleDbDataAdapter csvAdapter;
        public OleDbDataAdapter dAdapterFrequency;
        public DataTable dTableRaw;
        public DataTable dTableDFT;
        public DataTable dTableIndex;
        public DataTable dTableFrequency;
        public OleDbCommandBuilder cBuilderRaw;
        public OleDbCommandBuilder cBuilderDFT;
        public OleDbCommandBuilder cBuilderIndex;
        public OleDbCommandBuilder cBuilderFrequency;
        public DataView myDataViewRaw;
        public DataView myDataViewDFT;
        public DataView myDataViewIndex;
        public DataView myDataViewFrequency;
        public BindingSource myBindingSourceRaw;  // added
        public BindingSource myBindingSourceDFT;
        public BindingSource myBindingSourceIndex;
        public BindingSource myBindingSourceFrequency;


        public Form1()
        {
            InitializeComponent();

            this.BackColor = Properties.Settings.Default.myColor;  // used to load saved color on startup
            lblDefault.Text = Properties.Settings.Default.SettingFolder;  // used to show default folder on startup
            lblImportFile.Text = Properties.Settings.Default.SettingFile;  // used to show most recent imported file
            lblAnalyze.Text = Properties.Settings.Default.SettingAnalyze;  // used to show most recent analyzed file
            lblExported.Text = Properties.Settings.Default.SettingExported; // to show most recent exported file
            lblExportedFreq.Text = Properties.Settings.Default.SettingExportedFreq; 
               
            connString = "Provider=Microsoft.ACE.OLEDB.15.0; Data Source=C:\\OpenVibe\\OpenVibe.accdb";  // version Access 2013

            //connString = "Provider=Microsoft.ACE.OLEDB.15.0; Data Source=" + strFullPathToMyFile;  //

            queryRaw = "SELECT * FROM OpenVibeRaw";
            queryDFT = "SELECT * FROM OpenVibeDFT";
            queryIndex = "SELECT Seconds, Amplitude, FrequencyIndex FROM OpenVibeDFT ORDER BY Seconds, Amplitude DESC";
            queryFrequency = "SELECT * FROM Frequency";

            //SELECT OpenVibeDFT.Seconds, OpenVibeDFT.Amplitude, OpenVibeDFT.FrequencyIndex
            // FROM OpenVibeDFT
            //ORDER BY OpenVibeDFT.Seconds, OpenVibeDFT.Amplitude DESC;

            dAdapterRaw = new OleDbDataAdapter(queryRaw, connString);
            dAdapterDFT = new OleDbDataAdapter(queryDFT, connString);
            dAdapterIndex = new OleDbDataAdapter(queryIndex, connString);
            dAdapterFrequency = new OleDbDataAdapter(queryFrequency, connString);
            dTableRaw = new DataTable();
            dTableDFT = new DataTable();
            dTableIndex = new DataTable();
            dTableFrequency = new DataTable();
            cBuilderRaw = new OleDbCommandBuilder(dAdapterRaw);
            cBuilderDFT = new OleDbCommandBuilder(dAdapterDFT);
            cBuilderIndex = new OleDbCommandBuilder(dAdapterIndex);
            cBuilderIndex = new OleDbCommandBuilder(dAdapterFrequency);
            cBuilderRaw.QuotePrefix = "[";   //because database may have spaces in the field names
            cBuilderRaw.QuoteSuffix = "]";   //
            myDataViewRaw = dTableRaw.DefaultView;
            myDataViewDFT = dTableDFT.DefaultView;
            myDataViewIndex = dTableIndex.DefaultView;
            myDataViewIndex = dTableFrequency.DefaultView;
            dAdapterRaw.Fill(dTableRaw);
            dAdapterDFT.Fill(dTableDFT);
            dAdapterIndex.Fill(dTableIndex);
            dAdapterFrequency.Fill(dTableFrequency);
            myBindingSourceRaw = new BindingSource();
            myBindingSourceDFT = new BindingSource();
            myBindingSourceFrequency = new BindingSource();
            myBindingSourceRaw.DataSource = dTableRaw;
            myBindingSourceDFT.DataSource = dTableDFT;
            myBindingSourceFrequency.DataSource = dTableFrequency;
            this.dataGridView1.DataSource = myBindingSourceRaw;
            this.dataGridView2.DataSource = myBindingSourceDFT;
            this.dataGridView3.DataSource = myBindingSourceFrequency;

            lblLines.Text = "Number of Lines: " + dTableRaw.Rows.Count;

            lblSeconds.Text = "Number of Seconds: " + dTableRaw.Rows.Count / 512;

            // In properties, I set dataGridView1:  AutoSizeColumnMode to ColumnHeader (this results in smaller columns)
            //this.dataGridView1.Columns["Time"].DefaultCellStyle.Format = @"hh\:mm:ss";  //same as dataGridView1.Columns[0]
            // for microseconds:"hh:mm:ss.ffff"

            // Create a database connection object using the connection string. 
            OleDbConnection myConnection = new OleDbConnection(connString);

            // Create a database command on the connection using query. 
            OleDbCommand myCommandRaw = new OleDbCommand(queryRaw, myConnection);
            //OleDbCommand myCommandDFT = new OleDbCommand(queryDFT, myConnection);

            // Open the connection. 
            myCommandRaw.Connection.Open();
            //myCommandDFT.Connection.Open();

            // Create a database reader. 
            OleDbDataReader myReader = myCommandRaw.ExecuteReader(CommandBehavior.CloseConnection);
            //OleDbDataReader myReaderDFT = myCommandDFT.ExecuteReader(CommandBehavior.CloseConnection);


            // http://thedevelopertips.com/DotNet/WindowsFormsCSharp/Add-charts-to-Windows-Forms-Application.aspx?Id=10

            // *************************************************************************

            chart1.DataSource = myBindingSourceRaw;     //bndSource is the BindingSource
            // Set the chart title
            this.chart1.Titles.Add("Electrode, Attention, Meditation");

            this.chart1.ChartAreas[0].AxisY.IsLogarithmic = false;


            //this.chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "mm:ss";   // minutes and seconds "mm:ss" Label
            // if you go to Properties, ChartArea, Collections "ChartArea1" is the default name

            //
            chart1.Series["Electrode"].ChartType = SeriesChartType.Line; //chart time (can also be fastline)
            chart1.Series["Electrode"].IsValueShownAsLabel = false;   // To show chart value
            chart1.Series["Electrode"].XValueMember = "Time";
            // in Properties chart1.Series[0].XValueType = Time;
            chart1.Series["Electrode"].YValueMembers = "Electrode";      //which column in database
            chart1.Series["Electrode"].Color = Color.Violet;
            chart1.Series["Electrode"].LegendText = "Electrode";

            //
            chart1.Series["Attention"].ChartType = SeriesChartType.Line; //chart time (can also be fastline)
            chart1.Series["Attention"].IsValueShownAsLabel = false;   // To show chart value
            chart1.Series["Attention"].XValueMember = "Time";
            // in Properties chart1.Series[1].XValueType = Time;
            chart1.Series["Attention"].YValueMembers = "Attention";        //which column in database
            chart1.Series[1].Color = Color.Blue;
            chart1.Series["Attention"].LegendText = "Attention";

            //
            chart1.Series["Meditation"].ChartType = SeriesChartType.Line; //chart time (can also be fastline)
            chart1.Series["Meditation"].IsValueShownAsLabel = false;   // To show chart value
            chart1.Series["Meditation"].XValueMember = "Time";
            chart1.Series["Meditation"].YValueMembers = "Meditation";     //which column in database
            chart1.Series[2].Color = Color.Green;
            chart1.Series["Meditation"].LegendText = "Meditation";
              
            // ***********************************************************************

            // Create a database connection object using the connection string. 
            OleDbConnection myConnection2 = new OleDbConnection(connString);

            String queryDFTChart = queryDFT;

            OleDbCommand myCommandDFTChart = new OleDbCommand(queryDFT, myConnection2);

            myConnection2.Open();

            // set chart data source - the data source must implement IEnumerable
            chartDFT.DataSource = myCommandDFTChart.ExecuteReader(CommandBehavior.CloseConnection);


            //chartDFT.DataSource = myBindingSourceDFT;     //bndSource is the BindingSource
            // Set the chart title
            this.chartDFT.Titles.Add("Amplitude");

            this.chartDFT.ChartAreas[0].AxisY.IsLogarithmic = false;


            //this.chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "mm:ss";   // minutes and seconds "mm:ss" Label
            // if you go to Properties, ChartArea, Collections "ChartArea1" is the default name

            //
            chartDFT.Series["Amplitude"].ChartType = SeriesChartType.Line; //chart time (can also be fastline)
            chartDFT.Series["Amplitude"].IsValueShownAsLabel = false;   // To show chart value
            chartDFT.Series["Amplitude"].XValueMember = "FrequencyIndex";
            // in Properties chart1.Series[0].XValueType = Time;
            chartDFT.Series["Amplitude"].YValueMembers = "Amplitude";      //which column in database
            chartDFT.Series["Amplitude"].Color = Color.Blue;
            chartDFT.Series["Amplitude"].LegendText = "Amplitude";

 
            // ***********************************************************************

            // Create a database connection object using the connection string. 
            OleDbConnection myConnection3 = new OleDbConnection(connString); 

            myConnection3.Open();

            OleDbCommand myCommandFrequency = new OleDbCommand(queryFrequency, myConnection3);

            myBindingSourceFrequency = new BindingSource();
            myBindingSourceFrequency.DataSource = dTableFrequency;
            
            this.chart2.DataSource = myBindingSourceFrequency;
            

            // Set the chart title
            this.chart2.Titles.Add("Frequency per Second");

            this.chart2.ChartAreas[0].AxisY.IsLogarithmic = false;

            //this.chart2.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "mm:ss";   // minutes and seconds "mm:ss" Label
            // if you go to Properties, ChartArea, Collections "ChartArea1" is the default name

            //
            chart2.Series["Frequency"].ChartType = SeriesChartType.Column; //chart time (can also be fastline)
            chart2.Series["Frequency"].IsValueShownAsLabel = false;   // To show chart value
            chart2.Series["Frequency"].XValueMember = "Seconds";
            // in Properties chart1.Series[0].XValueType = Time;
            chart2.Series["Frequency"].YValueMembers = "Frequency";      //which column in database
            chart2.Series["Frequency"].Color = Color.Blue;
            chart2.Series["Frequency"].LegendText = "Frequency";
            //
              
            // Close the connection. 
            myConnection.Close();

            // ***********************************************************************
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'openVibeDataSet.OpenVibeRaw' table. You can move, or remove it, as needed.
            //this.openVibeRawTableAdapter.Fill(this.openVibeDataSet.OpenVibeRaw);

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }


        private void bindingSource2_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void bindingSource1_BindingComplete(object sender, BindingCompleteEventArgs e)
        {

        }



        private void btnCSV2_Click(object sender, EventArgs e)
        {

            // http://www.codeproject.com/Questions/422907/Copy-CSV-records-into-MS-Access-table

            // http://stackoverflow.com/questions/16606753/populating-a-dataset-from-a-csv-file     

            // http://stackoverflow.com/questions/14761952/c-sharp-after-using-openfiledialog-a-messagebox-doesnt-open-on-top

            // Show the dialog and get result.
            OpenFileDialog myOpenFile = new OpenFileDialog();
            myOpenFile.Filter = "CSV Files (*.csv) | *.csv";
            myOpenFile.Title = "Select OpenVibe CSV File";
            myOpenFile.InitialDirectory = lblDefault.Text;   //"C:\\OpenVibe";

            var csvFilename = "";

            if (myOpenFile.ShowDialog() == DialogResult.OK)
            {
                csvFilename = myOpenFile.FileName.ToString();
                lblImportFile.Text = csvFilename;

                Properties.Settings.Default.SettingFile = lblImportFile.Text;
                Properties.Settings.Default.Save();  // to save the file name between sessions

            }

            else return;  // exit the import if file not chosen

            //  CSV Connection

            //var csvFilename = @"c:\OpenVibe\OpenVibeRaw.csv";
            String csvConnString = string.Format(
                @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
                Path.GetDirectoryName(csvFilename));

            var csvConnection = new OleDbConnection(csvConnString);

            csvConnection.Open();
            var csvQuery = "SELECT * FROM [" + Path.GetFileName(csvFilename) + "]";
            var csvAdapter = new OleDbDataAdapter(csvQuery, csvConnection);

            var csvDataset = new DataSet("CSVFile");
            csvAdapter.Fill(csvDataset);

            //MessageBox.Show("Done!");

            DataTable csvTable = csvDataset.Tables[0];

            // Access Database Connection
            DataTable myDataTable = new DataTable();
            dAdapterRaw.Fill(myDataTable);
            OleDbConnection myConnection = new OleDbConnection(connString);
            OleDbCommand myCommand = new OleDbCommand(queryRaw, myConnection);
            myConnection.Open();

            myCommand.CommandText = "DELETE FROM OpenVibeRaw";
            myCommand.ExecuteNonQuery();

             //read each row in the csvTable and insert that record into the Access Database
            for (int s = 0; s < csvTable.Rows.Count; s++)  // normally "int i = 0" but data starts on the second row (1 instead of 0)
            {
                int z = s + 1;
                myCommand.CommandText = "INSERT INTO OpenVibeRaw VALUES ('" + csvTable.Rows[s].ItemArray.GetValue(0) + "'," + csvTable.Rows[s].ItemArray.GetValue(1) + "," + csvTable.Rows[s].ItemArray.GetValue(2) +
                     "," + csvTable.Rows[s].ItemArray.GetValue(3) + "," + z + ")";
                //"," + csvTable.Rows[i].ItemArray.GetValue(9) + ")";               

                lblLines.Text = "Number of Lines: " + z;

                lblSeconds.Text = "Number of Seconds: " + z / 512;

                //MessageBox.Show(myCommand.CommandText);

                myCommand.ExecuteNonQuery();
            }

            myConnection.Close();

            // This is to refresh the DataGrid so that the imported info shows up

            myDataTable = new DataTable();
            dAdapterRaw.Fill(myDataTable);

            // Create a database connection object using the connection string. 
            myConnection = new OleDbConnection(connString);

            // Create a database command on the connection using query. 
            myCommand = new OleDbCommand(queryRaw, myConnection);

            myBindingSourceRaw.DataSource = myDataTable;

            myConnection.Open();

            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = myBindingSourceRaw;

            this.chart1.DataSource = null;
            this.chart1.DataSource = myBindingSourceRaw;   //this is to refresh the chart
            this.chart1.Refresh();
  
            myBindingSourceDFT.DataSource = null;

            this.dataGridView2.DataSource = null; 

            this.dataGridView3.DataSource = null;

            myConnection.Close();

            chart1.Update();            

            MessageBox.Show("Done Importing!");
        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void bntAnalyze_Click(object sender, EventArgs e)
        {

            // Create a database connection object using the connection string. 
            connString = "Provider=Microsoft.ACE.OLEDB.15.0; Data Source=C:\\OpenVibe\\OpenVibe.accdb";  // version Access 2013
            OleDbConnection myConnection = new OleDbConnection(connString);

            // Delete Reader 
            string queryDelete = "DELETE FROM OpenVibeDFT";
            OleDbCommand myCommandDelete = new OleDbCommand(queryDelete, myConnection);
            myCommandDelete.Connection.Open();
            OleDbDataReader myReaderDelete = myCommandDelete.ExecuteReader(CommandBehavior.CloseConnection);
            myConnection.Close();


            // Copy Reader
            string queryCopy = "INSERT INTO OpenVibeDFT SELECT Time, Electrode, Rows FROM OpenVibeRaw";
            OleDbCommand myCommandCopy = new OleDbCommand(queryCopy, myConnection);
            myCommandCopy.Connection.Open();
            OleDbDataReader myReaderCopy = myCommandCopy.ExecuteReader(CommandBehavior.CloseConnection);
            myConnection.Close();


            // FFT (Fast Fourier Transform)
            dTableDFT = new DataTable();
            cBuilderDFT = new OleDbCommandBuilder(dAdapterDFT);
            myDataViewDFT = dTableDFT.DefaultView;
            dAdapterDFT.Fill(dTableDFT);

            int intCount = dTableDFT.Rows.Count;

            int intSeconds = intCount / 512;

            intCount = intSeconds * 512;

            //intCount = 512;  // testing with one second of info

            //object[] myArray = new object[8];  // created an array that can contain 8 objects
            // created it outside of the loop so that it accessible inside and outside of loop

            double[] real = new double[intCount];
            double[] imag = new double[intCount];

            for (int i = 0; i < intCount; i++)
            {
                Object objA = dTableDFT.Rows[i]["Electrode"];
                real[i] = Convert.ToDouble(objA);
            }

            Fft.TransformRadix2(real, imag);

            for (int i = 0; i < intCount; i++)
            {
                dTableDFT.Rows[i]["DFT"] = real[i];      // FFT real
                dTableDFT.Rows[i]["DFTimag"] = imag[i];  // FFT imaginary
                dTableDFT.Rows[i]["Amplitude"] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);  // Amplitude
            }

            dTableDFT.AcceptChanges();

            //******** Filling in Frequency Index *****************

            int intCountFreq = dTableDFT.Rows.Count;
            int intSeconds2 = intCountFreq / 512;
            
            // One loop iteration for each second of data
            for (int i = 0; i < intSeconds2; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    dTableDFT.Rows[j + (i * 512)]["FrequencyIndex"] = j + 1;
                    dTableDFT.Rows[j + (i * 512)]["Seconds"] = i + 1;
                }

                for (int j = 256; j < 512; j++)
                {
                    dTableDFT.Rows[j + (i * 512)]["FrequencyIndex"] = 512 - j;
                    dTableDFT.Rows[j + (i * 512)]["Seconds"] = i + 1;
                }
            }

            dTableDFT.AcceptChanges();

            //*****************************************************
 

            //MessageBox.Show(Convert.ToString(dTableDFT.Rows[2]["Amplitude"]));

            myBindingSourceDFT = new BindingSource();
            myBindingSourceDFT.DataSource = dTableDFT;
            this.dataGridView2.DataSource = myBindingSourceDFT;

            //MessageBox.Show("Updated Dataset");

            // Delete Reader2
            string queryDelete2 = "DELETE FROM OpenVibeDFT";
            OleDbCommand myCommandDelete2 = new OleDbCommand(queryDelete2, myConnection);
            myCommandDelete.Connection.Open();
            OleDbDataReader myReaderDelete2 = myCommandDelete2.ExecuteReader(CommandBehavior.CloseConnection);
   
            myConnection.Close();

            // Copy from Dataset to Database

            //MessageBox.Show("DFT Done, but still needs to be save to database");
                   
            // http://stackoverflow.com/questions/7070011/writing-large-number-of-records-bulk-insert-to-access-in-net-c

            myConnection.Open();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = myConnection;

            int intCount2 = dTableDFT.Rows.Count;
                             //MessageBox.Show(Convert.ToString(intCount));

            for (int i = 0; i < intCount; i++)
            {
                string strTime = Convert.ToString(dTableDFT.Rows[i]["Time"]);
                string strElectrode = Convert.ToString(dTableDFT.Rows[i]["Electrode"]);
                string strDFT = Convert.ToString(dTableDFT.Rows[i]["DFT"]);
                string strDFTimag = Convert.ToString(dTableDFT.Rows[i]["DFTimag"]);
                string strAmplitude = Convert.ToString(dTableDFT.Rows[i]["Amplitude"]);
                string strRows = Convert.ToString(dTableDFT.Rows[i]["Rows"]);
                string strFrequencyIndex = Convert.ToString(dTableDFT.Rows[i]["FrequencyIndex"]);
                string strSeconds = Convert.ToString(dTableDFT.Rows[i]["Seconds"]);

                                     //string queryDFT = "INSERT INTO [OpenVibeDFT] ([Time], Electrode, DFT, [Rows]) VALUES (0, 2, 5.00, 1)";
                string queryDFT = "INSERT INTO [OpenVibeDFT] ([Time], Electrode, DFT, DFTimag, Amplitude,[Rows], FrequencyIndex, Seconds) VALUES (" + strTime + " ," + strElectrode + " ," + strDFT + " ," + strDFTimag + " ," + strAmplitude + " ," + strRows + " ," + strFrequencyIndex + " ," + strSeconds + ")";
                cmd.CommandText = queryDFT;
                cmd.ExecuteNonQuery();
            }

            cmd.Dispose();
                       
            this.chartDFT.DataSource = null;
            this.chartDFT.DataSource = myBindingSourceDFT;   //this is to refresh the chart, but is not working
            this.chartDFT.Refresh();

            lblAnalyze.Text = "Last Analyzed " + lblImportFile.Text;

            Properties.Settings.Default.SettingAnalyze = lblAnalyze.Text;
            Properties.Settings.Default.Save();  // to save the file name between sessions

            chartDFT.Update();
            //chart2.Update();
 
            MessageBox.Show("Done Calculating!");

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            frmAbout About = new frmAbout();
            About.Show();

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

            
        }

  
        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {

            btnColor.PerformClick();   // Background Color Button    

        }

        private void defaultFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {

            btnDefault.PerformClick();

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void lblLines_Click(object sender, EventArgs e)
        {

        }

        private void lblSeconds_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void btnDefault_Click(object sender, EventArgs e)
        {

            // Show the color dialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();     //colorDialog1.ShowDialog();
            // See if user pressed ok.
            if (result == DialogResult.OK)
            {
                // https://msdn.microsoft.com/en-us/library/aa730869.aspx   
                // on saving settings between sessions
                // Go to Solution Explorer, click on Project Properties
                // click on Settings.settings
                // create setting

                // Set label to the selected folder

                lblDefault.Text = folderBrowserDialog1.SelectedPath;

                Properties.Settings.Default.SettingFolder = lblDefault.Text;     
                Properties.Settings.Default.Save();  // to save between sessions
                // lblDefault.Text = Properties.Settings.Default.Setting;  // used to show default folder on startup

            }

        }

        private void lblDefault_Click(object sender, EventArgs e)
        {

        }

        private void setImportExportDefaultFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {

            btnDefault.PerformClick();

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            string folderName;

            folderName = lblDefault.Text;  // the default folder picked by the user


            if (!Directory.Exists(folderName))   // default directory does not exist
            {
                MessageBox.Show("No folder or folder does not exist! \n" +
                                "Please click on the button \n " +
                                "Set Import/Export Default Folder");                  
                return;  // exit
            }

            // **********************************

            string strFullPath = lblAnalyze.Text;

            if (strFullPath == "")
            {
                MessageBox.Show("Please click on the button \n " +
                  "Calculate Frequency");  
                return;  // exit
            }

            string strNewFileName = Path.GetFileNameWithoutExtension(strFullPath);

            string myFileResults = Path.Combine(folderName, strNewFileName + "-FFT-Results.csv");

            string myFileFrequency = Path.Combine(folderName, strNewFileName + "-FFT-Frequency.csv");


            // **********************************

            // Create a database connection object using the connection string. 
            connString = "Provider=Microsoft.ACE.OLEDB.15.0; Data Source=C:\\OpenVibe\\OpenVibe.accdb";  // version Access 2013
            OleDbConnection myConnection = new OleDbConnection(connString);

            dTableDFT = new DataTable();
            cBuilderDFT = new OleDbCommandBuilder(dAdapterDFT);
            myDataViewDFT = dTableDFT.DefaultView;
            dAdapterDFT.Fill(dTableDFT);
 
            // ********************************
            
            //string myFile = folderName + "\\DFT-Results.csv";  // can also use:  @"c:\temp\MyTest.txt";

            //string myFileResults = strNewFullPath;

            string strLabelsDFT = "Time,Electrode,DFT,DFTimag,Amplitude,Rows,FrequencyIndex,Seconds" + "\n";  // "\n" is for new line

            //System.IO.File.Open(myPath1, FileMode.Create);

            File.WriteAllText(myFileResults, strLabelsDFT);

            int intCount = dTableDFT.Rows.Count;

            for (int d = 0; d < intCount; d++)
            {

                string appendText = Convert.ToString(dTableDFT.Rows[d][0]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][1]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][2]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][3]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][4]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][5]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][6]) + "," +
                    Convert.ToString(dTableDFT.Rows[d][7]) + "," +
                    //Convert.ToString(dTableDFT.Rows[i][8]) + "," +
                    //Convert.ToString(dTableDFT.Rows[i][9]) + "," +
                    //Convert.ToString(dTableDFT.Rows[i][10]) + "," +
                    Environment.NewLine;  // Environment.NewLine is so that it appends to the next line
                File.AppendAllText(myFileResults, appendText);
            }


           lblExported.Text = "Exported to: " + myFileResults;           

           Properties.Settings.Default.SettingExported = lblExported.Text;           
           Properties.Settings.Default.Save();  // to save between sessions

           //--------------------------------------------------------

           dTableFrequency = new DataTable();
           cBuilderFrequency = new OleDbCommandBuilder(dAdapterFrequency);
           myDataViewFrequency = dTableFrequency.DefaultView;
           dAdapterFrequency.Fill(dTableFrequency);

           int intSecs = intCount / 512;

           string strFrequencyLabels = "Seconds,Amplitude,Frequency" + "\n";  // "\n" is for new line

           File.WriteAllText(myFileFrequency, strFrequencyLabels);

           for (int f = 0; f < intSecs; f++)
           {
               string appendTextFreq = Convert.ToString(dTableFrequency.Rows[f][0]) + "," +
                   Convert.ToString(dTableFrequency.Rows[f][1]) + "," +
                   Convert.ToString(dTableFrequency.Rows[f][2]) + "," +
                   Environment.NewLine;
               File.AppendAllText(myFileFrequency, appendTextFreq);
           }

           //---------------------------------------------------------

           lblExportedFreq.Text = "And Exported to: " + myFileFrequency;

           Properties.Settings.Default.SettingExportedFreq = lblExportedFreq.Text;
           Properties.Settings.Default.Save();  // to save between sessions

           MessageBox.Show("Exported to: " + myFileResults + "\n" +
                             " and " + "\n" +
                              myFileFrequency);

            System.Diagnostics.Process.Start("explorer.exe", folderName);

            //System.Diagnostics.Process.Start("path to notepad.exe", myFile);

        }

        private void lblExported_Click(object sender, EventArgs e)
        {

        }

        private void chartDFT_Click(object sender, EventArgs e)
        {

        }

        private void btnColor_Click(object sender, EventArgs e)
        {

            // Show folder dialog.
            //DialogResult result = folderBrowserDialog1.ShowDialog();
            // See if user pressed ok.
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                // https://msdn.microsoft.com/en-us/library/aa730869.aspx   
                // on saving settings between sessions
                // Go to Solution Explorer, click on Project Properties
                // click on Settings.settings
                // create myColor

                // Set form background to the selected color.
                this.BackColor = colorDialog1.Color;

                Properties.Settings.Default.myColor = this.BackColor;
                Properties.Settings.Default.Save();  // to save the background color between sessions
                // this.BackColor = Properties.Settings.Default.myColor;  // used to load saved color on startup

            }
            
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void btnFrequency_Click(object sender, EventArgs e)
        {

            // Create a database connection object using the connection string. 
            connString = "Provider=Microsoft.ACE.OLEDB.15.0; Data Source=C:\\OpenVibe\\OpenVibe.accdb";  // version Access 2013
            OleDbConnection myConnection = new OleDbConnection(connString);


            //queryIndex = "SELECT Seconds, Amplitude, FrequencyIndex FROM OpenVibeDFT ORDER BY Seconds, Amplitude DESC";
 
            dTableIndex = new DataTable();
            cBuilderIndex = new OleDbCommandBuilder(dAdapterIndex);
            myDataViewIndex = dTableIndex.DefaultView;
            dAdapterIndex.Fill(dTableIndex);

            // Delete Reader 
            string queryDelete = "DELETE FROM Frequency";
            OleDbCommand myCommandDelete = new OleDbCommand(queryDelete, myConnection);
            myCommandDelete.Connection.Open();
            OleDbDataReader myReaderDelete = myCommandDelete.ExecuteReader(CommandBehavior.CloseConnection);
            myConnection.Close();
           

            //********** Frequency ********************************

 
            myConnection.Open();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = myConnection;

            int intCountIndex = dTableIndex.Rows.Count;
            int intSecondsFreq = intCountIndex / 512;
  
            for (int i = 0; i < intSecondsFreq * 512; i += 512)
            {
                string strSeconds = Convert.ToString(dTableIndex.Rows[i]["Seconds"]);
                string strAmplitude = Convert.ToString(dTableIndex.Rows[i]["Amplitude"]);
                string strFrequency = Convert.ToString(dTableIndex.Rows[i]["FrequencyIndex"]);

                //string queryDFT = "INSERT INTO [OpenVibeDFT] ([Time], Electrode, DFT, [Rows]) VALUES (0, 2, 5.00, 1)";
                string queryFrequency = "INSERT INTO [Frequency] (Seconds, Amplitude, Frequency) VALUES ( " + strSeconds + ", " + strAmplitude + ", " + strFrequency + ")";
                cmd.CommandText = queryFrequency;
                cmd.ExecuteNonQuery();
            }

            cmd.Dispose();

            //--------------------------------------------
            // Refresh the datagrid and chart
            //http://csharp.net-informations.com/datagridview/csharp-datagridview-oledb.htm

            string strFreq = "SELECT * FROM Frequency";
            OleDbDataAdapter myAdapterFreq = new OleDbDataAdapter(strFreq, myConnection);
            DataSet myDatasetFreq = new DataSet();
            myAdapterFreq.Fill(myDatasetFreq, "Frequency");
            myConnection.Close();
            dataGridView3.DataSource = myDatasetFreq;
            dataGridView3.DataMember = "Frequency";
  
         
            this.chart2.DataSource = null;
            this.chart2.DataSource = myDatasetFreq;   //this is to refresh the chart, but is not working ??
            this.chart2.Refresh();
             
            this.chart2.Update();

            //------------------------------------------------------
 
            MessageBox.Show("Done Calculating Frequency");

        }

    }
    
}
