using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MahApps.Metro.Controls.Dialogs;

namespace EVEScanHelper.Classes
{
    public class AboutDialog: BaseMetroDialog
    {
        public Button CloseButton;

        public AboutDialog()
        {
            DialogTop = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"EVE Scan Helper v{MainWindow.VERSION}",
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };

            DialogBottom = CloseButton = new Button { Content = "OK", Width = 100, Margin = new Thickness(5) };            


            var sp = new StackPanel {Orientation = Orientation.Vertical};
            
            var tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"Coded by PantheR `panthernet software`",
            };
            sp.Children.Add(tb);
            tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            tb.Inlines.Clear();
            var run = new Run("Visit our open-source GitHub repository!"); 
            var l2 = new Hyperlink(run) {NavigateUri = new Uri("https://github.com/panthernet/evescanhelper")};
            l2.RequestNavigate += (sender, args) => Process.Start("https://github.com/panthernet/evescanhelper");
            tb.Inlines.Add(l2);
            sp.Children.Add(tb);

            tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            tb.Inlines.Clear();
            run = new Run("Visit out Discord channel!\n"); 
            l2 = new Hyperlink(run) {NavigateUri = new Uri("https://discord.gg/UsnY6UR")};
            l2.RequestNavigate += (sender, args) => Process.Start("https://discord.gg/UsnY6UR");
            tb.Inlines.Add(l2);
            sp.Children.Add(tb);


            

            var sp2 = new StackPanel {Orientation = Orientation.Horizontal};
            var run3 = new Run("Duke Veldspar"); 
            var l = new Hyperlink(run3) {NavigateUri = new Uri("https://zkillboard.com/character/96496243/")};
            l.RequestNavigate += (sender, args) => Process.Start("https://zkillboard.com/character/96496243/");
            var tb2 = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,                
            };
            tb2.Inlines.Clear();
            tb2.Inlines.Add("ISK donations are welcome to ");
            tb2.Inlines.Add(l);
            tb2.Inlines.Add(" character!\n");
            sp2.Children.Add(tb2);

            sp.Children.Add(sp2); 

            Content = sp;
        }
    }
}
