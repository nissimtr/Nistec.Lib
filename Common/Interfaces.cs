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
using System.IO;
using Nistec.IO;
using System.Collections;
using Nistec.Generic;
using Nistec.Serialization;

#pragma warning disable  CS1591

namespace Nistec.Data
{
    public interface IDataTableAdaptor 
    {
        void Prepare(System.Data.DataTable dt);
    }
    public interface IDataRowAdaptor
    {
        void Prepare(System.Data.DataRow dr);
    }
   
}

namespace Nistec
{
    public enum ListenerState
    {
        Down=0,
        Initilaized=1,
        Started=2,
        Stoped=3,
        Paused=4
    }
    public enum OnOffState
    {
        On = 0,
        Off = 1,
        Toggle = 2,
    }

    public interface IListener
    {
        void Start();
        void Stop();
        bool Pause(OnOffState onOff);
        void Shutdown(bool waitForWorkers);
        //bool Initialized { get; }
        ListenerState State { get; }
    }

    public interface IAck
    {
        string Identifier { get;}
        string Message { get; }
        object Response { get; }
        int Status { get; }
        bool IsOk { get; }
        string Display();
        string ToJson();
    }

    //public interface IJson
    //{
    //    string ToJson();
    //}
}

namespace Nistec.Esb
{
    public interface IEsbTopic 
    {
        string ConnectionString { get; set; }
        string DbName { get; set; }
        string TopicName { get; set; }
        long MaxSize { get; set; }
        int MaxDocuments { get; set; }
        int Ttl { get; set; }
        bool DeleteOnRead { get; set; }
        bool Capped { get; set; }
        string MessageType { get; set; }
        bool EnableCreate { get; set; }//"EnableCreateCollection"
        string AbortName { get; }

        //public TimeSeries TimeSeries { get; set; }

        //public TimeSeriesOptions TimeSeriesOptions()
        //{
        //    if (TimeSeries == null)
        //        return null;
        //    return TimeSeries.ToTimeSeriesOptions();
        //}

        bool IsValid();

        //public IMongoDatabase GetDatabase()
        //{
        //    var client = new MongoClient(ConnectionString);
        //    return client.GetDatabase(DbName);
        //}

        //IMongoDatabase _Database;
        //public IMongoDatabase Database
        //{
        //    get
        //    {
        //        if (_Database == null)
        //        {
        //            _Database = GetDatabase();
        //        }
        //        return _Database;
        //    }
        //}
        //public IMongoCollection<EsbMessage<T>> GetAbortCollection<T>() where T : class
        //{
        //    return Database.GetCollection<EsbMessage<T>>(AbortName);
        //}
        //public IMongoCollection<EsbMessage<T>> GetCollection<T>() where T : class
        //{
        //    return Database.GetCollection<EsbMessage<T>>(TopicName);
        //}
        //public IMongoCollection<OffsetItem> GetOffsetCollection()
        //{
        //    return Database.GetCollection<OffsetItem>(Constants.CURSOR_TRACK);
        //}
        //public IMongoCollection<ObjectId> GetCollectionById()
        //{
        //    return Database.GetCollection<ObjectId>(TopicName);
        //}
        //public void CreateCollection()
        //{
        //    //if (!IsCollectionExists(mongoDB, info.TopicName))
        //    //{
        //    Database.CreateCollection(TopicName, new CreateCollectionOptions
        //    {
        //        Capped = Capped,
        //        MaxDocuments = MaxDocuments,
        //        MaxSize = MaxSize
        //        //TimeSeriesOptions = info.TimeSeriesOptions()// new TimeSeriesOptions("Enqueued","", TimeSeriesGranularity.Seconds)
        //    });
        //}
        //public void CreateAbortCollection()
        //{
        //    Database.CreateCollection(AbortName);
        //}

        //public bool IsCollectionExists()
        //{
        //    var filter = new BsonDocument("name", TopicName);
        //    var options = new ListCollectionNamesOptions { Filter = filter };
        //    return Database.ListCollectionNames(options).Any();
        //}

        //public bool IsAbortCollectionExists()
        //{
        //    var filter = new BsonDocument("name", AbortName);
        //    var options = new ListCollectionNamesOptions { Filter = filter };
        //    return Database.ListCollectionNames(options).Any();
        //}
    }
}