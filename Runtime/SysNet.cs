//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
#pragma warning disable  CS1591

namespace Nistec.Runtime
{
	
	/// <summary>
	/// Provides net core utility methods.
	/// </summary>
    public class SysNet
	{

        public static string GetExecutingAssemblyName()
        {
             return Assembly.GetExecutingAssembly().GetName().Name;
        }
        public static string GetExecutingAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public static string GetEntryAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
        public static MethodInfo GetMethodInfo()
        {
             MethodInfo method = (MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return method;
        }
        public static MethodInfo GetMethodInfo(int frame)
        {
            MethodInfo method = (MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(frame).GetMethod());
            return method;
        }
        public static string GetRandom(int length)
        {
            Random random = new Random();
            string str = random.Next(0x5f5e100, 0x3b9ac9ff).ToString();
            if ((length > 2) && (length < 9))
            {
                return str.Substring(0, length);
            }
            return str;
        }

 
		/// <summary>
		/// Checks if specified string data is acii data.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsAscii(string data)
		{			
			foreach(char c in data){
				if((int)c > 127){ 
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Gets file name from path.
		/// </summary>
		/// <param name="filePath">File file path with file name. For examples: c:\fileName.xxx, aaa\fileName.xxx.</param>
		/// <returns></returns>
		public static string GetFileNameFromPath(string filePath)
		{
			return Path.GetFileName(filePath);
		}


        /// <summary>
        /// Gets if specified value is IP address.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>Returns true if specified value is IP address.</returns>
        public static bool IsIP(string value)
        {
            try
            {
                IPAddress outip = IPAddress.None;
                return IPAddress.TryParse(value, out outip);//.Parse(value);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compares 2 IP addresses. Returns 0 if IPs are equal, 
        /// returns positive value if destination IP is bigger than source IP,
        /// returns negative value if destination IP is smaller than source IP.
        /// </summary>
        /// <param name="source">Source IP address.</param>
        /// <param name="destination">Destination IP address.</param>
        /// <returns></returns>
        public static int CompareIP(IPAddress source,IPAddress destination)
        {
            byte[] sourceIpBytes      = source.GetAddressBytes();
            byte[] destinationIpBytes = destination.GetAddressBytes();

            // IPv4 and IPv6
            if(sourceIpBytes.Length < destinationIpBytes.Length){
                return 1;
            }
            // IPv6 and IPv4
            else if(sourceIpBytes.Length > destinationIpBytes.Length){
                return -1;
            }
            // IPv4 and IPv4 OR IPv6 and IPv6
            else{                
                for(int i=0;i<sourceIpBytes.Length;i++){
                    if(sourceIpBytes[i] < destinationIpBytes[i]){
                        return 1;
                    }
                    else if(sourceIpBytes[i] > destinationIpBytes[i]){
                        return -1;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets if specified IP address is private LAN IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        /// <returns>Returns true if IP is private IP.</returns>
        public static bool IsPrivateIP(string ip)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }

            return IsPrivateIP(IPAddress.Parse(ip));
        }

        /// <summary>
        /// Gets if specified IP address is private LAN IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <returns>Returns true if IP is private IP.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public static bool IsPrivateIP(IPAddress ip)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }

			if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork){
				byte[] ipBytes = ip.GetAddressBytes();

				/* Private IPs:
					First Octet = 192 AND Second Octet = 168 (Example: 192.168.X.X) 
					First Octet = 172 AND (Second Octet >= 16 AND Second Octet <= 31) (Example: 172.16.X.X - 172.31.X.X)
					First Octet = 10 (Example: 10.X.X.X)
					First Octet = 169 AND Second Octet = 254 (Example: 169.254.X.X)

				*/

				if(ipBytes[0] == 192 && ipBytes[1] == 168){
					return true;
				}
				if(ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31){
					return true;
				}
				if(ipBytes[0] == 10){
					return true;
				}
				if(ipBytes[0] == 169 && ipBytes[1] == 254){
					return true;
				}
			}

			return false;
        }


        /// <summary>
        /// Creates new socket for the specified end point.
        /// </summary>
        /// <param name="localEP">Local end point.</param>
        /// <param name="protocolType">Protocol type.</param>
        /// <returns>Retruns newly created socket.</returns>
        public static Socket CreateSocket(IPEndPoint localEP,ProtocolType protocolType)
        {
            SocketType socketType = SocketType.Dgram;
            if(protocolType == ProtocolType.Udp){
                socketType = SocketType.Dgram;
            }

            if(localEP.AddressFamily == AddressFamily.InterNetwork){
                Socket socket = new Socket(AddressFamily.InterNetwork,socketType,protocolType);
                socket.Bind(localEP);

                return socket;
            }
            else if(localEP.AddressFamily == AddressFamily.InterNetworkV6){
                Socket socket = new Socket(AddressFamily.InterNetworkV6,socketType,protocolType);
                socket.Bind(localEP);

                return socket;
            }
            else{
                throw new ArgumentException("Invalid IPEndPoint address family.");
            }
        }


        /// <summary>
        /// Converts specified string to HEX string.
        /// </summary>
        /// <param name="text">String to convert.</param>
        /// <returns>Returns hex string.</returns> 
        public static string Hex(string text)
        {
            return BitConverter.ToString(Encoding.UTF8.GetBytes(text)).ToLower().Replace("-", "");
        }

        /// <summary>
		/// Converts string to hex string.
		/// </summary>
		/// <param name="data">String to convert.</param>
		/// <returns>Returns data as hex string.</returns>
		public static string ToHexString(string data)
		{
            return Encoding.UTF8.GetString(ToHex(Encoding.UTF8.GetBytes(data)));
        }

        /// <summary>
		/// Converts string to hex string.
		/// </summary>
		/// <param name="data">Data to convert.</param>
		/// <returns>Returns data as hex string.</returns>
		public static string ToHexString(byte[] data)
		{
            return Encoding.UTF8.GetString(ToHex(data));
        }

		/// <summary>
		/// Convert byte to hex data.
		/// </summary>
		/// <param name="byteValue">Byte to convert.</param>
		/// <returns></returns>
		public static byte[] ToHex(byte byteValue)
		{
			return ToHex(new byte[]{byteValue});
		}

		/// <summary>
		/// Converts data to hex data.
		/// </summary>
		/// <param name="data">Data to convert.</param>
		/// <returns></returns>
		public static byte[] ToHex(byte[] data)
		{
            byte[] val = null;
			char[] hexChars = new char[]{'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};

            using (MemoryStream retVal = new MemoryStream(data.Length * 2))
            {
                foreach (byte b in data)
                {
                    byte[] hexByte = new byte[2];

                    // left 4 bit of byte
                    hexByte[0] = (byte)hexChars[(b & 0xF0) >> 4];

                    // right 4 bit of byte
                    hexByte[1] = (byte)hexChars[b & 0x0F];

                    retVal.Write(hexByte, 0, 2);
                }
                val = retVal.ToArray();
            }
			return val;
		}
                

		/// <summary>
		/// Converts hex byte data to normal byte data. Hex data must be in two bytes pairs, for example: 0F,FF,A3,... .
		/// </summary>
		/// <param name="hexData">Hex data.</param>
		/// <returns></returns>
		public static byte[] FromHex(byte[] hexData)
		{
			if(hexData.Length < 2 || (hexData.Length / (double)2 != Math.Floor(hexData.Length / (double)2)))
            {
				throw new Exception("Illegal hex data, hex data must be in two bytes pairs, for example: 0F,FF,A3,... .");
			}
            byte[] val = null;
			using(MemoryStream retVal = new MemoryStream(hexData.Length / 2))
            {
			// Loop hex value pairs
                for (int i = 0; i < hexData.Length; i += 2)
                {
                    byte[] hexPairInDecimal = new byte[2];
                    // We need to convert hex char to decimal number, for example F = 15
                    for (int h = 0; h < 2; h++)
                    {
                        if (((char)hexData[i + h]) == '0')
                        {
                            hexPairInDecimal[h] = 0;
                        }
                        else if (((char)hexData[i + h]) == '1')
                        {
                            hexPairInDecimal[h] = 1;
                        }
                        else if (((char)hexData[i + h]) == '2')
                        {
                            hexPairInDecimal[h] = 2;
                        }
                        else if (((char)hexData[i + h]) == '3')
                        {
                            hexPairInDecimal[h] = 3;
                        }
                        else if (((char)hexData[i + h]) == '4')
                        {
                            hexPairInDecimal[h] = 4;
                        }
                        else if (((char)hexData[i + h]) == '5')
                        {
                            hexPairInDecimal[h] = 5;
                        }
                        else if (((char)hexData[i + h]) == '6')
                        {
                            hexPairInDecimal[h] = 6;
                        }
                        else if (((char)hexData[i + h]) == '7')
                        {
                            hexPairInDecimal[h] = 7;
                        }
                        else if (((char)hexData[i + h]) == '8')
                        {
                            hexPairInDecimal[h] = 8;
                        }
                        else if (((char)hexData[i + h]) == '9')
                        {
                            hexPairInDecimal[h] = 9;
                        }
                        else if (((char)hexData[i + h]) == 'A' || ((char)hexData[i + h]) == 'a')
                        {
                            hexPairInDecimal[h] = 10;
                        }
                        else if (((char)hexData[i + h]) == 'B' || ((char)hexData[i + h]) == 'b')
                        {
                            hexPairInDecimal[h] = 11;
                        }
                        else if (((char)hexData[i + h]) == 'C' || ((char)hexData[i + h]) == 'c')
                        {
                            hexPairInDecimal[h] = 12;
                        }
                        else if (((char)hexData[i + h]) == 'D' || ((char)hexData[i + h]) == 'd')
                        {
                            hexPairInDecimal[h] = 13;
                        }
                        else if (((char)hexData[i + h]) == 'E' || ((char)hexData[i + h]) == 'e')
                        {
                            hexPairInDecimal[h] = 14;
                        }
                        else if (((char)hexData[i + h]) == 'F' || ((char)hexData[i + h]) == 'f')
                        {
                            hexPairInDecimal[h] = 15;
                        }
                    }

                    // Join hex 4 bit(left hex cahr) + 4bit(right hex char) in bytes 8 it
                    retVal.WriteByte((byte)((hexPairInDecimal[0] << 4) | hexPairInDecimal[1]));
                }
                val = retVal.ToArray();
			}

			return val;
		}


        /// <summary>
        /// Computes md5 hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <param name="hex">Specifies if md5 value is returned as hex string.</param>
        /// <returns>Resturns md5 value or md5 hex value.</returns>
        public static string ComputeMd5(string text,bool hex)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();			
			byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));

            if(hex){
			    return ToHexString(System.Text.Encoding.UTF8.GetString(hash)).ToLower();
            }
            else{
                return System.Text.Encoding.UTF8.GetString(hash);
            }
        }


    }
}
