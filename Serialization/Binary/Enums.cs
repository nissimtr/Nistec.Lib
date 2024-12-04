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
#pragma warning disable  CS1591

namespace Nistec.Serialization
{

    public enum SerialContextType : byte
    {
        None = 0,
        ListGenericType = 28,
        hashtableType = 29,
        DictionaryGenericType = 30,
        DataTableType = 31,
        DataSetType = 32,
        StreamType = 33,
        XmlDocumentType = 34,
        XmlNodeType = 35,
        AnyClassType = 36,
        SerialEntityType = 37,
        SerialContextType = 38,
        GenericKeyValueType = 39,
        IEntityDictionaryType=40,
        dictionaryAssignType = 43,
        netStreamType = 44,
        dictionaryEntityType = 45,
        OtherType = 99,
        GenericEntityAsIDictionaryType = 100,
        GenericEntityAsIEntityType = 101
    }

    public enum EntityStreamState : byte
    {
        None = 0,
        ItemNotFound = 201,

        Timeout = 202,
        MessageError = 203,
        SerializeError = 204,
        SecurityError = 205,
        ArgumentsError = 206,
        NotSupportedError = 207,
        OperationError = 208,

        InternalError = 251,
        IoError = 252,
        SizeError = 253,
        NetworkError = 254,
        DataError = 255
    }

    public enum SerializerContextState
    {
        //Stream
    }


    // Enum for the standard types handled by Read/WriteObject()
    internal enum SerialType : byte
    {
        nullType = 0,
        boolType = 1,
        byteType = 2,
        uint16Type = 3,
        uint32Type = 4,
        uint64Type = 5,
        sbyteType = 6,
        int16Type = 7,
        int32Type = 8,
        int64Type = 9,
        charType = 10,
        stringType = 11,
        singleType = 12,
        doubleType = 13,
        decimalType = 14,
        dateTimeType = 15,
        timeSpanType = 16,
        byteArrayType = 17,
        charArrayType = 18,
        guidType = 19,
        enumType = 20,
        typeType = 21,
        int16ArrayType = 22,
        int32ArrayType = 23,
        int64ArrayType = 24,
        stringArrayType = 25,
        objectArrayType = 26,
        iCollectionType = 27,
        listGenericType = 28,
        hashtableType = 29,
        dictionaryGenericType = 30,
        dataTableType = 31,
        dataSetType = 32,
        streamType = 33,
        xmlDocumentType = 34,
        xmlNodeType = 35,
        anyClassType = 36,
        serialEntityType = 37,
        serialContextType = 38,
        genericKeyValueType = 39,
        iEntityDictionaryType = 40,
        stringDictionary=41,
        nameValueCollection=42,
        dictionaryAssignType = 43,
        netStreamType = 44,
        dictionaryEntityType = 45,
        otherType = 99,
        genericEntityAsIDictionaryType = 100,
        genericEntityAsIEntityType = 101

    }

    public enum SerialBaseType : byte
    {
        nullType = 0,
        boolType = 1,
        byteType = 2,
        uint16Type = 3,
        uint32Type = 4,
        uint64Type = 5,
        sbyteType = 6,
        int16Type = 7,
        int32Type = 8,
        int64Type = 9,
        charType = 10,
        stringType = 11,
        singleType = 12,
        doubleType = 13,
        decimalType = 14,
        dateTimeType = 15,
        timeSpanType = 16,
        byteArrayType = 17,
        charArrayType = 18,
        guidType = 19,

        otherType = 99,
    }
}
