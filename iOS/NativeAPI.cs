using System.Runtime.InteropServices;

public class NativeAPI
{
    [DllImport("__Internal")]
    public static extern void sendMessageToMobileApp(string message);
}