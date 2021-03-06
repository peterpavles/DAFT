﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace DAFT.Modules
{
    class SQLFuzzObjectName : Module
    {
        [DataContract]
        public struct Fuzz
        {
            [DataMember] public string ComputerName;
            [DataMember] public string Instance;
            [DataMember] public string ObjectId;
            [DataMember] public string ObjectName;
        }

        private List<Fuzz> fuzzed;

        private int start = 0;
        private int end = 5;

        internal SQLFuzzObjectName(Credentials credentials) : base(credentials)
        {
            fuzzed = new List<Fuzz>();
        }

        internal void SetStartId(int start)
        {
            this.start = start;
        }

        internal void SetEndId(int end)
        {
            this.end = end;
        }

        internal override bool Query()
        {
            using (SQLConnection sql = new SQLConnection(instance))
            {
                sql.BuildConnectionString(credentials);
                if (!sql.Connect())
                    return false;

                for (int i = start; i <= end; i++)
                {
                    string query1_1 = string.Format(
                        "SELECT \'{0}\' as [ComputerName],\n" +
                        "\'{1}\' as [Instance],\n" +
                        "\'{2}\' as [ObjectId],\n" +
                        "OBJECT_NAME({2}) as [ObjectName]",
                        computerName, instance, i
                        );
#if DEBUG
                    Console.WriteLine(query1_1);
#endif
                    table = sql.Query(query1_1);

                    foreach (DataRow row in table.AsEnumerable())
                    {
                        try
                        {
                            Fuzz f = new Fuzz
                            {
                                ComputerName = computerName,
                                Instance = instance,
                                ObjectId = (string)row["ObjectId"],
                            };
                            if (!(row["ObjectName"] is DBNull))
                                f.ObjectName = (string)row["ObjectName"];

#if DEBUG
                            Misc.PrintStruct<Fuzz>(f);
#endif
                            fuzzed.Add(f);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ArgumentNullException)
                                continue;
                            else
                                Console.WriteLine(ex.Message);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal List<Fuzz> GetResults()
        {
            return fuzzed;
        }
    }
}
