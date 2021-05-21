#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class iOSPostProcessBuild : MonoBehaviour {

    [PostProcessBuild]
    public static void ChangeXCodePList(BuildTarget buildTarget, string pathToBuildProject)
    {       
        if (buildTarget == BuildTarget.iOS)
        {
            const string BTPermissions = "NSBluetoothAlwaysUsageDescription";

            Debug.Log("Attempting to set info.plist for iOS build...");
            string plistPath = pathToBuildProject + "/Info.plist";

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict root = plist.root;

            //var arr = new PlistElementArray();
            //arr.AddString("For making SenzeBand connections.");
            root[BTPermissions] = new PlistElementString("For making SenzeBand connections.");

            plist.WriteToFile(plistPath);
        }
    }
}
#endif
