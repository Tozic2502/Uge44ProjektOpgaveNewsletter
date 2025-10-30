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

namespace Uge44ProjektOpgaveNewsletter.Views
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class PostWindow : Window
    {
        public string PostSubject { get; private set; } = "";
        public string PostBody { get; private set; } = "";

        public PostWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            PostSubject = SubjectTextBox.Text.Trim();
            PostBody = BodyTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(PostSubject))
            {
                MessageBox.Show("Please enter a subject.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PostBody))
            {
                MessageBox.Show("Please enter a message body.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
