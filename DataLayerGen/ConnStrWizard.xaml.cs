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

namespace DataLayerGen
{
    /// <summary>
    /// Interaction logic for ConnStrWizard.xaml
    /// </summary>
    public partial class ConnStrWizard : Window
    {
        public string CloseType { get; set; }
        public string Server { get; set; }
        public string InitialCatalog { get; set; }
        public string AuthType { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// ConnStrWizard() - Default Constructor
        /// </summary>
        public ConnStrWizard()
        {
            CloseType = "";
            Server = "";
            InitialCatalog = "";
            AuthType = "";
            UserId = "";
            Password = "";
            InitializeComponent();
            ShowHideUserInfo();

            // HACK: Testing code.  To be removed.
            //txtServer.Text = @"RSAVAGE-DESKTOP\SQLEXPRESS";
            //txtInitialCatalog.Text = "DataLayerGenDB";
            //txtUserId.Text = "TestUser";
            //txtPassword.Password = "test123";
            //radServer.IsChecked = true;
            //ShowHideUserInfo();
        }

        #region Events

        /// <summary>
        ///  btnCancel_Click() - Cancels the wizard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseType = "Cancel";
            this.Close();
        }

        /// <summary>
        /// btnSave_Click() - Validated the connection string and returns control to the 
        /// calling window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string valErrorMsg = ValidateInput();
            if (valErrorMsg != "")
            {
                MessageBox.Show(valErrorMsg, "Request Validation Error", MessageBoxButton.OK);
                return;
            }

            SetProperties();
            CloseType = "Save";
            this.Close();
        }

        /// <summary>
        /// radWindows_Checked() - Handle the "Windows Authentication Radio Button check.
        /// Basically to Hide/Show User Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radWindows_Checked(object sender, RoutedEventArgs e)
        {
            ShowHideUserInfo();
        }

        /// <summary>
        /// radServer_Checked() - Handle the "Windows Authentication Radio Button check.
        /// Basically to Hide/Show User Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radServer_Checked(object sender, RoutedEventArgs e)
        {
            ShowHideUserInfo();
        }

        #endregion Events

        #region Helpers

        /// <summary>
        /// ShowHideUserInfo() - Shows or hides the User ID/Password info based 
        /// on the Authertication Type.
        /// </summary>
        private void ShowHideUserInfo()
        {
            if (radWindows.IsChecked == true)
            {
                lblUserId.Visibility = Visibility.Hidden;
                txtUserId.Visibility = Visibility.Hidden;
                lblPassword.Visibility = Visibility.Hidden;
                txtPassword.Visibility = Visibility.Hidden;
            }
            else
            {
                lblUserId.Visibility = Visibility.Visible;
                txtUserId.Visibility = Visibility.Visible;
                lblPassword.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// GetConnectionString() - Return a formatted Connection String based ot this 
        /// object's properties.
        /// </summary>
        /// <returns>Connection String</returns>
        public string GetConnectionString()
        {
            string connStr = $"Data Source={Server};Initial Catalog={InitialCatalog};";
            if (radWindows.IsChecked == true)
            {
                connStr += "Integrated Security=SSPI;";

            }
            else
            {
                connStr += $"User ID={UserId};Password={Password};";
            }

            return connStr;
        }

        /// <summary>
        /// SetProperties() - Set the Connection String Properties
        /// </summary>
        private void SetProperties()
        {
            Server = txtServer.Text;
            InitialCatalog = txtInitialCatalog.Text;
            if (radWindows.IsChecked == true)
            {
                AuthType = "Windows";
                UserId = "";
                Password = "";
            }
            else
            {
                AuthType = "Windows";
                UserId = txtUserId.Text;
                Password = txtPassword.Password;
            }
        }

        /// <summary>
        /// ValidateInput() - Validate the Connection String details.
        /// </summary>
        /// <returns>String containing the formatted output.  Blank if no errors.</returns>
        private string ValidateInput()
        {
            List<string> errorMsg = new List<string>();
            StringBuilder sb = new StringBuilder();

            if (txtServer.Text == "") { errorMsg.Add("Please enter a Server Name"); }
            if (txtInitialCatalog.Text == "") { errorMsg.Add("Please enter an Initial Catalog"); }

            if (radServer.IsChecked == true)
            {
                if (txtUserId.Text == "") { errorMsg.Add("Please enter a User Id"); }
                if (txtPassword.Password == "") { errorMsg.Add("Please enter a Password"); }
            }

            if (errorMsg.Count > 0)
            {
                sb.AppendLine("Validation errors occurred:");
                errorMsg.ForEach(em => sb.AppendLine("- " + em));
            }

            return sb.ToString();
        }

        #endregion Helpers
    }
}
