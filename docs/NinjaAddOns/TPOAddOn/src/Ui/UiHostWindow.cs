// UiHostWindow.cs - clean, NT8-safe, matched braces, no top-level code
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Ui
{
    // Code-behind only (no XAML). All UI updates marshaled to Dispatcher.
    public class UiHostWindow : NTWindow
    {
        private TextBlock lastUpdated;
        private TextBlock bias;
        private TextBlock dayType;
        private TextBlock confidence;
        private TextBlock morph;
        private Border flag;
        private TextBox commentary;
        private TextBox evidence;
        private Canvas dpoc;

        private IList<DpocPoint> _trail;

        public UiHostWindow()
        {
            try { this.Caption = "TPO v9.4.3 — Session Monitor"; } catch { /* NT build without Caption */ }
            this.Title = "TPO v9.4.3 — Session Monitor";
            this.Width = 900;
            this.Height = 600;
            this.ResizeMode = ResizeMode.CanResize;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            this.Content = grid;

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            lastUpdated = new TextBlock { Text = "Last Updated (ET): --:--:--", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 20, 0) };
            flag        = new Border { Width = 14, Height = 14, Background = Brushes.Gold, Margin = new Thickness(0, 0, 6, 0) };
            bias        = new TextBlock { Text = "Bias: neutral", Margin = new Thickness(0, 0, 20, 0) };
            dayType     = new TextBlock { Text = "Day: -", Margin = new Thickness(0, 0, 20, 0) };
            confidence  = new TextBlock { Text = "Conf: -", Margin = new Thickness(0, 0, 20, 0) };
            morph       = new TextBlock { Text = "Morph: low", Foreground = Brushes.DarkOrange };

            header.Children.Add(lastUpdated);
            header.Children.Add(flag);
            header.Children.Add(bias);
            header.Children.Add(dayType);
            header.Children.Add(confidence);
            header.Children.Add(morph);
            Grid.SetColumnSpan(header, 2);
            grid.Children.Add(header);

            commentary = new TextBox
            {
                AcceptsReturn = true,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(commentary, 1);
            grid.Children.Add(commentary);

            var right = new Grid { Margin = new Thickness(10, 0, 0, 0) };
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            dpoc = new Canvas { Background = Brushes.Black, Margin = new Thickness(0, 0, 0, 6) };
            dpoc.SizeChanged += (s, e) => RedrawTrail();

            evidence = new TextBox
            {
                AcceptsReturn = true,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            Grid.SetRow(dpoc, 0);
            Grid.SetRow(evidence, 1);
            right.Children.Add(dpoc);
            right.Children.Add(evidence);

            Grid.SetColumn(right, 1);
            Grid.SetRow(right, 1);
            grid.Children.Add(right);
        }

        private static Brush FlagBrush(string biasStr)
        {
            if (string.Equals(biasStr, "bullish", StringComparison.OrdinalIgnoreCase)) return Brushes.LimeGreen;
            if (string.Equals(biasStr, "bearish", StringComparison.OrdinalIgnoreCase)) return Brushes.Red;
            return Brushes.Gold;
        }

        // Safe public API - always marshals to UI thread
        public void UpdateUi(
            DateTime et,
            string sym,
            string biasStr,
            string dayTypeStr,
            int conf,
            string morphRisk,
            IEnumerable<string> comments,
            IEnumerable<string> ev,
            IList<DpocPoint> trail)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                lastUpdated.Text = "Last Updated (ET): " + et.ToString("HH:mm:ss") + " — " + sym;
                flag.Background  = FlagBrush(biasStr);
                bias.Text        = "Bias: " + biasStr;
                dayType.Text     = "Day: " + dayTypeStr;
                confidence.Text  = "Conf: " + conf;
                morph.Text       = "Morph: " + morphRisk;

                if (comments != null)
                {
                    foreach (var c in comments)
                        commentary.AppendText("• " + c + Environment.NewLine);
                    commentary.ScrollToEnd();
                }

                evidence.Clear();
                if (ev != null)
                {
                    foreach (var x in ev)
                        evidence.AppendText("• " + x + Environment.NewLine);
                }

                _trail = trail;
                RedrawTrail();
            });
        }

        private void RedrawTrail()
        {
            dpoc.Children.Clear();
            if (_trail == null || _trail.Count < 2) return;

            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < _trail.Count; i++)
            {
                if (_trail[i].Price < min) min = _trail[i].Price;
                if (_trail[i].Price > max) max = _trail[i].Price;
            }
            if (Math.Abs(max - min) < 1e-9) { max += 1; min -= 1; }

            double w = dpoc.ActualWidth > 0 ? dpoc.ActualWidth : 400;
            double h = dpoc.ActualHeight > 0 ? dpoc.ActualHeight : 200;
            var poly = new Polyline { Stroke = Brushes.DeepSkyBlue, StrokeThickness = 2.0 };

            for (int i = 0; i < _trail.Count; i++)
            {
                double x = (i / Math.Max(1.0, (double)_trail.Count - 1)) * (w - 10) + 5;
                double y = h - 5 - ((_trail[i].Price - min) / (max - min)) * (h - 10);
                poly.Points.Add(new System.Windows.Point(x, y));
            }
            dpoc.Children.Add(poly);
        }
    }
}
