using DataLayerGen.Classes;
using DataLayerGen.DataLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace DataLayerGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<TemplateInfo> _TemplateInfoList = new List<TemplateInfo>();
        private List<ColumnData> ColDataList = new List<ColumnData>();
        private ObservableCollection<ComboBoxItem> TableItems = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> NameColumnItems = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> ActiveColumnItems = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> ModifiedByColumnItems = new ObservableCollection<ComboBoxItem>();

        public MainWindow()
        {
            InitializeComponent();
            LoadTemplateInfo();
            BuildTemplateCheckboxes();
            TableItems = new ObservableCollection<ComboBoxItem>();
            PopulateTableInfo();

            // HACK: Testing code.  To be removed.
            //txtConnStr.Text = @"Data Source=RSAVAGE-DESKTOP\SQLEXPRESS;Initial Catalog=DataLayerGenDB;User ID=TestUser;Password=test123;";
            //txtSaveLocation.Text = @"C:\Users\rsava\Temp";
        }

        #region Events

        /// <summary>
        /// btnExit_Click() - Exit the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// btnGenerate_Click() - Validate the input and generate the output.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            List<string> processMessages = new List<string>();
            string valErrorMsg = ValidateInput();
            if (valErrorMsg != "")
            {
                MessageBox.Show(valErrorMsg, "Request Validation Error", MessageBoxButton.OK);
                return;
            }

            lblConnStr.Content = "";
            lblConnStr.UpdateLayout();

            // Iterated over the selected Templates
            var children = LogicalTreeHelper.GetChildren(panTemplates);
            foreach (var item in children)
            {
                var workItem = item as CheckBox;
                if (workItem.IsChecked == true)
                {
                    lblConsole.Content = $"Processing {workItem.Content}";
                    lblConnStr.UpdateLayout();
                    string result = ProcessCheckedTemplate(workItem.Content.ToString(), ColDataList);
                    if (result != "") { processMessages.Add(result); }
                }
            }

            DisplayResults(processMessages);
        }

        /// <summary>
        /// txtConnStr_LostFocus() - When the Connection String is loses focus, the Table Combo Box 
        /// should be populated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtConnStr_LostFocus(object sender, RoutedEventArgs e)
        {
            PopulateTableInfo();
        }

        /// <summary>
        /// btnConnStrWizard_Click() - Initiates the process to allow the user to create 
        /// the Connection String
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnStrWizard_Click(object sender, RoutedEventArgs e)
        {
            ConnStrWizard cswWindow = new ConnStrWizard();
            cswWindow.ShowDialog();

            if (cswWindow.CloseType == "Cancel") { return; }

            txtConnStr.Text = cswWindow.GetConnectionString();
            PopulateTableInfo();
        }


        /// <summary>
        /// btnSaveFolderDlg_Click() - Handle the Save Folder Button Click by displaying the dialog to select a 
        /// location to Save the output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveFolderDlg_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult dlgResult = dialog.ShowDialog();
                if (dlgResult == WinForms.DialogResult.OK)
                {
                    txtSaveLocation.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// cboTableName_SelectionChanged() - Handle the Table Selection Changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)cboTableName.SelectedItem;
            string selection = (cbi == null) ? "" : cbi.Content.ToString();

            PopulateTableColumnCombos(selection);
        }

        /// <summary>
        /// cboActiveColumn_SelectionChanged() - Handle the Table Selection Changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboActiveColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetActiveInactiveValues();
        }

        #endregion Events

        #region Processing

        /// <summary>
        /// ProcessCheckedTemplate() - Process the selected Template by retrieving the template's
        /// lines and processing them.
        /// </summary>
        /// <param name="templateTitle">Template Title to process</param>
        /// <param name="cdList">List of Column Data</param>
        /// <returns>Result response, blank is success.</returns>
        private string ProcessCheckedTemplate(string templateTitle, List<ColumnData> cdList)
        {
            string result = "";
            ComboBoxItem tableCbi = (ComboBoxItem)cboTableName.SelectedItem;
            ComboBoxItem nameCbi = (ComboBoxItem)cboNameColumn.SelectedItem;
            ComboBoxItem activeCbi = (ComboBoxItem)cboActiveColumn.SelectedItem;
            ComboBoxItem modifiedByCbi = (ComboBoxItem)cboModifiedByColumn.SelectedItem;

            TemplateInfo template = _TemplateInfoList.First(t => t.Title == templateTitle);
            string table = (tableCbi == null) ? "" : tableCbi.Content.ToString();
            string nameCol = GetCboSelectedValue(nameCbi);
            string activeCol = GetCboSelectedValue(activeCbi);
            string activeColDataType = GetColDataType(activeCol);
            string modifiedByCol = GetCboSelectedValue(modifiedByCbi);
            bool isIdentityCol = (chkIsIdentityCol.IsChecked ?? false);

            // TODO: (Future) Overwrite file check (do for all?).

            TemplateProcessor tempProc = new TemplateProcessor(template, cdList, table, txtSaveLocation.Text, txtIdCols.Text, isIdentityCol,  
                                                               nameCol, activeCol, txtActiveValue.Text, txtInactiveValue.Text, activeColDataType,
                                                               modifiedByCol);
            try
            {
                result = tempProc.ProcessTemplate();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        #endregion Processing

        #region Helpers

        /// <summary>
        /// LoadTemplateInfo() - Load the Template Info
        /// </summary>
        private void LoadTemplateInfo()
        {
            string json = System.IO.File.ReadAllText(".\\Templates\\Templates.json");
            _TemplateInfoList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TemplateInfo>>(json);
        }

        /// <summary>
        /// BuildTemplateCheckboxes()
        /// </summary>
        private void BuildTemplateCheckboxes()
        {
            int itemHeight = 20;
            int tabIndex = 100;

            if (_TemplateInfoList.Count == 0)
            {
                string content = "No templates found";
                string name = "lblNoTemplates";
                Label lab = new Label { Content = content, Name = name, Foreground = new SolidColorBrush(Colors.White) };
                panTemplates.Children.Add(lab);
                panTemplates.Height += lab.Height;
                return;
            }

            foreach (TemplateInfo item in _TemplateInfoList)
            {
                string content = item.Title;
                //string name = "temp" + item.TemplateId;
                string name = "tempItem";
                CheckBox chk = new CheckBox { Content = content, Name = name, Height = itemHeight, Foreground = new SolidColorBrush(Colors.White), TabIndex = tabIndex };
                panTemplates.Children.Add(chk);
                panTemplates.Height += (int)chk.Height;
            }
        }

        /// <summary>
        /// ValidateInput() - Validate the request input and populate the error response
        /// </summary>
        /// <returns>Error response, blank is no errors.</returns>
        private string ValidateInput()
        {
            List<string> errorMsg = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool gotTemplate = false;

            if (txtConnStr.Text == "") { errorMsg.Add("Please enter a Connection String"); }
            if (txtSaveLocation.Text == "") { errorMsg.Add("Please select a Save Location"); }
            if (ColDataList.Count == 0) { errorMsg.Add("Please Pick a Table"); }
            IdColumnCheck(errorMsg);
            ValidateActiveInactiveInfo(errorMsg);

            var children = LogicalTreeHelper.GetChildren(panTemplates);
            foreach (var item in children)
            {
                var workItem = item as CheckBox;
                if (workItem.IsChecked == true)
                {
                    gotTemplate = true;
                    break;
                }
            }
            if (!gotTemplate) { errorMsg.Add("Please select at least 1 Template"); }

            if (errorMsg.Count > 0)
            {
                sb.AppendLine("Validation errors occurred:");
                errorMsg.ForEach(em => sb.AppendLine("- " + em));
            }

            return sb.ToString();
        }

        /// <summary>
        /// IdColumnCheck() - Validates that the Id Column(s) exist in the Table
        /// </summary>
        /// <param name="errorMsg">Error Message collection (to be added to if column does not exist)</param>
        private void IdColumnCheck(List<string> errorMsg)
        {
            bool gotError = false;
            string workIdCols = txtIdCols.Text;
            workIdCols = workIdCols.Replace(" ", "");

            char[] charSeparators = new char[] { ';', ',' };
            string[] idCols = workIdCols.Split(charSeparators);

            foreach (string idCol in idCols)
            {
                ColumnData colInList = ColDataList.FirstOrDefault(s => s.ColumnName == idCol);
                if (colInList == null)
                {
                    gotError = true;
                    errorMsg.Add($"Id Column \"{idCol}\" does not exist in table");
                }
            }

            if (gotError == false)
            {
                txtIdCols.Text = workIdCols.Replace('|', ';');
            }
        }

        /// <summary>
        /// ValidateActiveInactiveInfo() - Validates the Active/Inactive entries
        /// </summary>
        /// <param name="errorMsg">Error Message collection (to be added to if error found)</param>
        private void ValidateActiveInactiveInfo(List<string> errorMsg)
        {
            ComboBoxItem activeCbi = (ComboBoxItem)cboActiveColumn.SelectedItem;
            string activeCol = GetCboSelectedValue(activeCbi);

            if (activeCol == "")
            {
                if (txtActiveValue.Text.Trim() != "") { errorMsg.Add("If entering an Active Value, an Active Column must be selected.");  }
                if (txtInactiveValue.Text.Trim() != "") { errorMsg.Add("If entering an Inactive Value, an Active Column must be selected."); }
            }
            else
            {
                if (txtActiveValue.Text.Trim() == "") { errorMsg.Add("An Active Value must be provided."); }
                if (txtInactiveValue.Text.Trim() == "") { errorMsg.Add("An Inactive Value must be provided."); }
            }
        }

        /// <summary>
        /// PopulateTableInfo() - Populate the Table List and load the Tables ComboBox
        /// </summary>
        private void PopulateTableInfo()
        {
            List<TableData> tdList;
            string defaultSelection = LoadTableList(out tdList);

            // Create and add the default item
            TableItems = new ObservableCollection<ComboBoxItem>();
            var defaultItem = new ComboBoxItem { Content = defaultSelection };
            TableItems.Add(defaultItem);

            foreach (TableData tdItem in tdList)
            {
                TableItems.Add(new ComboBoxItem { Content = tdItem.TableName });
            }

            cboTableName.ItemsSource = TableItems;
            cboTableName.SelectedIndex = 0;
        }

        /// <summary>
        /// LoadTableList() - Load the list of Table Names
        /// </summary>
        /// <param name="tdList">(Output) List to be loaded</param>
        /// <returns>Initial Select Item (Caption)</returns>
        private string LoadTableList(out List<TableData> tdList)
        {
            string defaultSelection = "<-- No Tables Available -->";
            tdList = new List<TableData>();
            try
            {
                if (txtConnStr.Text != "")
                {
                    // Load Columns fpr requested Table
                    TableDataDl dl = new TableDataDl();
                    tdList = dl.ListTableData(txtConnStr.Text);
                    if (tdList.Count > 0)
                    {
                        defaultSelection = "<--Select-->";
                    }
                }
            }
            catch (Exception)
            {
                defaultSelection = "<-- DB Connection Issue -->";
            }

            return defaultSelection;
        }

        /// <summary>
        /// PopulateTableColumnCombos() - Populate the Column Data and load the 
        /// Name Column and Active Column ComboBox.
        /// </summary>
        /// <param name="tableName">Table Name to retrieve info for.</param>
        private void PopulateTableColumnCombos(string tableName)
        {
            ClearColumnComboBoxes();
            cboNameColumn.ItemsSource = null;
            cboActiveColumn.ItemsSource = null;
            cboModifiedByColumn.ItemsSource = null;

            if (tableName == "") { return; }

            string defaultSelection = LoadColumnData(tableName);

            // Create and add the default item
            var defaultItem = new ComboBoxItem { Content = defaultSelection };
            NameColumnItems.Add(defaultItem);
            ActiveColumnItems.Add(defaultItem);
            ModifiedByColumnItems.Add(defaultItem);

            foreach (ColumnData colItem in ColDataList)
            {
                NameColumnItems.Add(new ComboBoxItem { Content = colItem.ColumnName });
                ActiveColumnItems.Add(new ComboBoxItem { Content = colItem.ColumnName });
                ModifiedByColumnItems.Add(new ComboBoxItem { Content = colItem.ColumnName });
            }

            cboNameColumn.ItemsSource = NameColumnItems;
            cboActiveColumn.ItemsSource = ActiveColumnItems;
            cboModifiedByColumn.ItemsSource = ModifiedByColumnItems;
            cboNameColumn.SelectedIndex = 0;
            cboActiveColumn.SelectedIndex = 0;
            cboModifiedByColumn.SelectedIndex = 0;

            txtIdCols.Text = "";
            chkIsIdentityCol.IsChecked = true;
            ResetActiveInactiveValues();
        }

        /// <summary>
        /// ClearColumnComboBoxes() - Clear out/Reset the Name Column and Active Column
        /// Combo Boxes;
        /// </summary>
        private void ClearColumnComboBoxes()
        {
            NameColumnItems.Clear();
            ActiveColumnItems.Clear();
            ModifiedByColumnItems.Clear();

            cboNameColumn.ItemsSource = NameColumnItems;
            cboActiveColumn.ItemsSource = ActiveColumnItems;
            cboModifiedByColumn.ItemsSource = ModifiedByColumnItems;
        }

        /// <summary>
        /// ResetActiveInactiveValues() - Resets the Active/Inactive values
        /// </summary>
        private void ResetActiveInactiveValues()
        {
            txtActiveValue.Text = "";
            txtInactiveValue.Text = "";
        }

        /// <summary>
        /// LoadColumnData() - Load the Column information for the requested Table
        /// </summary>
        /// <param name="tableName">Table Name to retrieve info for.</param>
        /// <returns>Initial Select Item (Caption)</returns>
        private string LoadColumnData(string tableName)
        {
            string defaultSelection = "<-- No Columns Available -->";
            var tableNameInfo = tableName.Split('.');

            if (txtConnStr.Text == "" || tableNameInfo.Length != 2)
            {
                ColDataList.Clear();
                return defaultSelection;
            }

            try
            {
                ColumnDataDl dl = new ColumnDataDl();
                ColDataList = dl.ListColumnData(txtConnStr.Text, tableNameInfo[0], tableNameInfo[1]);
                if (ColDataList.Count > 0)
                {
                    defaultSelection = "<--Select-->";
                }
            }
            catch (Exception)
            {
                defaultSelection = "<-- DB Connection Issue -->";
            }

            return defaultSelection;
        }

        /// <summary>
        /// DisplayResults() - Displays the processing results.
        /// </summary>
        /// <param name="processMessages">Messages generated during processing.</param>
        private void DisplayResults(List<string> processMessages)
        {
            if (processMessages.Count == 0)
            {
                string resultMsg = "Processing completed successfully";
                lblConsole.Content = resultMsg;
                MessageBox.Show(resultMsg, "Processing Completed", MessageBoxButton.OK);
            }
            else
            {
                string resultMsg = "Processing completed with errors";
                lblConsole.Content = resultMsg;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{resultMsg}:");
                processMessages.ForEach(pm => sb.AppendLine("- " + pm));
                MessageBox.Show(sb.ToString(), "Processing Completed with Error", MessageBoxButton.OK);
            }
            lblConnStr.UpdateLayout();
        }

        /// <summary>
        /// GetColDataType() - Retrieves the Active Column's data type
        /// </summary>
        /// <param name="activeCol">Active Column</param>
        /// <returns>Active Column's data type (blank if irrelevant or not found)</returns>
        private string GetColDataType(string activeCol)
        {
            if (activeCol == "") { return ""; }
            
            ColumnData colDataItem = ColDataList.FirstOrDefault(s => s.ColumnName == activeCol);
            if (colDataItem == null)
            {
                return "";
            }

            return colDataItem.DataType;
        }

        /// <summary>
        /// GetCboSelectedValue() - Retrueves the Selected Combo Box item.  If the 
        /// selected value is "<--Select-->", return blank.
        /// </summary>
        /// <param name="workCbi">Combo Box Item to analyze</param>
        /// <returns>Selected value or blank</returns>
        private string GetCboSelectedValue(ComboBoxItem workCbi)
        {
            string result = (workCbi == null) ? "" : workCbi.Content.ToString();
            result = (result == "<--Select-->") ? "" : result;
            return result;
        }

        #endregion Helpers

    }
}
