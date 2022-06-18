using System;
using System.IO;

namespace AudioUtilityToolkit
{
    public static class PCMUtility
    {
        /// <summary>
        /// Convert 32 bit float PCM to 16 bit PCM data bytes.
        /// </summary>
        /// <param name="pcm"></param>
        /// <returns></returns>
        public static byte[] Convert32bitFloatTo16bitBytes(float[] pcm)
        {
            var dataStream = new MemoryStream();

            for (int i = 0; i < pcm.Length; i++)
            {
                // The maximum absolute value of Int16 is 32768.
                dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(pcm[i] * 32768f)), 0, sizeof(Int16));
            }

            return dataStream.ToArray();
        }
    }
}