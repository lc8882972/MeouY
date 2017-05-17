namespace ZooKeeperClient.Util
{
    public class ZKPathUtil
    {
        public static string LeadingZeros(long number, int numberOfLeadingZeros)
        {
            return number.ToString().PadLeft(numberOfLeadingZeros, '0');
        }   
    }
}
