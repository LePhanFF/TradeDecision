
#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Gui.Tools;
#endregion
namespace NinjaTrader.NinjaScript.AddOns.Ui
{
    public class UiHostWindow : NTWindow
    {
        private TextBlock lastUpdated, bias, dayType, confidence, morph; private Border flag; private TextBox commentary, evidence; private Canvas dpoc;
        public UiHostWindow(){
            Caption="TPO v9.4.3 — Session Monitor"; Width=900; Height=600;
            var grid=new Grid{Margin=new Thickness(10)}; grid.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto}); grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}); grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(2,GridUnitType.Star)}); grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(3,GridUnitType.Star)}); Content=grid;
            var header=new StackPanel{Orientation=Orientation.Horizontal, Margin=new Thickness(0,0,0,10)};
            lastUpdated=new TextBlock{Text="Last Updated (ET): --:--:--", FontWeight=FontWeights.Bold, Margin=new Thickness(0,0,20,0)};
            flag=new Border{Width=14,Height=14,Background=Brushes.Gold,Margin=new Thickness(0,0,6,0)};
            bias=new TextBlock{Text="Bias: neutral",Margin=new Thickness(0,0,20,0)};
            dayType=new TextBlock{Text="Day: -",Margin=new Thickness(0,0,20,0)};
            confidence=new TextBlock{Text="Conf: -",Margin=new Thickness(0,0,20,0)};
            morph=new TextBlock{Text="Morph: low",Foreground=Brushes.DarkOrange};
            header.Children.Add(lastUpdated); header.Children.Add(flag); header.Children.Add(bias); header.Children.Add(dayType); header.Children.Add(confidence); header.Children.Add(morph);
            Grid.SetColumnSpan(header,2); grid.Children.Add(header);
            commentary=new TextBox{AcceptsReturn=true,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
            Grid.SetRow(commentary,1); grid.Children.Add(commentary);
            var right=new Grid{Margin=new Thickness(10,0,0,0)}; right.RowDefinitions.Add(new RowDefinition{Height=new GridLength(2,GridUnitType.Star)}); right.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});
            dpoc=new Canvas{Background=Brushes.Black,Margin=new Thickness(0,0,0,6)}; evidence=new TextBox{AcceptsReturn=true,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
            Grid.SetRow(dpoc,0); Grid.SetRow(evidence,1); right.Children.Add(dpoc); right.Children.Add(evidence);
            Grid.SetColumn(right,1); Grid.SetRow(right,1); grid.Children.Add(right);
        }
        private Brush Flag(string b)=> b=="bullish"?Brushes.LimeGreen: b=="bearish"?Brushes.Red: Brushes.Gold;
        public void UpdateUi(DateTime et,string sym,string biasStr,string dayTypeStr,int conf,string morphRisk,IEnumerable<string> comments,IEnumerable<string> ev,IList<(DateTime et,double price)> trail){
            Dispatcher.BeginInvoke(new Action(()=>{
                lastUpdated.Text="Last Updated (ET): " + et.ToString("HH:mm:ss") + " — " + sym;
                flag.Background=Flag(biasStr); bias.Text="Bias: " + biasStr; dayType.Text="Day: " + dayTypeStr; confidence.Text="Conf: " + conf; morph.Text="Morph: " + morphRisk;
                if(comments!=null){ foreach(var c in comments){ commentary.AppendText("• " + c + Environment.NewLine); commentary.ScrollToEnd(); } }
                if(ev!=null){ evidence.Clear(); foreach(var x in ev) evidence.AppendText("• " + x + Environment.NewLine); }
                DrawTrail(trail);
            }));
        }
        private void DrawTrail(IList<(DateTime et,double price)> trail){
            dpoc.Children.Clear(); if(trail==null || trail.Count<2) return; double min=trail.Min(p=>p.price), max=trail.Max(p=>p.price); if(Math.Abs(max-min)<1e-9){ max+=1; min-=1; }
            double w=dpoc.ActualWidth>0?dpoc.ActualWidth:400, h=dpoc.ActualHeight>0?dpoc.ActualHeight:200;
            var poly=new Polyline{Stroke=Brushes.DeepSkyBlue, StrokeThickness=2.0};
            for(int i=0;i<trail.Count;i++){ double x=(i/Math.Max(1.0, trail.Count-1))*(w-10)+5; double y=h-5-((trail[i].price-min)/(max-min))*(h-10); poly.Points.Add(new System.Windows.Point(x,y)); }
            dpoc.Children.Add(poly);
        }
    }
}
