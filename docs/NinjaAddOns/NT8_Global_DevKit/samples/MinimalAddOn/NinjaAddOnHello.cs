
#region Using declarations
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
#endregion
namespace NinjaTrader.NinjaScript.AddOns
{
    public class NinjaAddOnHello : AddOnBase
    {
        private NTMenuItem menuItem;
        protected override void OnWindowCreated(System.Windows.Window w){
            var cc = w as ControlCenter; if (cc==null) return;
            var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem; if (newMenu==null) return;
            if(menuItem==null){ menuItem = new NTMenuItem{ Header="Hello Add-On" }; newMenu.Items.Add(menuItem); }
        }
        protected override void OnWindowDestroyed(System.Windows.Window w){
            var cc = w as ControlCenter; if (cc==null) return;
            if(menuItem!=null){ var newMenu = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem; if(newMenu!=null && newMenu.Items.Contains(menuItem)) newMenu.Items.Remove(menuItem); menuItem=null; }
        }
    }
}
