using System;
using System.Net.Sockets;
using System.Text;

namespace Tic_Tac_Toc_Core
{
    static public class Tcp_Reader
    {
        static public byte[] ReadStream(this NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            int readCount = 0;
            stream.Read(buffer, 0, buffer.Length); // 1 значение - DataSize
            readCount = BitConverter.ToInt32(buffer, 0); // Размер из байтов в int
            buffer = new byte[readCount]; // Массив данных
            stream.Read(buffer, 0, buffer.Length); // Читаем данные из потока и записываем в buffer
            return buffer;
        }

        static public string ReadStringStream(this NetworkStream stream)
        {
            return Encoding.ASCII.GetString(stream.ReadStream());
        }

        static public void WriteStream(this NetworkStream stream, byte[] data)
        {
            byte[] array = BitConverter.GetBytes(data.Length);
            stream.Write(array, 0, array.Length);
            stream.Flush();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        static public void WriteStringStream(this NetworkStream stream, string data)
        {
            stream.WriteStream(Encoding.ASCII.GetBytes(data));
        }
    }
}
