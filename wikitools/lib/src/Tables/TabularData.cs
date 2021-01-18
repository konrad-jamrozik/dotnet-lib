﻿namespace Wikitools.Lib.Tables
{
    public record TabularData(object[] HeaderRow, object[][] Rows)
    {
        public TabularData((object[] headerRow, object[][] rows) Data) : this(Data.headerRow, Data.rows) { }
    }
}