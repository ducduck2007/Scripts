public static class NetworkUtility 
{
    public static void SendDisconect()
    {
        Contexts.sharedInstance.network.ReplaceDisconnect(true);
    }
    
    public static byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[NetworkConfig.SIZE_CONTROL];
        for (int i = 0; i < NetworkConfig.SIZE_CONTROL; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }
    
    public static int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + NetworkConfig.SIZE_CONTROL; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
}
