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
namespace Nistec.Collections
{

    public class GenericArray<T> where T : class
    {
        private T[] m_array;

        public GenericArray(int initialSize)
        {
            this.m_array = new T[initialSize];
        }

        public int Add(T e)
        {
            T[] localArray;
           
            localArray = this.m_array;
            lock (localArray)
            {
                for (int i = 0; i < localArray.Length; i++)
                {
                    if (localArray[i] == null)
                    {
                        localArray[i] = e;
                        return i;
                    }
                    if ((i == (localArray.Length - 1)) && (localArray == this.m_array))
                    {
                        T[] destinationArray = new T[localArray.Length * 2];
                        Array.Copy(localArray, destinationArray, (int)(i + 1));
                        destinationArray[i + 1] = e;
                        this.m_array = destinationArray;
                        return (i + 1);
                    }
                }
               
            }
            return 0;
        }

        public void Remove(T e)
        {
            T[] array = this.m_array;
            lock (array)
            {
                for (int i = 0; i < this.m_array.Length; i++)
                {
                    if (this.m_array[i] == e)
                    {
                        this.m_array[i] = default(T);
                        break;
                    }
                }
            }
        }

        // Properties
        public T[] Current
        {
            get
            {
                return this.m_array;
            }
        }
    }

}
