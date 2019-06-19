using System.Globalization;
using System.Threading;
using UnityEditor;

[InitializeOnLoad]
public class StartupSetLocalizationToUs
{
    static StartupSetLocalizationToUs()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
    }
}
