using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JC
{
    public class CSV
    {
        public static string newline
        {
            set
            {
                s_newline = value;
            }
            get
            {
                return s_newline;
            }
        }

        public int rows
        {
            get
            {
                return m_rows;
            }
        }

        public int columns
        {
            get
            {
                return m_columns;
            }
        }

        public string Get(int row, int column)
        {
            if (row < 0 || row >= m_rows || column < 0 || column >= m_columns)
            {
                return null;
            }

            return table[row, column];
        }

        public string Get(int row, string fieldName)
        {
            if (row < 0 || row >= m_rows)
            {
                return null;
            }

            int column = 0;

            if (headerMappingMap.TryGetValue(fieldName, out column))
            {
                return table[row, column];
            }

            return null;
        }

        public bool Parse(string cvsString, int rowStartIndex = 0)
        {
            if (string.IsNullOrEmpty(cvsString))
            {
                return false;
            }

            table = null;
            headerMappingMap = null;

            try
            {
                string[] rowBlocks = Regex.Split(cvsString, s_newline);
                if (rowBlocks.Length - rowStartIndex > 0)
                {
                    m_rows = rowBlocks.Length - rowStartIndex - 1;
                    string[] headerBlocks = rowBlocks[rowStartIndex].Split(',');
                    m_columns = headerBlocks.Length;
                    table = new string[m_rows, m_columns];

                    headerMappingMap = new Dictionary<string, int>();
                    for (int columnIndex = 0; columnIndex < m_columns; ++columnIndex)
                    {
                        headerMappingMap.Add(headerBlocks[columnIndex], columnIndex);
                    }
                }
                else
                {
                    return false;
                }

                int emptyRows = 0;
                for (int rowIndex = 0; rowIndex < m_rows; ++rowIndex)
                {
                    string rowBlock = rowBlocks[rowIndex + rowStartIndex + 1];
                    if (string.IsNullOrEmpty(rowBlock))
                    {
                        ++emptyRows;
                        continue;
                    }

                    string[] cellBlocks = rowBlock.Split(',');
                    for (int columnIndex = 0; columnIndex < m_columns; ++columnIndex)
                    {
                        string sasdf = cellBlocks[columnIndex];
                        table[rowIndex, columnIndex] = sasdf;
                    }
                }
                m_rows -= emptyRows;

                return true;
            }
            catch (System.Exception exception)
            {
                m_columns = 0;
                m_rows = 0;
                table = null;
                headerMappingMap = null;
                Debug.LogError(exception.Message);
                Debug.LogError(exception.StackTrace);
                return false;
            }
        }

        private static string s_newline = "\r\n";

        private string[,] table = null;

        private Dictionary<string, int> headerMappingMap = null;

        private int m_rows = 0;

        private int m_columns = 0;
    }
}
