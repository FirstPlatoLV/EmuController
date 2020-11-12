using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmuController.DevManager
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Application
    {
        [STAThread]
        public static void Main()
        {
            using var appLock = new SingleInstanceApplicationLock();
            if (!appLock.TryAcquireExclusiveLock())
                return;

            Start app = new Start();
            app.InitializeComponent();
            app.Run();
        }
    }
}
