using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace NinjaTrader.NinjaScript.AddOns.TpoV25.Core
{
    public class MetricsReporter
    {
        readonly string folder;
        readonly JavaScriptSerializer ser = new JavaScriptSerializer();

        public MetricsReporter(string baseDir)
        {
            folder = System.IO.Path.Combine(baseDir, "metrics");
            try { Directory.CreateDirectory(folder); } catch {}
        }
        
        public void Write(MetricsSnapshot snap)
        {
            if (snap == null) return;
            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var path1 = System.IO.Path.Combine(folder, Safe(snap.Symbol) + "_latest.json");
                var path2 = System.IO.Path.Combine(folder, "metrics_latest.json");

                var js1 = ser.Serialize(snap);
                File.WriteAllText(path1, js1);

                var all = MetricsBus.All();
                var jsAll = ser.Serialize(new {
                    generatedAtEt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm"),
                    snapshots = all
                });
                File.WriteAllText(path2, jsAll);

                Logger.Info("[TPO] wrote JSON -> " + path1);
            }
            catch (Exception ex)
            {
                Logger.Warn("[TPO] Reporter error: " + ex.Message);
            }
        }



        string Safe(string s){ foreach(var c in System.IO.Path.GetInvalidFileNameChars()) s=s.Replace(c,'_'); return s.Replace(' ','_'); }
    }
}
