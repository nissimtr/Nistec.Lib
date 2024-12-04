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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Nistec.Serialization;

#pragma warning disable  CS1591

namespace Nistec.IO
{
    public struct VersionStream
    {
        byte[] _Binary;
        public byte[] Binary
        {
            get { return _Binary; }
        }
        string _Text;
        public string Text
        {
            get { return _Text; }
        }

        public bool IsValid
        {
            get { return (_Binary != null && _Binary.Length == 8) && (_Text != null && _Text.Length == 8); }
        }

        public VersionStream(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentNullException("VersionStream.version");
            }
            if (version.Length != 8)
            {
                throw new ArgumentException("VersionStream.version is incorrect length, it must be 8 chars");
            }
            _Text = version;
            _Binary = BinaryConverter.GetBytes(version);
        }

        public VersionStream(byte[] version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("VersionStream.version");
            }
            if (version.Length != 8)
            {
                throw new ArgumentException("VersionStream.version is incorrect length, it must be 8 chars");
            }
            _Text = BinaryConverter.ReadString(version, 0, version.Length);
            _Binary = version;
        }

        internal static VersionStream Get(byte[] version)
        {
            if (version == null)
            {
                return new VersionStream();
            }
            if (version.Length != 8)
            {
                return new VersionStream();
            }
            return new VersionStream(version);
        }
    }

    public class MapStream : NetStream
    {

        /* Table structure:
                8    bytes         - version
                2     bytes         - CRLF
                4     bytes         - info fields count
                2     bytes         - CRLF
       fields * 4     bytes         - fields count * size
                2     bytes         - CRLF
       fields * 1     bytes         - fields count * type
                2     bytes         - CRLF
                ... data
            */


        /// <summary>
        /// Initialize a new instance of MapStream.
        /// </summary>
        public MapStream()
            : base()
        {

        }

        /// <summary>
        /// Initialize a new instance of MapStream.
        /// </summary>
        /// <param name="buffer"></param>
        public MapStream(byte[] buffer)
            : base(buffer)
        {

        }

        /// <summary>
        /// Initialize a new instance of MapStream.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="values"></param>
        public MapStream(VersionStream version, params object[] values)
        {
            if (!version.IsValid)
            {
                throw new ArgumentException("MapStream.version is not valid");
            }
            if (values == null)
            {
                throw new ArgumentNullException("MapStream.values");
            }

            ArrayList rowByteValues = new ArrayList();
            ArrayList rowSizes = new ArrayList();
            ArrayList rowTypes = new ArrayList();

            for (int i = 0; i < values.Length; i++)
            {

                object val = values[i];
                //rowAllValues.Add(val);
                SerialBaseType st = SerialBaseType.nullType;
                byte[] b = BinaryConverter.GetBytes(val, out st);
                rowSizes.Add(b.Length);
                rowTypes.Add((byte)st);
                rowByteValues.Add(b);
            }

            //version
            Write(version.Binary, 0, version.Binary.Length);

            Write(BinaryConverter.CrLf, 0, 2);

            //fieldcount
            byte[] fieldcount = BinaryConverter.GetBytes(rowByteValues.Count);
            Write(fieldcount, 0, fieldcount.Length);

            Write(BinaryConverter.CrLf, 0, 2);

            //fields sizes
            for (int i = 0; i < rowSizes.Count; i++)
            {
                byte[] b = (byte[])rowSizes[i];
                Write(b, 0, b.Length);
            }

            Write(BinaryConverter.CrLf, 0, 2);

            //field types
            for (int i = 0; i < rowTypes.Count; i++)
            {
                byte b = (byte)rowTypes[i];
                WriteByte(b);
            }

            Write(BinaryConverter.CrLf, 0, 2);

            //values
            for (int i = 0; i < rowByteValues.Count; i++)
            {
                byte[] b = (byte[])rowByteValues[i];
                Write(b, 0, b.Length);
            }
            Flush();
        }

        public VersionStream Version()
        {
            //version
            byte[] b = PeekBytes(0, 8);
            return VersionStream.Get(b);
        }

        public int Fields()
        {
            //fieldcount
            byte[] b = PeekBytes(10, 4);
            return BinaryConverter.ReadInt32(b);
        }

        public int[] FieldsSize()
        {
            //8,2,4,2,
            int fields = Fields();
            int[] sizes = new int[fields];
            int offset = 16;
            //fields sizes
            for (int i = 0; i < fields; i++)
            {
                byte[] b = PeekBytes(offset, 4);
                sizes[i] = BinaryConverter.ReadInt32(b);
                offset += 4;
            }
            return sizes;
        }

        public SerialBaseType[] FieldsType()
        {
            //8,2,4,2,
            int fields = Fields();
            SerialBaseType[] types = new SerialBaseType[fields];
            int offset = 16 + (fields * 4) + 2;
            //fields sizes
            for (int i = 0; i < fields; i++)
            {
                types[i] = (SerialBaseType)PeekByte(offset);
                offset++;
            }
            return types;
        }

        public object[] Values()
        {
            //8,2,4,2,
            int fields = Fields();
            int[] sizes = FieldsSize();
            SerialBaseType[] types = FieldsType();
            object[] values = new object[fields];
            int offset = 16 + (fields * 4) + 2 + (fields * 1) + 2;
            //fields sizes
            for (int i = 0; i < fields; i++)
            {
                int len = sizes[i];
                SerialBaseType st = (SerialBaseType)types[i];
                byte[] b = PeekBytes(offset, len);
                values[i] = BinaryConverter.ReadBytes(b, st);
                offset += len;
            }
            return values;
        }

        public object[] Parse(out VersionStream version, out int fields, out int[] sizes, out SerialBaseType[] types)
        {
            byte[] b = new byte[8];

            Position = 0;

            //version
            Read(b, 0, 8);
            version = new VersionStream(b);

            //crlf
            Position += 2;

            //fieldcount
            b = new byte[4];
            Read(b, 0, 4);
            fields = BinaryConverter.ReadInt32(b);

            //crlf
            Position += 2;

            sizes = new int[fields];

            //fields sizes
            for (int i = 0; i < fields; i++)
            {
                byte[] bs = new byte[4];
                Read(bs, 0, 4);
                sizes[i] = BinaryConverter.ReadInt32(bs);
            }

            //crlf
            Position += 2;

            types = new SerialBaseType[fields];

            //fields types
            for (int i = 0; i < fields; i++)
            {
                int t = ReadByte();
                types[i] = (SerialBaseType)(byte)(t > 0 ? 0 : t);
            }

            //crlf
            Position += 2;

            object[] values = new object[fields];

            //fields values
            for (int i = 0; i < fields; i++)
            {
                int len = sizes[i];
                SerialBaseType st = (SerialBaseType)types[i];
                byte[] bv = new byte[len];
                Read(bv, 0, len);
                values[i] = BinaryConverter.ReadBytes(bv, st);
            }

            return values;
        }

        internal static MapStream Get(int status, string statusDescription)
        {
            return new MapStream(new VersionStream("MSGSTATE"), status, statusDescription);
        }

        internal static MapStream Get(string version, int status, string statusDescription)
        {
            return new MapStream(new VersionStream(version), status, statusDescription);
        }
    }
}
