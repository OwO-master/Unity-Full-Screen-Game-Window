#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

//Wait for testing
//1. no game window
//2. 1 not detech game window
//3. 2 not detech game window
//4. 1 detech game window
//5. 2 detech game window
//6. 1 not detech game window, 1 detech game window
//7. 2 not detech game window, 1 detech game window
//8. 1 not detech game window, 2 detech game window

[InitializeOnLoad]
public class cmdow
{
    public static bool isAvailable = false;
    public static bool isFullScreen = false;

    //Pre FullScreen Settings
    static Vector2 PFS_Size = new Vector2();
    static Vector2 PFS_Position = new Vector2();

    //FullScreen Settings
    static Vector2 FS_Size = new Vector2(1934, 1129);
    static Vector2 FS_Position = new Vector2(-7, -42);

    static cmdow()
    {
        cmdow.ACGW_bool = EditorPrefs.GetBool(ACGW, false);
        EditorApplication.delayCall += () =>
        {
            ACW_Action(ACGW_bool);
        };
    }

    [MenuItem("cmdow/Refresh _%F12")]
    private static void Refresh()
    {
        Refresh(false);
    }
    private static void Refresh(bool s_mode = false)
    {
        List<string> results = ScreenInfo.GetScreenInfoList();
        int windows = 0;
        foreach (string r in results)
        {
            if (r.StartsWith("0x")) windows++;
        }
        switch (windows)
        {
            case 0:
                isAvailable = ACGW_bool;
                if (s_mode) break;
                if (isAvailable)
                {
                    Debug.LogWarning($"<b>[cmdow Refresh]</b> cmdow has warning:" +
                        $"cmdow does not detect any deteched Game window, but \"Auto Create Game Window\" is checked" +
                        $"FullScreen function will still work if cmdow detect 0 deteched Game window");
                }
                else
                {
                    Debug.LogError($"<b>[cmdow Refresh]</b> cmdow has error: \n" +
                        $"1. Check if there has any Game window that is deteched from the main Unity Editor.\n" +
                        $"2. The current version Unity Editor is not supported.\n" +
                        $"3. The current version Unity Editor has renamed the Game window title, please wait for update or using cmdow to find the currect window title");
                }
                break;
            case 1:
                isAvailable = true;
                if (s_mode) break;
                Debug.Log($"<b>[cmdow Refresh]</b> cmdow has detect the Game window. FullScreen is now available");
                break;
            default:
                isAvailable = false;
                if (s_mode) break;
                Debug.LogWarning($"<b>[cmdow Refresh]</b> The current script does not support more then one deteched Game window");
                break;
        }
    }

    [MenuItem("cmdow/FullScreen _F12")]
    private static async void FullScreen()
    {
        Refresh(false);
        if (!isAvailable)
        {
            Debug.LogError("<b>[cmdow FullScreen]</b> cmdow is currently not available, please try Refresh(Control + F12)");
            return;
        }

        if (ScreenInfo.GetScreenInfoList().Count != 1)
        {
            if (ScreenInfo.GetGameWindow() != null)
            {
                ScreenInfo.KillAllGameWindow();
            }
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            isFullScreen = false;
        }
        
        int gsil = ScreenInfo.GetScreenInfoList().Count;
        while(gsil == 0)
        {
            await Task.Delay(1);
            gsil = ScreenInfo.GetScreenInfoList().Count;
        }

        //3. check if window is fullscreen
        ScreenInfo si = ScreenInfo.Resolve(ScreenInfo.GetScreenInfoList()[0]);
        if (si.Size == FS_Size && si.Position == FS_Position)
        {
            isFullScreen = true;
        }
        else
        {
            isFullScreen = false;
            PFS_Size = si.Size;
            PFS_Position = si.Position;
        }
        using (Process p = new Process())
        {
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            if (isFullScreen)
            {
                //this line is call cmdow to resize deteched game window
                Debug.Log($"exit play mode: pfs {PFS_Position} {PFS_Size}");
                p.StandardInput.WriteLine($@"cd {Application.dataPath}/cmdow/ & cmdow Game /siz {PFS_Size.x} {PFS_Size.y} /mov {PFS_Position.x} {PFS_Position.y} & exit");
                isFullScreen = false;
            }
            else
            {
                //this line is call cmdow to fullscreen deteched game window
                Debug.Log($"Enter play mode: fs {FS_Position} {FS_Size}");
                p.StandardInput.WriteLine($@"cd {Application.dataPath}/cmdow/ & cmdow Game /siz {FS_Size.x} {FS_Size.y} /mov {FS_Position.x} {FS_Position.y} & exit");
                isFullScreen = true;
            }
            p.StandardInput.AutoFlush = true;
            p.Close();
        }
    }

    [MenuItem("cmdow/PANIK! _#F12", false, 999)]
    private static void PANIK()
    {
        List<string> results = ScreenInfo.GetScreenInfoList();
        string cmdInput = "";
        foreach (string r in results)
        {
            if (r.StartsWith("0x"))
            {
                cmdInput += $"& cmdow {r.Split(' ')[0]} /not /siz 800 600 /mov 0 0 ";
            }
        }
        using (Process t = new Process())
        {
            t.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            t.StartInfo.Arguments = "/k";
            t.StartInfo.UseShellExecute = false;
            t.StartInfo.RedirectStandardInput = true;
            t.StartInfo.CreateNoWindow = true;
            t.Start();
            t.StandardInput.WriteLine($@"cd {Application.dataPath}/cmdow/ {cmdInput}& exit");
        }
        isFullScreen = false;
        Debug.LogWarning($"<b>[cmdow PANIK!]</b> All Game windows(if there has any) are set to:\n" +
            $"<b><i>Position</i></b>(Main Screen) 0 0\n" +
            $"<b><i>Size</i></b> 800 600");
    }

    const string ACGW = "cmdow/Auto Create Game Window";
    static bool ACGW_bool = false;
    [MenuItem(ACGW)]
    private static void Auto_Create_Window()
    {
        ACW_Action(!ACGW_bool);
    }
    public static void ACW_Action(bool enabled)
    {
        Menu.SetChecked(ACGW, enabled);
        EditorPrefs.SetBool(ACGW, enabled);
        ACGW_bool = enabled;
        Debug.Log($"<b>[cmdow ACGW]</b> Auto Create Game Window is set to <b>{enabled}</b>");
    }
}

[InitializeOnLoad]
public class ScreenInfo
{
    #region CMDOW Output Info
    public string Handle;
    public int Level;
    public int Pid;
    //window status
    public enum Mode
    {
        Min, Max, Res
    }
    public Mode SizeStatus = Mode.Res;
    public bool Activate = false;
    public bool Enable = true;
    public bool Visible = true;
    //window status
    public Vector2 Position = new Vector2(0, 0);
    public Vector2 Size = new Vector2(800, 600);
    public string Image;
    public string Caption;
    #endregion

    public static ScreenInfo Resolve(string input)
    {
        List<string> temp = input.Split(' ').ToList();
        temp.RemoveAll(xa => xa == "");
        ScreenInfo si = new ScreenInfo();
        //Handle
        if (temp[0].StartsWith("0x"))
        {
            si.Handle = temp[0];
        }
        else
        {
            Debug.LogError($"ScreenInfo Resolve Error. Return Default ScreenInfo.\n" +
                $"Handle -> {temp[0]} not start with 0x");
            return new ScreenInfo();
        }
        //Level
        si.Level = int.Parse(temp[1]);
        //Pid
        si.Pid = int.Parse(temp[2]);
        //SizeStatus
        switch (temp[3])
        {
            case "Min":
                si.SizeStatus = Mode.Min;
                break;
            case "Max":
                si.SizeStatus = Mode.Max;
                break;
            case "Res":
                si.SizeStatus = Mode.Res;
                break;
            default:
                Debug.LogError($"ScreenInfo Resolve Error. Return Default ScreenInfo.\n" +
                    $"SizeStatus -> {temp[3]} can not resolve to any mode");
                return new ScreenInfo();
        }
        //Activate
        switch (temp[4])
        {
            case "Act":
                si.Activate = true;
                break;
            case "Ina":
                si.Activate = false;
                break;
            default:
                Debug.LogError($"ScreenInfo Resolve Error. Return Default ScreenInfo.\n" +
                    $"SizeStatus -> {temp[4]} can not resolve to any mode");
                return new ScreenInfo();
        }
        //Enable
        switch (temp[5])
        {
            case "Ena":
                si.Enable = true;
                break;
            case "Dis":
                si.Enable = false;
                break;
            default:
                Debug.LogError($"ScreenInfo Resolve Error. Return Default ScreenInfo.\n" +
                    $"SizeStatus -> {temp[5]} can not resolve to any mode");
                return new ScreenInfo();
        }
        //Visible
        switch (temp[6])
        {
            case "Vis":
                si.Enable = true;
                break;
            case "Hid":
                si.Enable = false;
                break;
            default:
                Debug.LogError($"ScreenInfo Resolve Error. Return Default ScreenInfo.\n" +
                    $"SizeStatus -> {temp[6]} can not resolve to any mode");
                return new ScreenInfo();
        }
        //Position
        si.Position = new Vector2(int.Parse(temp[7]), int.Parse(temp[8]));
        //Size
        si.Size = new Vector2(int.Parse(temp[9]), int.Parse(temp[10]));
        //Image
        si.Image = temp[11];
        //Caption
        si.Caption = temp[12];
        return si;
    }

    public static List<string> GetScreenInfoList()
    {
        string result = "";
        using (Process p = new Process())
        {
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/k";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine($@"cd {Application.dataPath}/cmdow/ & cmdow Game /p & exit");
            p.StandardInput.AutoFlush = true;
            result = p.StandardOutput.ReadToEnd();
        }
        List<string> results = result.Split('\n').ToList();
        results = results.FindAll(xa => xa.StartsWith("0x"));
        return results;
    }

    public static EditorWindow GetGameWindow()
    {
        Assembly assembly = typeof(EditorWindow).Assembly;
        Type type = assembly.GetType("UnityEditor.PlayModeView");
        EditorWindow ew = null;
        try { ew = EditorWindow.GetWindow(type); } catch (Exception exc){ Debug.Log(exc); }
        return ew;
    }

    //SENSATION

    public static void KillAllGameWindow()
    {
        while(GetGameWindow() != null)
        {
            GetGameWindow().Close();
        }
    }
}

#endif
