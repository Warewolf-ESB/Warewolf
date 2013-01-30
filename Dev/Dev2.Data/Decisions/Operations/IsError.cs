﻿using System;
using System.IO;

namespace Dev2.Data.Decisions.Operations
{
    public class IsError : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            //if (cols.Length == 0)
            //{
            //    return false;
            //}

            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return (cols[0].Length > 0);
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsError;
        }
    }
}
